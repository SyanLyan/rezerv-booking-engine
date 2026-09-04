using FluentValidation;
using Rezerv.Api.Contracts.Packages;

namespace Rezerv.Api.Validators;

public sealed class CreatePackageRequestValidator : AbstractValidator<CreatePackageRequest>
{
    public CreatePackageRequestValidator()
    {
        RuleFor(request => request.BusinessId)
            .GreaterThan(0)
            .WithMessage("BusinessId must be greater than zero.");

        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters.");

        RuleFor(request => request.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters.")
            .When(request => request.Description is not null);

        RuleFor(request => request.Credits)
            .GreaterThan(0)
            .WithMessage("Credits must be greater than zero.");

        RuleFor(request => request.ExpiresAtUtc)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("ExpiresAtUtc must be in the future.");
    }
}