using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rezerv.Domain.Entities;

namespace Rezerv.Infrastructure.Persistence.Configurations;

public sealed class CustomerPackageConfiguration : IEntityTypeConfiguration<CustomerPackage>
{
    public void Configure(EntityTypeBuilder<CustomerPackage> builder)
    {
        builder.ToTable("customer_packages");

        builder.HasKey(customerPackage => customerPackage.Id);

        builder.HasOne(customerPackage => customerPackage.Customer)
            .WithMany()
            .HasForeignKey(customerPackage => customerPackage.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(customerPackage => customerPackage.Package)
            .WithMany()
            .HasForeignKey(customerPackage => customerPackage.PackageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(customerPackage => customerPackage.CustomerId);
        builder.HasIndex(customerPackage => customerPackage.PackageId);
    }
}