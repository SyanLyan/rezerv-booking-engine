using Rezerv.Application.Commands.Bookings;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Application.Services.Bookings;
using Rezerv.Domain.Entities;
using Rezerv.Domain.Enums;
using Xunit;

namespace Rezerv.Application.Tests.Services.Bookings;

public sealed class BookingServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenScheduleIsFull_DoesNotChangeSlotsOrCredits()
    {
        var candidate = CreateCandidate(availableSlots: 0, remainingCredits: 1);
        var repository = new RecordingBookingRepository { Candidate = candidate };
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(new CreateBookingCommand(1, 1, 1)));

        Assert.Equal("Schedule is full. Please join the waitlist.", exception.Message);
        Assert.Equal(0, candidate.Schedule.AvailableSlots);
        Assert.Equal(1, candidate.CustomerPackage.RemainingCredits);
        Assert.Empty(repository.AddedBookings);
    }

    [Fact]
    public async Task JoinWaitlistAsync_WhenScheduleIsFull_CreatesWaitlistedBooking()
    {
        var candidate = CreateCandidate(availableSlots: 0, remainingCredits: 1);
        var repository = new RecordingBookingRepository { Candidate = candidate };
        var service = CreateService(repository);

        var booking = await service.JoinWaitlistAsync(new JoinWaitlistCommand(1, 1, 1));

        Assert.Equal(BookingStatus.Waitlisted, booking.Status);
        var addedBooking = Assert.Single(repository.AddedBookings);
        Assert.Equal(BookingStatus.Waitlisted, addedBooking.Status);
        Assert.Equal(candidate.Schedule.Id, addedBooking.ActiveTimetableScheduleId);
        Assert.Equal(1, candidate.CustomerPackage.RemainingCredits);
    }

    [Fact]
    public async Task JoinWaitlistAsync_WhenScheduleHasAvailability_RejectsWaitlistEntry()
    {
        var candidate = CreateCandidate(availableSlots: 1, remainingCredits: 1);
        var repository = new RecordingBookingRepository { Candidate = candidate };
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.JoinWaitlistAsync(new JoinWaitlistCommand(1, 1, 1)));

        Assert.Equal("The schedule has available slots. Create a booking instead.", exception.Message);
        Assert.Empty(repository.AddedBookings);
    }

    [Fact]
    public async Task CancelAsync_WhenAtLeastFourHoursBeforeSchedule_RefundsCredit()
    {
        var booking = CreateBookedBooking(DateTime.UtcNow.AddHours(5), remainingCredits: 4);
        var repository = new RecordingBookingRepository { CancellationCandidate = new BookingCancellationCandidate(booking) };
        var service = CreateService(repository);

        var result = await service.CancelAsync(1);

        Assert.True(result.CreditRefunded);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Null(booking.ActiveTimetableScheduleId);
        Assert.NotNull(booking.CancelledAtUtc);
        Assert.Equal(5, booking.CustomerPackage.RemainingCredits);
        Assert.Equal(1, booking.TimetableSchedule.AvailableSlots);
    }

    [Fact]
    public async Task CancelAsync_WhenLessThanFourHoursBeforeSchedule_DoesNotRefundCredit()
    {
        var booking = CreateBookedBooking(DateTime.UtcNow.AddHours(3), remainingCredits: 4);
        var repository = new RecordingBookingRepository { CancellationCandidate = new BookingCancellationCandidate(booking) };
        var service = CreateService(repository);

        var result = await service.CancelAsync(1);

        Assert.False(result.CreditRefunded);
        Assert.Equal(4, booking.CustomerPackage.RemainingCredits);
        Assert.Equal(1, booking.TimetableSchedule.AvailableSlots);
    }

    [Fact]
    public async Task CancelAsync_WhenScheduleHasStarted_RejectsCancellationWithoutChangingState()
    {
        var booking = CreateBookedBooking(DateTime.UtcNow.AddMinutes(-1), remainingCredits: 4);
        var repository = new RecordingBookingRepository { CancellationCandidate = new BookingCancellationCandidate(booking) };
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CancelAsync(1));

        Assert.Equal("Bookings cannot be cancelled after the schedule has started.", exception.Message);
        Assert.Equal(BookingStatus.Booked, booking.Status);
        Assert.Equal(1, booking.ActiveTimetableScheduleId);
        Assert.Null(booking.CancelledAtUtc);
        Assert.Equal(4, booking.CustomerPackage.RemainingCredits);
        Assert.Equal(0, booking.TimetableSchedule.AvailableSlots);
    }

    [Fact]
    public async Task CancelAsync_PromotesTheFirstEligibleWaitlistedBooking()
    {
        var cancelledBooking = CreateBookedBooking(DateTime.UtcNow.AddHours(5), remainingCredits: 4);
        var firstWaitlistedBooking = CreateWaitlistedBooking(customerId: 2, remainingCredits: 2);
        var secondWaitlistedBooking = CreateWaitlistedBooking(customerId: 3, remainingCredits: 2);
        var repository = new RecordingBookingRepository
        {
            CancellationCandidate = new BookingCancellationCandidate(cancelledBooking),
            WaitlistCandidates =
            [
                new WaitlistPromotionCandidate(firstWaitlistedBooking, HasOverlappingBooking: false),
                new WaitlistPromotionCandidate(secondWaitlistedBooking, HasOverlappingBooking: false)
            ]
        };
        var service = CreateService(repository);

        var result = await service.CancelAsync(1);

        Assert.NotNull(result.PromotedBooking);
        Assert.Equal(2, result.PromotedBooking.CustomerId);
        Assert.Equal(BookingStatus.Booked, firstWaitlistedBooking.Status);
        Assert.Equal(1, firstWaitlistedBooking.CustomerPackage.RemainingCredits);
        Assert.Equal(BookingStatus.Waitlisted, secondWaitlistedBooking.Status);
        Assert.Equal(2, secondWaitlistedBooking.CustomerPackage.RemainingCredits);
        Assert.Equal(0, cancelledBooking.TimetableSchedule.AvailableSlots);
    }

    [Fact]
    public async Task CancelAsync_WhenFirstWaitlistedBookingIsIneligible_PromotesTheNextEligibleBooking()
    {
        var cancelledBooking = CreateBookedBooking(DateTime.UtcNow.AddHours(5), remainingCredits: 4);
        var ineligibleBooking = CreateWaitlistedBooking(customerId: 2, remainingCredits: 0);
        var eligibleBooking = CreateWaitlistedBooking(customerId: 3, remainingCredits: 2);
        var repository = new RecordingBookingRepository
        {
            CancellationCandidate = new BookingCancellationCandidate(cancelledBooking),
            WaitlistCandidates =
            [
                new WaitlistPromotionCandidate(ineligibleBooking, HasOverlappingBooking: false),
                new WaitlistPromotionCandidate(eligibleBooking, HasOverlappingBooking: false)
            ]
        };
        var service = CreateService(repository);

        var result = await service.CancelAsync(1);

        Assert.NotNull(result.PromotedBooking);
        Assert.Equal(3, result.PromotedBooking.CustomerId);
        Assert.Equal(BookingStatus.Waitlisted, ineligibleBooking.Status);
        Assert.Equal(BookingStatus.Booked, eligibleBooking.Status);
        Assert.Equal(1, eligibleBooking.CustomerPackage.RemainingCredits);
    }

    [Fact]
    public async Task DeleteStartedWaitlistsAsync_DelegatesCleanupToRepository()
    {
        var repository = new RecordingBookingRepository { DeletedWaitlistEntryCount = 2 };
        var service = CreateService(repository);
        var beforeCleanup = DateTime.UtcNow;

        var deletedCount = await service.DeleteStartedWaitlistsAsync();

        Assert.Equal(2, deletedCount);
        Assert.NotNull(repository.StartedBeforeOrAtUtc);
        Assert.InRange(repository.StartedBeforeOrAtUtc.Value, beforeCleanup, DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestsCompeteForFinalSlot_CreatesExactlyOneBooking()
    {
        var candidate = CreateCandidate(availableSlots: 1, remainingCredits: 2);
        var firstCandidateLoaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var continueFirstRequest = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var repository = new RecordingBookingRepository
        {
            Candidate = candidate,
            FirstCandidateLoaded = firstCandidateLoaded,
            ContinueFirstCandidate = continueFirstRequest
        };
        var service = new BookingService(
            repository,
            new ImmediateTransactionExecutor(),
            new BookingRuleEngine(),
            new SerializedLockProvider(),
            new NoOpCache());

        var firstRequest = service.CreateAsync(new CreateBookingCommand(1, 1, 1));
        await firstCandidateLoaded.Task;
        var secondRequest = service.CreateAsync(new CreateBookingCommand(2, 1, 2));
        continueFirstRequest.SetResult();

        await firstRequest;
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => secondRequest);

        Assert.Equal("Schedule is full. Please join the waitlist.", exception.Message);
        Assert.Single(repository.AddedBookings);
        Assert.Equal(0, candidate.Schedule.AvailableSlots);
        Assert.Equal(1, candidate.CustomerPackage.RemainingCredits);
    }

    private static BookingService CreateService(RecordingBookingRepository repository) => new(
        repository,
        new ImmediateTransactionExecutor(),
        new BookingRuleEngine(),
        new AcquiredLockProvider(),
        new NoOpCache());

    private static BookingCandidate CreateCandidate(int availableSlots, int remainingCredits)
    {
        var schedule = new TimetableSchedule
        {
            BusinessId = 1,
            StartTimeUtc = DateTime.UtcNow.AddHours(5),
            EndTimeUtc = DateTime.UtcNow.AddHours(6),
            AvailableSlots = availableSlots
        };
        var customerPackage = new CustomerPackage
        {
            CustomerId = 1,
            RemainingCredits = remainingCredits,
            Package = new Package { BusinessId = 1, ExpiresAtUtc = DateTime.UtcNow.AddDays(1) }
        };

        return new BookingCandidate(schedule, customerPackage, true, false, false);
    }

    private static Booking CreateBookedBooking(DateTime startTimeUtc, int remainingCredits)
    {
        var schedule = new TimetableSchedule
        {
            BusinessId = 1,
            StartTimeUtc = startTimeUtc,
            EndTimeUtc = startTimeUtc.AddHours(1),
            AvailableSlots = 0
        };
        var customerPackage = new CustomerPackage
        {
            CustomerId = 1,
            RemainingCredits = remainingCredits,
            Package = new Package { BusinessId = 1, ExpiresAtUtc = DateTime.UtcNow.AddDays(1) }
        };

        return new Booking
        {
            CustomerId = 1,
            TimetableScheduleId = 1,
            ActiveTimetableScheduleId = 1,
            CustomerPackageId = 1,
            Status = BookingStatus.Booked,
            TimetableSchedule = schedule,
            CustomerPackage = customerPackage
        };
    }

    private static Booking CreateWaitlistedBooking(int customerId, int remainingCredits) => new()
    {
        CustomerId = customerId,
        TimetableScheduleId = 1,
        ActiveTimetableScheduleId = 1,
        CustomerPackageId = customerId,
        Status = BookingStatus.Waitlisted,
        CustomerPackage = new CustomerPackage
        {
            CustomerId = customerId,
            RemainingCredits = remainingCredits,
            Package = new Package { BusinessId = 1, ExpiresAtUtc = DateTime.UtcNow.AddDays(1) }
        }
    };

    private sealed class RecordingBookingRepository : IBookingRepository
    {
        public BookingCandidate? Candidate { get; init; }
        public BookingCancellationCandidate? CancellationCandidate { get; init; }
        public IReadOnlyList<WaitlistPromotionCandidate> WaitlistCandidates { get; init; } = [];
        public int DeletedWaitlistEntryCount { get; init; }
        public TaskCompletionSource? FirstCandidateLoaded { get; init; }
        public TaskCompletionSource? ContinueFirstCandidate { get; init; }
        public DateTime? StartedBeforeOrAtUtc { get; private set; }
        public List<Booking> AddedBookings { get; } = [];
        private int _candidateLoadCount;

        public Task AddAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            AddedBookings.Add(booking);
            return Task.CompletedTask;
        }

        public Task<int> DeleteStartedWaitlistEntriesAsync(DateTime startedBeforeOrAtUtc, CancellationToken cancellationToken = default)
        {
            StartedBeforeOrAtUtc = startedBeforeOrAtUtc;
            return Task.FromResult(DeletedWaitlistEntryCount);
        }

        public Task<int?> GetTimetableScheduleIdAsync(int bookingId, CancellationToken cancellationToken = default) =>
            Task.FromResult(CancellationCandidate is null ? null : (int?)CancellationCandidate.Booking.TimetableScheduleId);

        public Task<IReadOnlyList<WaitlistPromotionCandidate>> ListWaitlistPromotionCandidatesAsync(
            int timetableScheduleId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(WaitlistCandidates);

        public Task<BookingCancellationCandidate?> LoadCancellationCandidateAsync(
            int bookingId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(CancellationCandidate);

        public async Task<BookingCandidate?> LoadCandidateAsync(
            int customerId,
            int timetableScheduleId,
            int customerPackageId,
            CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref _candidateLoadCount) == 1 &&
                FirstCandidateLoaded is not null &&
                ContinueFirstCandidate is not null)
            {
                FirstCandidateLoaded.SetResult();
                await ContinueFirstCandidate.Task.WaitAsync(cancellationToken);
            }

            return Candidate;
        }
    }

    private sealed class ImmediateTransactionExecutor : ITransactionExecutor
    {
        public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default) =>
            operation(cancellationToken);
    }

    private sealed class AcquiredLockProvider : IDistributedLock
    {
        public Task<IAsyncDisposable?> TryAcquireAsync(
            string resource,
            TimeSpan expiration,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IAsyncDisposable?>(new NoOpAsyncDisposable());
    }

    private sealed class SerializedLockProvider : IDistributedLock
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<IAsyncDisposable?> TryAcquireAsync(
            string resource,
            TimeSpan expiration,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            return new ReleasingAsyncDisposable(_semaphore);
        }
    }

    private sealed class NoOpCache : IApplicationCache
    {
        public Task<T> GetOrCreateAsync<T>(
            string key,
            TimeSpan expiration,
            Func<CancellationToken, Task<T>> factory,
            CancellationToken cancellationToken = default) =>
            factory(cancellationToken);

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class NoOpAsyncDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class ReleasingAsyncDisposable(SemaphoreSlim semaphore) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            semaphore.Release();
            return ValueTask.CompletedTask;
        }
    }
}