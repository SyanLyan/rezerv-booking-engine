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
                ActiveTimetableScheduleId = candidate.Schedule.Id,
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

    public async Task<BookingCancellationDto> CancelAsync(
        int bookingId,
        CancellationToken cancellationToken = default)
    {
        var timetableScheduleId = await bookingRepository.GetTimetableScheduleIdAsync(bookingId, cancellationToken)
            ?? throw new KeyNotFoundException("Booking was not found.");

        await using var bookingLock = await AcquireScheduleLockAsync(timetableScheduleId, cancellationToken);

        var result = await transactionExecutor.ExecuteAsync(async token =>
        {
            var candidate = await bookingRepository.LoadCancellationCandidateAsync(bookingId, token)
                ?? throw new KeyNotFoundException("Booking was not found.");
            var cancelledBooking = candidate.Booking;

            if (cancelledBooking.Status != BookingStatus.Booked)
            {
                throw new InvalidOperationException("Only confirmed bookings can be cancelled.");
            }

            var cancelledAtUtc = DateTime.UtcNow;
            if (cancelledBooking.TimetableSchedule.StartTimeUtc <= cancelledAtUtc)
            {
                throw new InvalidOperationException("Bookings cannot be cancelled after the schedule has started.");
            }

            cancelledBooking.Status = BookingStatus.Cancelled;
            cancelledBooking.CancelledAtUtc = cancelledAtUtc;
            cancelledBooking.ActiveTimetableScheduleId = null;
            cancelledBooking.TimetableSchedule.AvailableSlots += 1;

            var refundEvaluation = await bookingRuleEngine.EvaluateCancellationAsync(
                new BookingCancellationRuleInput(
                    cancelledBooking.TimetableSchedule.StartTimeUtc - cancelledAtUtc >= TimeSpan.FromHours(4)),
                token);

            if (refundEvaluation.ShouldRefund)
            {
                cancelledBooking.CustomerPackage.RemainingCredits += 1;
            }

            var promotedBooking = await PromoteFirstEligibleWaitlistedBookingAsync(
                cancelledBooking.TimetableSchedule,
                token);

            return new BookingCancellationResult(
                cancelledBooking,
                refundEvaluation.ShouldRefund,
                promotedBooking);
        }, cancellationToken);

        await Task.WhenAll(TimetableCacheKeys.AffectedBy(
                result.CancelledBooking.TimetableSchedule.BusinessId,
                result.CancelledBooking.TimetableSchedule.StartTimeUtc)
            .Select(key => cache.RemoveAsync(key, cancellationToken)));

        return new BookingCancellationDto(
            result.CancelledBooking.Id,
            result.CreditRefunded,
            result.PromotedBooking is null ? null : MapToDto(result.PromotedBooking));
    }

    public Task<int> DeleteStartedWaitlistsAsync(CancellationToken cancellationToken = default) =>
        bookingRepository.DeleteStartedWaitlistEntriesAsync(DateTime.UtcNow, cancellationToken);

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

    private async Task<Booking?> PromoteFirstEligibleWaitlistedBookingAsync(
        TimetableSchedule schedule,
        CancellationToken cancellationToken)
    {
        var candidates = await bookingRepository.ListWaitlistPromotionCandidatesAsync(schedule.Id, cancellationToken);

        foreach (var candidate in candidates)
        {
            var waitlistedBooking = candidate.Booking;
            var customerPackage = waitlistedBooking.CustomerPackage;
            var ruleInput = new BookingRuleInput(
                schedule.StartTimeUtc > DateTime.UtcNow,
                schedule.AvailableSlots,
                customerPackage.RemainingCredits > 0,
                customerPackage.Package.ExpiresAtUtc <= DateTime.UtcNow,
                customerPackage.Package.BusinessId == schedule.BusinessId,
                false,
                candidate.HasOverlappingBooking);
            var evaluation = await bookingRuleEngine.EvaluateAsync(ruleInput, cancellationToken);

            if (!evaluation.IsAllowed || customerPackage.CustomerId != waitlistedBooking.CustomerId)
            {
                continue;
            }

            waitlistedBooking.Status = BookingStatus.Booked;
            schedule.AvailableSlots -= 1;
            customerPackage.RemainingCredits -= 1;

            return waitlistedBooking;
        }

        return null;
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

    private sealed record BookingCancellationResult(
        Booking CancelledBooking,
        bool CreditRefunded,
        Booking? PromotedBooking);

    private static BookingDto MapToDto(Booking booking) => new(
        booking.Id,
        booking.CustomerId,
        booking.TimetableScheduleId,
        booking.CustomerPackageId,
        booking.Status,
        booking.CreatedAtUtc);
}