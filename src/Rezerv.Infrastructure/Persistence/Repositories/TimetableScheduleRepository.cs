using Microsoft.EntityFrameworkCore;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Application.DTOs.Timetable;
using Rezerv.Domain.Enums;

namespace Rezerv.Infrastructure.Persistence.Repositories;

public sealed class TimetableScheduleRepository(RezervDbContext dbContext) : ITimetableScheduleRepository
{
    public async Task<IReadOnlyList<TimetableScheduleDto>> ListAsync(
        int? businessId,
        DateOnly? date,
        CancellationToken cancellationToken = default)
    {
        var schedules = dbContext.TimetableSchedules
            .AsNoTracking()
            .AsQueryable();

        if (businessId.HasValue)
        {
            schedules = schedules.Where(schedule => schedule.BusinessId == businessId.Value);
        }

        if (date.HasValue)
        {
            var dayStartUtc = date.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var nextDayStartUtc = dayStartUtc.AddDays(1);

            schedules = schedules.Where(schedule =>
                schedule.StartTimeUtc >= dayStartUtc &&
                schedule.StartTimeUtc < nextDayStartUtc);
        }

        return await schedules
            .OrderBy(schedule => schedule.StartTimeUtc)
            .Select(schedule => new TimetableScheduleDto(
                schedule.Id,
                schedule.BusinessId,
                schedule.Business.Name,
                schedule.ClassName,
                schedule.Instructor,
                schedule.StartTimeUtc,
                schedule.EndTimeUtc,
                schedule.TotalSlots,
                schedule.AvailableSlots,
                schedule.Bookings.Count(booking => booking.Status == BookingStatus.Booked)))
            .ToListAsync(cancellationToken);
    }
}