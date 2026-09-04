using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rezerv.Domain.Entities;

namespace Rezerv.Infrastructure.Persistence.Configurations;

public sealed class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("businesses");

        builder.HasKey(business => business.Id);

        builder.Property(business => business.Name)
            .HasMaxLength(200)
            .IsRequired();
    }
}