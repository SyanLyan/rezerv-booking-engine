using FluentValidation;
using Rezerv.Api.Contracts.Bookings;

namespace Rezerv.Api.Validators;

public sealed class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(request => request.CustomerId)
            .GreaterThan(0)
            .WithMessage("CustomerId must be greater than zero.");

        RuleFor(request => request.TimetableScheduleId)
            .GreaterThan(0)
            .WithMessage("TimetableScheduleId must be greater than zero.");

        RuleFor(request => request.CustomerPackageId)
            .GreaterThan(0)
            .WithMessage("CustomerPackageId must be greater than zero.");
    }
}