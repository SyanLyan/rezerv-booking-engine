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

    [HttpPost("{bookingId:int}/cancel")]
    public async Task<ActionResult<ApiResponse<BookingCancellationDto>>> Cancel(
        int bookingId,
        CancellationToken cancellationToken)
    {
        try
        {
            var cancellation = await bookingService.CancelAsync(bookingId, cancellationToken);
            return UpdatedResponse(cancellation);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFoundResponse<BookingCancellationDto>(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequestResponse<BookingCancellationDto>(exception.Message);
        }
    }
}