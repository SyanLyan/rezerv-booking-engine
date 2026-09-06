using FluentValidation;
using Rezerv.Api.Contracts.Bookings;

namespace Rezerv.Api.Validators;

public sealed class JoinWaitlistRequestValidator : AbstractValidator<JoinWaitlistRequest>
{
    public JoinWaitlistRequestValidator()
    {
        RuleFor(request => request.CustomerId)
            .GreaterThan(0)
            .WithMessage("Customer ID must be greater than zero.");
        RuleFor(request => request.TimetableScheduleId)
            .GreaterThan(0)
            .WithMessage("Timetable schedule ID must be greater than zero.");
        RuleFor(request => request.CustomerPackageId)
            .GreaterThan(0)
            .WithMessage("Customer package ID must be greater than zero.");
    }
}