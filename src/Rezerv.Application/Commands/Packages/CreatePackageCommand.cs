namespace Rezerv.Application.Commands.Packages;

public sealed record CreatePackageCommand(
    int BusinessId,
    string Name,
    string? Description,
    int Credits,
    DateTime ExpiresAtUtc);