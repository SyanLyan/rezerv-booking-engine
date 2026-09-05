using Rezerv.Domain.Enums;

namespace Rezerv.Application.DTOs.Bookings;

public sealed record BookingDto(
    int Id,
    int CustomerId,
    int TimetableScheduleId,
    int CustomerPackageId,
    BookingStatus Status,
    DateTime CreatedAtUtc);