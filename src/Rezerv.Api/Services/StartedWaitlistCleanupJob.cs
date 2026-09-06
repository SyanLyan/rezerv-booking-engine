using Rezerv.Application.Services.Bookings;

namespace Rezerv.Api.Services;

public sealed class StartedWaitlistCleanupJob(IBookingService bookingService)
{
    public Task ExecuteAsync() => bookingService.DeleteStartedWaitlistsAsync();
}