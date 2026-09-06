using Rezerv.Application.Services.Bookings;
using Rezerv.Domain.Entities;

namespace Rezerv.Application.Common.Interfaces;

public interface IBookingRepository
{
    Task<int?> GetTimetableScheduleIdAsync(
        int bookingId,
        CancellationToken cancellationToken = default);

    Task<BookingCandidate?> LoadCandidateAsync(
        int customerId,
        int timetableScheduleId,
        int customerPackageId,
        CancellationToken cancellationToken = default);

    Task<BookingCancellationCandidate?> LoadCancellationCandidateAsync(
        int bookingId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WaitlistPromotionCandidate>> ListWaitlistPromotionCandidatesAsync(
        int timetableScheduleId,
        CancellationToken cancellationToken = default);

    Task<int> DeleteStartedWaitlistEntriesAsync(
        DateTime startedBeforeOrAtUtc,
        CancellationToken cancellationToken = default);

    Task AddAsync(Booking booking, CancellationToken cancellationToken = default);
}