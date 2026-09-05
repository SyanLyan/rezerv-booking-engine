using Rezerv.Domain.Common;

namespace Rezerv.Domain.Entities;

public sealed class CustomerPackage : Entity
{
    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public int PackageId { get; set; }

    public Package Package { get; set; } = null!;

    public int TotalCredits { get; set; }

    public int RemainingCredits { get; set; }

    public ICollection<Booking> Bookings { get; set; } = [];
}