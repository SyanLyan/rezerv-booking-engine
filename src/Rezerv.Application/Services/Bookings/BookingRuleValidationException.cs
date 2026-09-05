namespace Rezerv.Application.Services.Bookings;

public sealed class BookingRuleValidationException(IReadOnlyList<string> errors)
    : InvalidOperationException("Booking rules were not satisfied.")
{
    public IReadOnlyList<string> Errors { get; } = errors;
}