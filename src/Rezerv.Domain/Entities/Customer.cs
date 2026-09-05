using Rezerv.Domain.Common;

namespace Rezerv.Domain.Entities;

public sealed class Customer : Entity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = [];
}