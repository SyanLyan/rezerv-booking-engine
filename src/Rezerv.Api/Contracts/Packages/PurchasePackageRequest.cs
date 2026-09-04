namespace Rezerv.Api.Contracts.Packages;

public sealed class PurchasePackageRequest
{
    public int CustomerId { get; set; }

    public int PackageId { get; set; }
}