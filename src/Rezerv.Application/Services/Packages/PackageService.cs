using Rezerv.Application.Commands.Packages;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Application.DTOs.Packages;
using Rezerv.Domain.Entities;

namespace Rezerv.Application.Services.Packages;

public sealed class PackageService(
    IGenericRepository<Package> packageRepository,
    IGenericRepository<Business> businessRepository,
    IGenericRepository<Customer> customerRepository,
    IGenericRepository<CustomerPackage> customerPackageRepository) : IPackageService
{
    public async Task<IReadOnlyList<PackageDto>> ListAsync(int? businessId, CancellationToken cancellationToken = default)
    {
        var packages = await packageRepository.ListAsync(cancellationToken);

        return packages
            .Where(package =>
                package.IsActive &&
                package.ExpiresAtUtc > DateTime.UtcNow &&
                (!businessId.HasValue || package.BusinessId == businessId.Value))
            .OrderBy(package => package.Name)
            .Select(package => new PackageDto(
                package.Id,
                package.BusinessId,
                package.Name,
                package.Description,
                package.Credits,
                package.ExpiresAtUtc))
            .ToList();
    }

    public async Task<PackageDto> CreateAsync(CreatePackageCommand command, CancellationToken cancellationToken = default)
    {
        var business = await businessRepository.GetByIdAsync(command.BusinessId, cancellationToken)
            ?? throw new KeyNotFoundException("Business was not found.");

        var package = new Package
        {
            BusinessId = business.Id,
            Name = command.Name.Trim(),
            Description = command.Description?.Trim(),
            Credits = command.Credits,
            ExpiresAtUtc = command.ExpiresAtUtc,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await packageRepository.AddAsync(package, cancellationToken);
        await packageRepository.SaveChangesAsync(cancellationToken);

        return new PackageDto(
            package.Id,
            package.BusinessId,
            package.Name,
            package.Description,
            package.Credits,
            package.ExpiresAtUtc);
    }

    public async Task<PurchasedPackageDto> PurchaseAsync(PurchasePackageCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(command.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException("Customer was not found.");

        var package = await packageRepository.GetByIdAsync(command.PackageId, cancellationToken)
            ?? throw new KeyNotFoundException("Package was not found.");

        if (!package.IsActive || package.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Package is not available for purchase.");
        }

        var customerPackage = new CustomerPackage
        {
            CustomerId = customer.Id,
            PackageId = package.Id,
            TotalCredits = package.Credits,
            RemainingCredits = package.Credits,
            CreatedAtUtc = DateTime.UtcNow
        };

        await customerPackageRepository.AddAsync(customerPackage, cancellationToken);
        await customerPackageRepository.SaveChangesAsync(cancellationToken);

        return new PurchasedPackageDto(
            customerPackage.Id,
            customerPackage.CustomerId,
            customerPackage.PackageId,
            customerPackage.TotalCredits,
            customerPackage.RemainingCredits);
    }
}