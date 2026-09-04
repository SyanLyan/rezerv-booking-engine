namespace Rezerv.Application.DTOs.Packages;

public sealed record PurchasedPackageDto(
    int Id,
    int CustomerId,
    int PackageId,
    int TotalCredits,
    int RemainingCredits);