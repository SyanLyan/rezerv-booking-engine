using Rezerv.Domain.Common;

namespace Rezerv.Domain.Entities;

public sealed class Package : Entity
{
    public int BusinessId { get; set; }

    public Business Business { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Credits { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsActive { get; set; } = true;
}