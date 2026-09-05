using Rezerv.Application.Commands.Bookings;
using Rezerv.Application.DTOs.Bookings;

namespace Rezerv.Application.Services.Bookings;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(
        CreateBookingCommand command,
        CancellationToken cancellationToken = default);
}