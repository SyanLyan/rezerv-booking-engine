using Rezerv.Application.Commands.Customers;
using Rezerv.Application.DTOs.Customers;

namespace Rezerv.Application.Services.Customers;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<CustomerDto> CreateAsync(
        CreateCustomerCommand command,
        CancellationToken cancellationToken = default);
}