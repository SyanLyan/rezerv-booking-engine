using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rezerv.Domain.Entities;

namespace Rezerv.Infrastructure.Persistence.Configurations;

public sealed class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.ToTable("packages");

        builder.HasKey(package => package.Id);

        builder.Property(package => package.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(package => package.Description)
            .HasMaxLength(1000);

        builder.Property(package => package.Credits)
            .IsRequired();

        builder.Property(package => package.ExpiresAtUtc)
            .HasColumnType("datetime")
            .IsRequired();

        builder.HasOne(package => package.Business)
            .WithMany()
            .HasForeignKey(package => package.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(package => new { package.BusinessId, package.Name })
            .IsUnique();
    }
}