using Microsoft.AspNetCore.Mvc;
using Rezerv.Api.Contracts.Packages;
using Rezerv.Application.Commands.Packages;
using Rezerv.Application.DTOs.Packages;
using Rezerv.Application.Services.Packages;

namespace Rezerv.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PackagesController(IPackageService packageService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PackageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PackageDto>>> Get(
        [FromQuery] int? businessId,
        CancellationToken cancellationToken)
    {
        var packages = await packageService.ListAsync(businessId, cancellationToken);
        return Ok(packages);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PackageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PackageDto>> Create(
        CreatePackageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var package = await packageService.CreateAsync(
                new CreatePackageCommand(
                    request.BusinessId,
                    request.Name,
                    request.Description,
                    request.Credits,
                    request.ExpiresAtUtc),
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, package);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ProblemDetails { Detail = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ProblemDetails { Detail = exception.Message });
        }
    }

    [HttpPost("purchase")]
    [ProducesResponseType(typeof(PurchasedPackageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchasedPackageDto>> Purchase(
        PurchasePackageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var purchasedPackage = await packageService.PurchaseAsync(
                new PurchasePackageCommand(
                    request.CustomerId,
                    request.PackageId),
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, purchasedPackage);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ProblemDetails { Detail = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ProblemDetails { Detail = exception.Message });
        }
    }
}