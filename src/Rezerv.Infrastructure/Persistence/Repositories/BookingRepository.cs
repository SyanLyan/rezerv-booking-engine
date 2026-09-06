using Microsoft.EntityFrameworkCore;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Application.Services.Bookings;
using Rezerv.Domain.Entities;
using Rezerv.Domain.Enums;

namespace Rezerv.Infrastructure.Persistence.Repositories;

public sealed class BookingRepository(RezervDbContext dbContext) : IBookingRepository
{
    public Task<int?> GetTimetableScheduleIdAsync(
        int bookingId,
        CancellationToken cancellationToken = default) =>
        dbContext.Bookings
            .Where(booking => booking.Id == bookingId)
            .Select(booking => (int?)booking.TimetableScheduleId)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<BookingCandidate?> LoadCandidateAsync(
        int customerId,
        int timetableScheduleId,
        int customerPackageId,
        CancellationToken cancellationToken = default)
    {
        var customerExists = await dbContext.Customers
            .AnyAsync(customer => customer.Id == customerId, cancellationToken);
        var schedule = await dbContext.TimetableSchedules
            .SingleOrDefaultAsync(item => item.Id == timetableScheduleId, cancellationToken);
        var customerPackage = await dbContext.CustomerPackages
            .Include(item => item.Package)
            .SingleOrDefaultAsync(item => item.Id == customerPackageId, cancellationToken);

        if (!customerExists || schedule is null || customerPackage is null)
        {
            return null;
        }

        var activeBookings = await dbContext.Bookings
            .Include(booking => booking.TimetableSchedule)
            .Where(booking => booking.CustomerId == customerId && booking.Status == BookingStatus.Booked)
            .ToListAsync(cancellationToken);

        var hasExistingBooking = await dbContext.Bookings
            .AnyAsync(
                booking =>
                    booking.CustomerId == customerId &&
                    booking.TimetableScheduleId == timetableScheduleId &&
                    (booking.Status == BookingStatus.Booked || booking.Status == BookingStatus.Waitlisted),
                cancellationToken);
        var hasOverlappingBooking = activeBookings.Any(booking =>
            schedule.StartTimeUtc < booking.TimetableSchedule.EndTimeUtc &&
            schedule.EndTimeUtc > booking.TimetableSchedule.StartTimeUtc);

        return new BookingCandidate(
            schedule,
            customerPackage,
            customerPackage.CustomerId == customerId,
            hasExistingBooking,
            hasOverlappingBooking);
    }

    public async Task<BookingCancellationCandidate?> LoadCancellationCandidateAsync(
        int bookingId,
        CancellationToken cancellationToken = default)
    {
        var booking = await dbContext.Bookings
            .Include(item => item.TimetableSchedule)
            .Include(item => item.CustomerPackage)
            .ThenInclude(item => item.Package)
            .SingleOrDefaultAsync(item => item.Id == bookingId, cancellationToken);

        return booking is null ? null : new BookingCancellationCandidate(booking);
    }

    public async Task<IReadOnlyList<WaitlistPromotionCandidate>> ListWaitlistPromotionCandidatesAsync(
        int timetableScheduleId,
        CancellationToken cancellationToken = default)
    {
        var candidates = await dbContext.Bookings
            .Include(booking => booking.CustomerPackage)
            .ThenInclude(customerPackage => customerPackage.Package)
            .Include(booking => booking.TimetableSchedule)
            .Where(booking =>
                booking.TimetableScheduleId == timetableScheduleId &&
                booking.Status == BookingStatus.Waitlisted)
            .OrderBy(booking => booking.CreatedAtUtc)
            .ThenBy(booking => booking.Id)
            .Select(waitlistedBooking => new
            {
                Booking = waitlistedBooking,
                HasOverlappingBooking = dbContext.Bookings.Any(booking =>
                    booking.CustomerId == waitlistedBooking.CustomerId &&
                    booking.Status == BookingStatus.Booked &&
                    booking.TimetableScheduleId != timetableScheduleId &&
                    waitlistedBooking.TimetableSchedule.StartTimeUtc < booking.TimetableSchedule.EndTimeUtc &&
                    waitlistedBooking.TimetableSchedule.EndTimeUtc > booking.TimetableSchedule.StartTimeUtc)
            })
            .ToListAsync(cancellationToken);

        return candidates
            .Select(candidate => new WaitlistPromotionCandidate(
                candidate.Booking,
                candidate.HasOverlappingBooking))
            .ToList();
    }

    public Task<int> DeleteStartedWaitlistEntriesAsync(
        DateTime startedBeforeOrAtUtc,
        CancellationToken cancellationToken = default) =>
        dbContext.Bookings
            .Where(booking =>
                booking.Status == BookingStatus.Waitlisted &&
                booking.TimetableSchedule.StartTimeUtc <= startedBeforeOrAtUtc)
            .ExecuteDeleteAsync(cancellationToken);

    public Task AddAsync(Booking booking, CancellationToken cancellationToken = default) =>
        dbContext.Bookings.AddAsync(booking, cancellationToken).AsTask();
}