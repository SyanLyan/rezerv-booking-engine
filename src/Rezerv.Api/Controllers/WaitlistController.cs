using Microsoft.AspNetCore.Mvc;
using Rezerv.Api.Contracts.Bookings;
using Rezerv.Api.Contracts.Common;
using Rezerv.Application.Commands.Bookings;
using Rezerv.Application.DTOs.Bookings;
using Rezerv.Application.Services.Bookings;

namespace Rezerv.Api.Controllers;

[ApiController]
[Route("api/waitlist")]
public sealed class WaitlistController(IBookingService bookingService) : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookingDto>>> Join(
        JoinWaitlistRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var waitlistEntry = await bookingService.JoinWaitlistAsync(
                new JoinWaitlistCommand(
                    request.CustomerId,
                    request.TimetableScheduleId,
                    request.CustomerPackageId),
                cancellationToken);

            return CreatedResponse(waitlistEntry);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFoundResponse<BookingDto>(exception.Message);
        }
        catch (BookingRuleValidationException exception)
        {
            return BadRequestResponse<BookingDto>(exception.Errors);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequestResponse<BookingDto>(exception.Message);
        }
    }
}