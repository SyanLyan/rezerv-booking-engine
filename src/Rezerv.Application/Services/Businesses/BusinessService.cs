using Rezerv.Application.Commands.Businesses;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Application.DTOs.Businesses;
using Rezerv.Domain.Entities;

namespace Rezerv.Application.Services.Businesses;

public sealed class BusinessService(IGenericRepository<Business> businessRepository) : IBusinessService
{
    public async Task<IReadOnlyList<BusinessDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var businesses = await businessRepository.ListAsync(cancellationToken);

        return businesses
            .OrderBy(business => business.Name)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<BusinessDto> CreateAsync(
        CreateBusinessCommand command,
        CancellationToken cancellationToken = default)
    {
        var business = new Business
        {
            Name = command.Name.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        await businessRepository.AddAsync(business, cancellationToken);
        await businessRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(business);
    }

    private static BusinessDto MapToDto(Business business) =>
        new(business.Id, business.Name, business.CreatedAtUtc);
}