namespace Rezerv.Application.Commands.Packages;

public sealed record PurchasePackageCommand(int CustomerId, int PackageId);