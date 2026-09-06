namespace Rezerv.Application.Commands.Bookings;

public sealed record JoinWaitlistCommand(
    int CustomerId,
    int TimetableScheduleId,
    int CustomerPackageId);