using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rezerv.Domain.Entities;

namespace Rezerv.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(customer => customer.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(customer => customer.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(customer => customer.Email)
            .IsUnique();
    }
}