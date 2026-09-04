using Microsoft.AspNetCore.Mvc;
using Rezerv.Api.Contracts.Common;
using Rezerv.Api.Contracts.Timetable;
using Rezerv.Application.Commands.Timetable;
using Rezerv.Application.DTOs.Timetable;
using Rezerv.Application.Services.Timetable;

namespace Rezerv.Api.Controllers;

[ApiController]
[Route("api/timetable")]
public sealed class TimetableController(ITimetableService timetableService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TimetableScheduleDto>>>> Get(
        [FromQuery] int? businessId,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        var schedules = await timetableService.ListAsync(businessId, date, cancellationToken);
        return OkResponse(schedules);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TimetableScheduleDto>>> Create(
        CreateTimetableScheduleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var schedule = await timetableService.CreateAsync(
                new CreateTimetableScheduleCommand(
                    request.BusinessId,
                    request.ClassName,
                    request.Instructor,
                    request.StartTimeUtc,
                    request.EndTimeUtc,
                    request.TotalSlots),
                cancellationToken);

            return CreatedResponse(schedule);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFoundResponse<TimetableScheduleDto>(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequestResponse<TimetableScheduleDto>(exception.Message);
        }
    }
}