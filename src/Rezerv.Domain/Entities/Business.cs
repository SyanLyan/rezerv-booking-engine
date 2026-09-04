using Rezerv.Domain.Common;

namespace Rezerv.Domain.Entities;

public sealed class Business : Entity
{
    public string Name { get; set; } = null!;
}