using Microsoft.EntityFrameworkCore;
using Rezerv.Domain.Entities;

namespace Rezerv.Infrastructure.Persistence;

public sealed class RezervDbContext(DbContextOptions<RezervDbContext> options) : DbContext(options)
{
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<CustomerPackage> CustomerPackages => Set<CustomerPackage>();
    public DbSet<TimetableSchedule> TimetableSchedules => Set<TimetableSchedule>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RezervDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}