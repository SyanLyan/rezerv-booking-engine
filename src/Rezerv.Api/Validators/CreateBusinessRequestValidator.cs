using FluentValidation;
using Rezerv.Api.Contracts.Businesses;

namespace Rezerv.Api.Validators;

public sealed class CreateBusinessRequestValidator : AbstractValidator<CreateBusinessRequest>
{
    public CreateBusinessRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters.");
    }
}