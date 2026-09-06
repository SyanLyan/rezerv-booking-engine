using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rezerv.Api.Contracts.Common;

namespace Rezerv.Api.Filters;

public sealed class FluentValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values.Where(value => value is not null))
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationResult = await validator.ValidateAsync(
                new ValidationContext<object>(argument),
                context.HttpContext.RequestAborted);

            if (validationResult.IsValid)
            {
                continue;
            }

            context.Result = new BadRequestObjectResult(
                ApiResponse<object>.Failed(
                    ApiResponseMessages.ValidationFailed,
                    validationResult.Errors.Select(error => error.ErrorMessage).ToArray()));
            return;
        }

        await next();
    }
}