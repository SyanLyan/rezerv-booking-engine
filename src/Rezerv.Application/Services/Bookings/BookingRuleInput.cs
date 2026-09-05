namespace Rezerv.Application.Services.Bookings;

public sealed record BookingRuleInput(
    bool IsScheduleInFuture,
    int AvailableSlots,
    bool HasRemainingPackageCredit,
    bool IsPackageExpired,
    bool HasMatchingBusinessPackage,
    bool HasExistingBooking,
    bool HasOverlappingBooking);