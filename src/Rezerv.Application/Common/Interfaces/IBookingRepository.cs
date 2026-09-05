using Rezerv.Application.Services.Bookings;
using Rezerv.Domain.Entities;

namespace Rezerv.Application.Common.Interfaces;

public interface IBookingRepository
{
    Task<BookingCandidate?> LoadCandidateAsync(
        int customerId,
        int timetableScheduleId,
        int customerPackageId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);
}