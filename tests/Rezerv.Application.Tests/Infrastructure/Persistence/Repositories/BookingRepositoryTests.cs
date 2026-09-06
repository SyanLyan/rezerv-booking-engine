using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Rezerv.Domain.Entities;
using Rezerv.Domain.Enums;
using Rezerv.Infrastructure.Persistence;
using Rezerv.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Rezerv.Application.Tests.Infrastructure.Persistence.Repositories;

public sealed class BookingRepositoryTests
{
    [Fact]
    public async Task DeleteStartedWaitlistEntriesAsync_DeletesOnlyWaitlistedEntriesForStartedSchedules()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<RezervDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new RezervDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var startedSchedule = CreateSchedule(DateTime.UtcNow.AddMinutes(-1));
        var futureSchedule = CreateSchedule(DateTime.UtcNow.AddHours(1));
        var sharedCustomer = new Customer
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = "cleanup-test@example.com"
        };
        var package = new Package
        {
            Business = startedSchedule.Business,
            Name = "Test package",
            Credits = 10,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1)
        };
        var customerPackage = new CustomerPackage
        {
            Customer = sharedCustomer,
            Package = package,
            TotalCredits = 10,
            RemainingCredits = 10
        };
        var startedWaitlistBooking = CreateBooking(sharedCustomer, customerPackage, startedSchedule, BookingStatus.Waitlisted);
        var startedConfirmedBooking = CreateBooking(sharedCustomer, customerPackage, startedSchedule, BookingStatus.Booked);
        var startedCancelledBooking = CreateBooking(sharedCustomer, customerPackage, startedSchedule, BookingStatus.Cancelled);
        var futureWaitlistBooking = CreateBooking(sharedCustomer, customerPackage, futureSchedule, BookingStatus.Waitlisted);

        dbContext.AddRange(
            startedWaitlistBooking,
            startedConfirmedBooking,
            startedCancelledBooking,
            futureWaitlistBooking);
        await dbContext.SaveChangesAsync();
        var repository = new BookingRepository(dbContext);

        var deletedCount = await repository.DeleteStartedWaitlistEntriesAsync(DateTime.UtcNow);
        var remainingBookingIds = await dbContext.Bookings.Select(booking => booking.Id).ToListAsync();

        Assert.Equal(1, deletedCount);
        Assert.DoesNotContain(startedWaitlistBooking.Id, remainingBookingIds);
        Assert.Contains(startedConfirmedBooking.Id, remainingBookingIds);
        Assert.Contains(startedCancelledBooking.Id, remainingBookingIds);
        Assert.Contains(futureWaitlistBooking.Id, remainingBookingIds);
    }

    private static TimetableSchedule CreateSchedule(DateTime startTimeUtc)
    {
        var business = new Business { Name = $"Business {Guid.NewGuid():N}" };

        return new TimetableSchedule
        {
            Business = business,
            ClassName = "Test class",
            Instructor = "Test instructor",
            StartTimeUtc = startTimeUtc,
            EndTimeUtc = startTimeUtc.AddHours(1),
            TotalSlots = 10,
            AvailableSlots = 10
        };
    }

    private static Booking CreateBooking(
        Customer customer,
        CustomerPackage customerPackage,
        TimetableSchedule schedule,
        BookingStatus status) => new()
    {
        Customer = customer,
        CustomerPackage = customerPackage,
        TimetableSchedule = schedule,
        Status = status
    };
}