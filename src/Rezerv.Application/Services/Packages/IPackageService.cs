using Rezerv.Application.Commands.Packages;
using Rezerv.Application.DTOs.Packages;

namespace Rezerv.Application.Services.Packages;

public interface IPackageService
{
    Task<IReadOnlyList<PackageDto>> ListAsync(int? businessId, CancellationToken cancellationToken = default);

    Task<PackageDto> CreateAsync(CreatePackageCommand command, CancellationToken cancellationToken = default);

    Task<PurchasedPackageDto> PurchaseAsync(PurchasePackageCommand command, CancellationToken cancellationToken = default);
}