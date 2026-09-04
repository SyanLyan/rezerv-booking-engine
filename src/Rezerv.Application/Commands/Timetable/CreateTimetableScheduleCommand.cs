namespace Rezerv.Application.Commands.Timetable;

public sealed record CreateTimetableScheduleCommand(
    int BusinessId,
    string ClassName,
    string Instructor,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    int TotalSlots);