using Rezerv.Application.Commands.Timetable;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Application.DTOs.Timetable;
using Rezerv.Domain.Entities;

namespace Rezerv.Application.Services.Timetable;

public sealed class TimetableService(
    IGenericRepository<TimetableSchedule> timetableScheduleRepository,
    IGenericRepository<Business> businessRepository) : ITimetableService
{
    public async Task<IReadOnlyList<TimetableScheduleDto>> ListAsync(
        int? businessId,
        DateOnly? date,
        CancellationToken cancellationToken = default)
    {
        var schedules = await timetableScheduleRepository.ListAsync(cancellationToken);

        return schedules
            .Where(schedule =>
                (!businessId.HasValue || schedule.BusinessId == businessId.Value) &&
                (!date.HasValue || DateOnly.FromDateTime(schedule.StartTimeUtc) == date.Value))
            .OrderBy(schedule => schedule.StartTimeUtc)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<TimetableScheduleDto> CreateAsync(
        CreateTimetableScheduleCommand command,
        CancellationToken cancellationToken = default)
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

        return MapToDto(schedule);
    }

    private static TimetableScheduleDto MapToDto(TimetableSchedule schedule) => new(
        schedule.Id,
        schedule.BusinessId,
        schedule.ClassName,
        schedule.Instructor,
        schedule.StartTimeUtc,
        schedule.EndTimeUtc,
        schedule.TotalSlots,
        schedule.AvailableSlots);
}