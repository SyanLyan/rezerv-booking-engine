using Rezerv.Application.DTOs.Timetable;

namespace Rezerv.Application.Common.Interfaces;

public interface ITimetableScheduleRepository
{
    Task<IReadOnlyList<TimetableScheduleDto>> ListAsync(
        int? businessId,
        DateOnly? date,
        CancellationToken cancellationToken = default);
}