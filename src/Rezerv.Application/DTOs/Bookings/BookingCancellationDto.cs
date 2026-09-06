namespace Rezerv.Application.DTOs.Bookings;

public sealed record BookingCancellationDto(
    int CancelledBookingId,
    bool CreditRefunded,
    BookingDto? PromotedBooking);