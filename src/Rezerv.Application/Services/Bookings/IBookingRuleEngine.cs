namespace Rezerv.Application.Services.Bookings;

public interface IBookingRuleEngine
{
    Task<BookingRuleEvaluation> EvaluateAsync(
        BookingRuleInput input,
        CancellationToken cancellationToken = default);

    Task<BookingCancellationEvaluation> EvaluateCancellationAsync(
        BookingCancellationRuleInput input,
        CancellationToken cancellationToken = default);
}