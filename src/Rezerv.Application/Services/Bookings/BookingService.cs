using Rezerv.Application.Commands.Bookings;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Application.DTOs.Bookings;
using Rezerv.Application.Services.Timetable;
using Rezerv.Domain.Entities;
using Rezerv.Domain.Enums;

namespace Rezerv.Application.Services.Bookings;

public sealed class BookingService(
    IBookingRepository bookingRepository,
    ITransactionExecutor transactionExecutor,
    IBookingRuleEngine bookingRuleEngine,
    IDistributedLock distributedLock,
    IApplicationCache cache) : IBookingService
{
    private static readonly TimeSpan LockExpiration = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan LockRetryDelay = TimeSpan.FromMilliseconds(50);
    private const int LockRetryCount = 50;

    public async Task<BookingDto> CreateAsync(
        CreateBookingCommand command,
        CancellationToken cancellationToken = default)
    {
        await using var bookingLock = await AcquireScheduleLockAsync(command.TimetableScheduleId, cancellationToken);

        var result = await transactionExecutor.ExecuteAsync(async token =>
        {
            var candidate = await bookingRepository.LoadCandidateAsync(
                command.CustomerId,
                command.TimetableScheduleId,
                command.CustomerPackageId,
                token) ?? throw new KeyNotFoundException("Customer, timetable schedule, or customer package was not found.");

            if (!candidate.IsCustomerPackageOwnedByCustomer)
            {
                throw new InvalidOperationException("The customer package does not belong to the customer.");
            }

            var ruleInput = new BookingRuleInput(
                candidate.Schedule.StartTimeUtc > DateTime.UtcNow,
                candidate.Schedule.AvailableSlots,
                candidate.CustomerPackage.RemainingCredits > 0,
                candidate.CustomerPackage.Package.ExpiresAtUtc <= DateTime.UtcNow,
                candidate.CustomerPackage.Package.BusinessId == candidate.Schedule.BusinessId,
                candidate.HasExistingBooking,
                candidate.HasOverlappingBooking);

            var status = candidate.Schedule.AvailableSlots > 0
                ? await ValidateBookingAsync(ruleInput, token)
                : await ValidateWaitlistAsync(ruleInput, token);

            if (status == BookingStatus.Booked)
            {
                candidate.Schedule.AvailableSlots -= 1;
                candidate.CustomerPackage.RemainingCredits -= 1;
            }

            var createdBooking = new Booking
            {
                CustomerId = command.CustomerId,
                TimetableScheduleId = candidate.Schedule.Id,
                CustomerPackageId = candidate.CustomerPackage.Id,
                Status = status,
                CreatedAtUtc = DateTime.UtcNow
            };

            await bookingRepository.AddAsync(createdBooking, token);

            return new BookingCreationResult(
                createdBooking,
                candidate.Schedule.BusinessId,
                candidate.Schedule.StartTimeUtc);
        }, cancellationToken);

        await Task.WhenAll(TimetableCacheKeys.AffectedBy(result.BusinessId, result.StartTimeUtc)
            .Select(key => cache.RemoveAsync(key, cancellationToken)));

        return new BookingDto(
            result.Booking.Id,
            result.Booking.CustomerId,
            result.Booking.TimetableScheduleId,
            result.Booking.CustomerPackageId,
            result.Booking.Status,
            result.Booking.CreatedAtUtc);
    }

    private async Task<BookingStatus> ValidateBookingAsync(BookingRuleInput input, CancellationToken cancellationToken)
    {
        var evaluation = await bookingRuleEngine.EvaluateAsync(input, cancellationToken);
        if (!evaluation.IsAllowed)
        {
            throw new BookingRuleValidationException(evaluation.Failures);
        }

        return BookingStatus.Booked;
    }

    private async Task<BookingStatus> ValidateWaitlistAsync(BookingRuleInput input, CancellationToken cancellationToken)
    {
        var evaluation = await bookingRuleEngine.EvaluateWaitlistAsync(input, cancellationToken);
        if (!evaluation.IsAllowed)
        {
            throw new BookingRuleValidationException(evaluation.Failures);
        }

        return BookingStatus.Waitlisted;
    }

    private async Task<IAsyncDisposable> AcquireScheduleLockAsync(int timetableScheduleId, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < LockRetryCount; attempt++)
        {
            var bookingLock = await distributedLock.TryAcquireAsync(
                $"lock:timetable:{timetableScheduleId}",
                LockExpiration,
                cancellationToken);

            if (bookingLock is not null)
            {
                return bookingLock;
            }

            await Task.Delay(LockRetryDelay, cancellationToken);
        }

        throw new InvalidOperationException("The timetable schedule is busy. Please try again.");
    }

    private sealed record BookingCreationResult(
        Booking Booking,
        int BusinessId,
        DateTime StartTimeUtc);
}