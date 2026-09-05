using Rezerv.Application.Commands.Packages;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Application.DTOs.Packages;
using Rezerv.Domain.Entities;

namespace Rezerv.Application.Services.Packages;

public sealed class PackageService(
    IGenericRepository<Package> packageRepository,
    IGenericRepository<Business> businessRepository,
    IGenericRepository<Customer> customerRepository,
    IGenericRepository<CustomerPackage> customerPackageRepository,
    IApplicationCache cache) : IPackageService
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(1);

    public async Task<IReadOnlyList<PackageDto>> ListAsync(int? businessId, CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(
            GetCacheKey(businessId),
            CacheExpiration,
            async token =>
            {
                var packages = await packageRepository.ListAsync(token);

                return packages
                    .Where(package =>
                        package.IsActive &&
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
            },
            cancellationToken);
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
        await Task.WhenAll(
            cache.RemoveAsync(GetCacheKey(null), cancellationToken),
            cache.RemoveAsync(GetCacheKey(package.BusinessId), cancellationToken));

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

    private static string GetCacheKey(int? businessId) =>
        $"packages:business:{businessId?.ToString() ?? "all"}";
}