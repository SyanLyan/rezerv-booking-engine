namespace Rezerv.Api.Contracts.Packages;

public sealed class CreatePackageRequest
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}