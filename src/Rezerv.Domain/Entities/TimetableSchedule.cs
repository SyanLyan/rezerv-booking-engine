using Rezerv.Domain.Common;

namespace Rezerv.Domain.Entities;

public sealed class TimetableSchedule : Entity
{
    public int BusinessId { get; set; }

    public Business Business { get; set; } = null!;

    public string ClassName { get; set; } = null!;

    public string Instructor { get; set; } = null!;

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }

    public int TotalSlots { get; set; }

    public int AvailableSlots { get; set; }
}