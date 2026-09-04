namespace Rezerv.Application.DTOs.Packages;

public sealed record PackageDto(
    int Id,
    int BusinessId,
    string Name,
    string? Description,
    int Credits,
    DateTime ExpiresAtUtc);