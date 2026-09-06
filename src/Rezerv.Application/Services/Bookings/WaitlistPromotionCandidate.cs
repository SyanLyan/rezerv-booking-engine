using Rezerv.Domain.Entities;

namespace Rezerv.Application.Services.Bookings;

public sealed record WaitlistPromotionCandidate(
    Booking Booking,
    bool HasOverlappingBooking);