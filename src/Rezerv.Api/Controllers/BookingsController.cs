using Microsoft.AspNetCore.Mvc;
using Rezerv.Api.Contracts.Bookings;
using Rezerv.Api.Contracts.Common;
using Rezerv.Application.Commands.Bookings;
using Rezerv.Application.DTOs.Bookings;
using Rezerv.Application.Services.Bookings;

namespace Rezerv.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController(IBookingService bookingService) : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookingDto>>> Create(
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var booking = await bookingService.CreateAsync(
                new CreateBookingCommand(
                    request.CustomerId,
                    request.TimetableScheduleId,
                    request.CustomerPackageId),
                cancellationToken);

            return CreatedResponse(booking);
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