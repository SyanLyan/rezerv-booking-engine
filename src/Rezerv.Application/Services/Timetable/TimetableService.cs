using Rezerv.Application.Commands.Timetable;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Application.DTOs.Timetable;
using Rezerv.Domain.Entities;

namespace Rezerv.Application.Services.Timetable;

public sealed class TimetableService(
    IGenericRepository<TimetableSchedule> timetableScheduleRepository,
    IGenericRepository<Business> businessRepository,
    ITimetableScheduleRepository timetableScheduleReadRepository,
    IApplicationCache cache) : ITimetableService
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(1);

    public async Task<IReadOnlyList<TimetableScheduleDto>> ListAsync(int? businessId, DateOnly? date, CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(
            TimetableCacheKeys.List(businessId, date),
            CacheExpiration,
            token => timetableScheduleReadRepository.ListAsync(businessId, date, token),
            cancellationToken);
    }

    public async Task<TimetableScheduleDto> CreateAsync(CreateTimetableScheduleCommand command,CancellationToken cancellationToken = default)
    {
        var business = await businessRepository.GetByIdAsync(command.BusinessId, cancellationToken)
            ?? throw new KeyNotFoundException("Business was not found.");

        var schedule = new TimetableSchedule
        {
            BusinessId = business.Id,
            ClassName = command.ClassName.Trim(),
            Instructor = command.Instructor.Trim(),
            StartTimeUtc = command.StartTimeUtc,
            EndTimeUtc = command.EndTimeUtc,
            TotalSlots = command.TotalSlots,
            AvailableSlots = command.TotalSlots,
            CreatedAtUtc = DateTime.UtcNow
        };

        await timetableScheduleRepository.AddAsync(schedule, cancellationToken);
        await timetableScheduleRepository.SaveChangesAsync(cancellationToken);
        await Task.WhenAll(TimetableCacheKeys.AffectedBy(schedule.BusinessId, schedule.StartTimeUtc)
            .Select(key => cache.RemoveAsync(key, cancellationToken)));

        return MapToDto(schedule, business.Name, 0);
    }

    private static TimetableScheduleDto MapToDto(
        TimetableSchedule schedule,
        string businessName,
        int attendanceCount) => new(
        schedule.Id,
        schedule.BusinessId,
        businessName,
        schedule.ClassName,
        schedule.Instructor,
        schedule.StartTimeUtc,
        schedule.EndTimeUtc,
        schedule.TotalSlots,
        schedule.AvailableSlots,
        attendanceCount);

}