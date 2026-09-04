namespace Rezerv.Application.Services.Bookings;

public sealed record BookingCancellationRuleInput(
    bool IsAtLeastFourHoursBeforeSchedule);