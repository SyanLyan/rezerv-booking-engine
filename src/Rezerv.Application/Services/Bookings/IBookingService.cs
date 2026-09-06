using Rezerv.Application.Commands.Bookings;
using Rezerv.Application.DTOs.Bookings;

namespace Rezerv.Application.Services.Bookings;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(
        CreateBookingCommand command,
        CancellationToken cancellationToken = default);

    Task<BookingCancellationDto> CancelAsync(
        int bookingId,
        CancellationToken cancellationToken = default);

    Task<int> DeleteStartedWaitlistsAsync(CancellationToken cancellationToken = default);
}