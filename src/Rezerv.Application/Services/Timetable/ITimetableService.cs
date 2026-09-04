using Rezerv.Application.Commands.Timetable;
using Rezerv.Application.DTOs.Timetable;

namespace Rezerv.Application.Services.Timetable;

public interface ITimetableService
{
    Task<IReadOnlyList<TimetableScheduleDto>> ListAsync(
        int? businessId,
        DateOnly? date,
        CancellationToken cancellationToken = default);

    Task<TimetableScheduleDto> CreateAsync(
        CreateTimetableScheduleCommand command,
        CancellationToken cancellationToken = default);
}