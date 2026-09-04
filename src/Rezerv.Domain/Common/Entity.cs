namespace Rezerv.Domain.Common;

public abstract class Entity
{
    public int Id { get; protected set; }

    public DateTime CreatedAtUtc { get; set; }
}