using Rezerv.Application.Commands.Customers;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Application.DTOs.Customers;
using Rezerv.Domain.Entities;

namespace Rezerv.Application.Services.Customers;

public sealed class CustomerService(IGenericRepository<Customer> customerRepository) : ICustomerService
{
    public async Task<IReadOnlyList<CustomerDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var customers = await customerRepository.ListAsync(cancellationToken);

        return customers
            .OrderBy(customer => customer.LastName)
            .ThenBy(customer => customer.FirstName)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<CustomerDto> CreateAsync(
        CreateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            Email = command.Email.Trim().ToLowerInvariant(),
            CreatedAtUtc = DateTime.UtcNow
        };

        await customerRepository.AddAsync(customer, cancellationToken);
        await customerRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(customer);
    }

    private static CustomerDto MapToDto(Customer customer) =>
        new(customer.Id, customer.FirstName, customer.LastName, customer.Email, customer.CreatedAtUtc);
}