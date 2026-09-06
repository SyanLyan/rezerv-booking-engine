using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rezerv.Domain.Entities;

namespace Rezerv.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(booking => booking.Id);

        builder.Property(booking => booking.Status)
            .IsRequired();

        builder.HasOne(booking => booking.Customer)
            .WithMany(customer => customer.Bookings)
            .HasForeignKey(booking => booking.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(booking => booking.TimetableSchedule)
            .WithMany(schedule => schedule.Bookings)
            .HasForeignKey(booking => booking.TimetableScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(booking => booking.CustomerPackage)
            .WithMany(customerPackage => customerPackage.Bookings)
            .HasForeignKey(booking => booking.CustomerPackageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(booking => new { booking.CustomerId, booking.ActiveTimetableScheduleId })
            .IsUnique();

        builder.HasIndex(booking => new { booking.TimetableScheduleId, booking.Status, booking.CreatedAtUtc });
    }
}