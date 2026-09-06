using Rezerv.Domain.Common;
using Rezerv.Domain.Enums;

namespace Rezerv.Domain.Entities;

public sealed class Booking : Entity
{
    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public int TimetableScheduleId { get; set; }

    public TimetableSchedule TimetableSchedule { get; set; } = null!;

    public int? ActiveTimetableScheduleId { get; set; }

    public int CustomerPackageId { get; set; }

    public CustomerPackage CustomerPackage { get; set; } = null!;

    public BookingStatus Status { get; set; }

    public DateTime? CancelledAtUtc { get; set; }
}