using Microsoft.EntityFrameworkCore;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Application.Services.Bookings;
using Rezerv.Domain.Entities;
using Rezerv.Domain.Enums;

namespace Rezerv.Infrastructure.Persistence.Repositories;

public sealed class BookingRepository(RezervDbContext dbContext) : IBookingRepository
{
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
                    booking.Status != BookingStatus.Cancelled,
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

    public Task AddAsync(Booking booking, CancellationToken cancellationToken = default) =>
        dbContext.Bookings.AddAsync(booking, cancellationToken).AsTask();
}