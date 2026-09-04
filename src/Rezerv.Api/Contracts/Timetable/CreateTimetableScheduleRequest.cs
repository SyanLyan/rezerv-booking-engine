namespace Rezerv.Api.Contracts.Timetable;

public sealed class CreateTimetableScheduleRequest
{
    public int BusinessId { get; set; }

    public string ClassName { get; set; } = null!;

    public string Instructor { get; set; } = null!;

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }

    public int TotalSlots { get; set; }
}