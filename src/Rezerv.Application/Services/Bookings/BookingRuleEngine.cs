using RulesEngine.Models;

namespace Rezerv.Application.Services.Bookings;

public sealed class BookingRuleEngine : IBookingRuleEngine
{
    private const string BookingEligibilityWorkflowName = "BookingEligibility";
    private const string WaitlistEligibilityWorkflowName = "WaitlistEligibility";
    private const string CancellationRefundWorkflowName = "CancellationRefundEligibility";

    private static readonly RulesEngine.RulesEngine Engine = new(
    [
        new Workflow
        {
            WorkflowName = BookingEligibilityWorkflowName,
            Rules =
            [
                CreateRule("ScheduleIsInFuture", "input1.IsScheduleInFuture", "The schedule has already started."),
                CreateRule("ScheduleIsNotFull", "input1.AvailableSlots > 0", "The schedule is full. No more bookings are allowed."),
                CreateRule("CustomerHasCredit", "input1.HasRemainingPackageCredit", "The customer has no remaining package credit."),
                CreateRule("PackageHasNotExpired", "!input1.IsPackageExpired", "Expired packages cannot be used."),
                CreateRule("PackageMatchesBusiness", "input1.HasMatchingBusinessPackage", "The customer package belongs to another business."),
                CreateRule("NoExistingBooking", "!input1.HasExistingBooking", "The customer already has a booking for this schedule."),
                CreateRule("NoOverlappingBooking", "!input1.HasOverlappingBooking", "The customer already has a booking that overlaps this schedule.")
            ]
        },
        new Workflow
        {
            WorkflowName = WaitlistEligibilityWorkflowName,
            Rules =
            [
                CreateRule("ScheduleIsInFuture", "input1.IsScheduleInFuture", "The schedule has already started."),
                CreateRule("CustomerHasCredit", "input1.HasRemainingPackageCredit", "The customer has no remaining package credit."),
                CreateRule("PackageHasNotExpired", "!input1.IsPackageExpired", "Expired packages cannot be used."),
                CreateRule("PackageMatchesBusiness", "input1.HasMatchingBusinessPackage", "The customer package belongs to another business."),
                CreateRule("NoExistingBooking", "!input1.HasExistingBooking", "The customer already has a booking for this schedule."),
                CreateRule("NoOverlappingBooking", "!input1.HasOverlappingBooking", "The customer already has a booking that overlaps this schedule.")
            ]
        },
        new Workflow
        {
            WorkflowName = CancellationRefundWorkflowName,
            Rules =
            [
                CreateRule(
                    "CancellationIsAtLeastFourHoursBeforeSchedule",
                    "input1.IsAtLeastFourHoursBeforeSchedule",
                    "The cancellation was made less than four hours before the schedule, so the credit is not refunded.")
            ]
        }
    ]);

    public async Task<BookingRuleEvaluation> EvaluateAsync(
        BookingRuleInput input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var results = await Engine.ExecuteAllRulesAsync(BookingEligibilityWorkflowName, input);
        var failures = results
            .Where(result => !result.IsSuccess)
            .Select(result => result.Rule.ErrorMessage)
            .ToList();

        return new BookingRuleEvaluation(failures.Count == 0, failures);
    }

    public async Task<BookingRuleEvaluation> EvaluateWaitlistAsync(
        BookingRuleInput input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var results = await Engine.ExecuteAllRulesAsync(WaitlistEligibilityWorkflowName, input);
        var failures = results
            .Where(result => !result.IsSuccess)
            .Select(result => result.Rule.ErrorMessage)
            .ToList();

        return new BookingRuleEvaluation(failures.Count == 0, failures);
    }

    public async Task<BookingCancellationEvaluation> EvaluateCancellationAsync(
        BookingCancellationRuleInput input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var results = await Engine.ExecuteAllRulesAsync(CancellationRefundWorkflowName, input);
        var failedRule = results.SingleOrDefault(result => !result.IsSuccess);

        return new BookingCancellationEvaluation(
            failedRule is null,
            failedRule?.Rule.ErrorMessage);
    }

    private static Rule CreateRule(string name, string expression, string errorMessage) => new()
    {
        RuleName = name,
        Expression = expression,
        ErrorMessage = errorMessage
    };
}