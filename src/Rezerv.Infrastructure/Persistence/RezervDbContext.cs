using Microsoft.EntityFrameworkCore;
using Rezerv.Domain.Entities;

namespace Rezerv.Infrastructure.Persistence;

public sealed class RezervDbContext(DbContextOptions<RezervDbContext> options) : DbContext(options)
{
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<CustomerPackage> CustomerPackages => Set<CustomerPackage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RezervDbContext).Assembly);

        var seededAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Business>().HasData(
            new { Id = 1, Name = "Rezerv Fitness", CreatedAtUtc = seededAtUtc },
            new { Id = 2, Name = "Studio Flow", CreatedAtUtc = seededAtUtc });

        modelBuilder.Entity<Customer>().HasData(
            new { Id = 1, FirstName = "Ava", LastName = "Smith", Email = "ava.smith@example.com", CreatedAtUtc = seededAtUtc },
            new { Id = 2, FirstName = "Noah", LastName = "Johnson", Email = "noah.johnson@example.com", CreatedAtUtc = seededAtUtc },
            new { Id = 3, FirstName = "Mia", LastName = "Williams", Email = "mia.williams@example.com", CreatedAtUtc = seededAtUtc },
            new { Id = 4, FirstName = "Liam", LastName = "Brown", Email = "liam.brown@example.com", CreatedAtUtc = seededAtUtc },
            new { Id = 5, FirstName = "Emma", LastName = "Jones", Email = "emma.jones@example.com", CreatedAtUtc = seededAtUtc },
            new { Id = 6, FirstName = "Oliver", LastName = "Garcia", Email = "oliver.garcia@example.com", CreatedAtUtc = seededAtUtc },
            new { Id = 7, FirstName = "Sophia", LastName = "Miller", Email = "sophia.miller@example.com", CreatedAtUtc = seededAtUtc },
            new { Id = 8, FirstName = "Ethan", LastName = "Davis", Email = "ethan.davis@example.com", CreatedAtUtc = seededAtUtc },
            new { Id = 9, FirstName = "Isabella", LastName = "Wilson", Email = "isabella.wilson@example.com", CreatedAtUtc = seededAtUtc },
            new { Id = 10, FirstName = "James", LastName = "Moore", Email = "james.moore@example.com", CreatedAtUtc = seededAtUtc });

        base.OnModelCreating(modelBuilder);
    }
}