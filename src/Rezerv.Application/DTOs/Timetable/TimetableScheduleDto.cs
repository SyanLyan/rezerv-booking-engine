namespace Rezerv.Application.DTOs.Timetable;

public sealed record TimetableScheduleDto(
    int Id,
    int BusinessId,
    string ClassName,
    string Instructor,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    int TotalSlots,
    int AvailableSlots);