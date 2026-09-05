namespace Rezerv.Application.DTOs.Timetable;

public sealed record TimetableScheduleDto(
    int Id,
    int BusinessId,
    string BusinessName,
    string ClassName,
    string Instructor,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    int TotalSlots,
    int AvailableSlots,
    int AttendanceCount);