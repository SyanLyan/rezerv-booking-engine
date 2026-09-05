namespace Rezerv.Application.Commands.Bookings;

public sealed record CreateBookingCommand(
    int CustomerId,
    int TimetableScheduleId,
    int CustomerPackageId);