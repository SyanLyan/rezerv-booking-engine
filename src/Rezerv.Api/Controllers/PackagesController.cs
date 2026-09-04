using Microsoft.AspNetCore.Mvc;
using Rezerv.Api.Contracts.Common;
using Rezerv.Api.Contracts.Packages;
using Rezerv.Application.Commands.Packages;
using Rezerv.Application.DTOs.Packages;
using Rezerv.Application.Services.Packages;

namespace Rezerv.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PackagesController(IPackageService packageService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PackageDto>>>> Get(
        [FromQuery] int? businessId,
        CancellationToken cancellationToken)
    {
        var packages = await packageService.ListAsync(businessId, cancellationToken);
        return OkResponse(packages);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PackageDto>>> Create(
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

            return CreatedResponse(package);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFoundResponse<PackageDto>(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequestResponse<PackageDto>(exception.Message);
        }
    }

    [HttpPost("purchase")]
    public async Task<ActionResult<ApiResponse<PurchasedPackageDto>>> Purchase(
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

            return CreatedResponse(purchasedPackage);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFoundResponse<PurchasedPackageDto>(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequestResponse<PurchasedPackageDto>(exception.Message);
        }
    }
}