namespace Rezerv.Api.Contracts.Bookings;

public sealed class JoinWaitlistRequest
{
    public int CustomerId { get; set; }
    public int TimetableScheduleId { get; set; }
    public int CustomerPackageId { get; set; }
}