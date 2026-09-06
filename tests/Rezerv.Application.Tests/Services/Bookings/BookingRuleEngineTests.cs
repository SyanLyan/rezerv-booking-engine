using Rezerv.Application.Services.Bookings;
using Xunit;

namespace Rezerv.Application.Tests.Services.Bookings;

public sealed class BookingRuleEngineTests
{
    private readonly BookingRuleEngine _sut = new();

    [Fact]
    public async Task EvaluateAsync_WhenPackageIsExpired_RejectsBooking()
    {
        var evaluation = await _sut.EvaluateAsync(ValidInput with { IsPackageExpired = true });

        Assert.False(evaluation.IsAllowed);
        Assert.Contains("Expired packages cannot be used.", evaluation.Failures);
    }

    [Fact]
    public async Task EvaluateAsync_WhenCustomerHasNoRemainingCredits_RejectsBooking()
    {
        var evaluation = await _sut.EvaluateAsync(ValidInput with { HasRemainingPackageCredit = false });

        Assert.False(evaluation.IsAllowed);
        Assert.Contains("The customer has no remaining package credit.", evaluation.Failures);
    }

    [Fact]
    public async Task EvaluateAsync_WhenPackageBelongsToAnotherBusiness_RejectsBooking()
    {
        var evaluation = await _sut.EvaluateAsync(ValidInput with { HasMatchingBusinessPackage = false });

        Assert.False(evaluation.IsAllowed);
        Assert.Contains("The customer package belongs to another business.", evaluation.Failures);
    }

    [Fact]
    public async Task EvaluateAsync_WhenCustomerHasAnOverlappingBooking_RejectsBooking()
    {
        var evaluation = await _sut.EvaluateAsync(ValidInput with { HasOverlappingBooking = true });

        Assert.False(evaluation.IsAllowed);
        Assert.Contains("The customer already has a booking that overlaps this schedule.", evaluation.Failures);
    }

    private static readonly BookingRuleInput ValidInput = new(
        IsScheduleInFuture: true,
        AvailableSlots: 1,
        HasRemainingPackageCredit: true,
        IsPackageExpired: false,
        HasMatchingBusinessPackage: true,
        HasExistingBooking: false,
        HasOverlappingBooking: false);
}