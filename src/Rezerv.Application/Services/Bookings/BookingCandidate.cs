using Rezerv.Domain.Entities;

namespace Rezerv.Application.Services.Bookings;

public sealed record BookingCandidate(
    TimetableSchedule Schedule,
    CustomerPackage CustomerPackage,
    bool IsCustomerPackageOwnedByCustomer,
    bool HasExistingBooking,
    bool HasOverlappingBooking);