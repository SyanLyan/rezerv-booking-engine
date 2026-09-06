using Microsoft.AspNetCore.Mvc;
using Rezerv.Api.Contracts.Businesses;
using Rezerv.Api.Contracts.Common;
using Rezerv.Application.Commands.Businesses;
using Rezerv.Application.DTOs.Businesses;
using Rezerv.Application.Services.Businesses;

namespace Rezerv.Api.Controllers;

[ApiController]
[Route("api/businesses")]
public sealed class BusinessesController(IBusinessService businessService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BusinessDto>>>> Get(CancellationToken cancellationToken)
    {
        var businesses = await businessService.ListAsync(cancellationToken);
        return OkResponse(businesses);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BusinessDto>>> Create(
        CreateBusinessRequest request,
        CancellationToken cancellationToken)
    {
        var business = await businessService.CreateAsync(
            new CreateBusinessCommand(request.Name),
            cancellationToken);

        return CreatedResponse(business);
    }
}