using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rezerv.Domain.Entities;

namespace Rezerv.Infrastructure.Persistence.Configurations;

public sealed class TimetableScheduleConfiguration : IEntityTypeConfiguration<TimetableSchedule>
{
    public void Configure(EntityTypeBuilder<TimetableSchedule> builder)
    {
        builder.ToTable("timetable_schedules", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_timetable_schedules_slots", "`TotalSlots` > 0 AND `AvailableSlots` >= 0 AND `AvailableSlots` <= `TotalSlots`");
            tableBuilder.HasCheckConstraint("CK_timetable_schedules_time", "`EndTimeUtc` > `StartTimeUtc`");
        });

        builder.HasKey(schedule => schedule.Id);

        builder.Property(schedule => schedule.ClassName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(schedule => schedule.Instructor)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(schedule => schedule.StartTimeUtc)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(schedule => schedule.EndTimeUtc)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(schedule => schedule.TotalSlots)
            .IsRequired();

        builder.Property(schedule => schedule.AvailableSlots)
            .IsRequired();

        builder.HasOne(schedule => schedule.Business)
            .WithMany()
            .HasForeignKey(schedule => schedule.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(schedule => new { schedule.BusinessId, schedule.StartTimeUtc });
    }
}