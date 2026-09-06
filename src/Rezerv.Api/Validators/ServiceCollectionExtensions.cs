using FluentValidation;
using Rezerv.Api.Contracts.Bookings;
using Rezerv.Api.Contracts.Businesses;
using Rezerv.Api.Contracts.Customers;
using Rezerv.Api.Contracts.Packages;
using Rezerv.Api.Contracts.Timetable;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRezervValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateBookingRequest>, CreateBookingRequestValidator>();
        services.AddScoped<IValidator<JoinWaitlistRequest>, JoinWaitlistRequestValidator>();
        services.AddScoped<IValidator<CreateBusinessRequest>, CreateBusinessRequestValidator>();
        services.AddScoped<IValidator<CreateCustomerRequest>, CreateCustomerRequestValidator>();
        services.AddScoped<IValidator<CreatePackageRequest>, CreatePackageRequestValidator>();
        services.AddScoped<IValidator<PurchasePackageRequest>, PurchasePackageRequestValidator>();
        services.AddScoped<IValidator<CreateTimetableScheduleRequest>, CreateTimetableScheduleRequestValidator>();

        return services;
    }
}