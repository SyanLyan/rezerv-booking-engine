namespace Rezerv.Application.Services.Bookings;

public sealed record BookingCancellationEvaluation(
    bool ShouldRefund,
    string? Reason);