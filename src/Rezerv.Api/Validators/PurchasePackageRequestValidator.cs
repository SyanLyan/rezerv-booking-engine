using FluentValidation;
using Rezerv.Api.Contracts.Packages;

namespace Rezerv.Api.Validators;

public sealed class PurchasePackageRequestValidator : AbstractValidator<PurchasePackageRequest>
{
    public PurchasePackageRequestValidator()
    {
        RuleFor(request => request.CustomerId)
            .GreaterThan(0)
            .WithMessage("CustomerId must be greater than zero.");

        RuleFor(request => request.PackageId)
            .GreaterThan(0)
            .WithMessage("PackageId must be greater than zero.");
    }
}