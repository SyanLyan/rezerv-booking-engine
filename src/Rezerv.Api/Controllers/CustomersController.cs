using Microsoft.AspNetCore.Mvc;
using Rezerv.Api.Contracts.Common;
using Rezerv.Api.Contracts.Customers;
using Rezerv.Application.Commands.Customers;
using Rezerv.Application.DTOs.Customers;
using Rezerv.Application.Services.Customers;

namespace Rezerv.Api.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(ICustomerService customerService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CustomerDto>>>> Get(CancellationToken cancellationToken)
    {
        var customers = await customerService.ListAsync(cancellationToken);
        return OkResponse(customers);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await customerService.CreateAsync(
            new CreateCustomerCommand(request.FirstName, request.LastName, request.Email),
            cancellationToken);

        return CreatedResponse(customer);
    }
}