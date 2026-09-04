namespace Rezerv.Application.Services.Bookings;

public sealed record BookingRuleEvaluation(
    bool IsAllowed,
    IReadOnlyList<string> Failures);