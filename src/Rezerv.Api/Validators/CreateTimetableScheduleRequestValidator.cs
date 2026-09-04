using FluentValidation;
using Rezerv.Api.Contracts.Timetable;

namespace Rezerv.Api.Validators;

public sealed class CreateTimetableScheduleRequestValidator : AbstractValidator<CreateTimetableScheduleRequest>
{
    public CreateTimetableScheduleRequestValidator()
    {
        RuleFor(request => request.BusinessId)
            .GreaterThan(0)
            .WithMessage("BusinessId must be greater than zero.");

        RuleFor(request => request.ClassName)
            .NotEmpty()
            .WithMessage("ClassName is required.")
            .MaximumLength(200)
            .WithMessage("ClassName must not exceed 200 characters.");

        RuleFor(request => request.Instructor)
            .NotEmpty()
            .WithMessage("Instructor is required.")
            .MaximumLength(200)
            .WithMessage("Instructor must not exceed 200 characters.");

        RuleFor(request => request.EndTimeUtc)
            .GreaterThan(request => request.StartTimeUtc)
            .WithMessage("EndTimeUtc must be after StartTimeUtc.");

        RuleFor(request => request.TotalSlots)
            .GreaterThan(0)
            .WithMessage("TotalSlots must be greater than zero.");
    }
}