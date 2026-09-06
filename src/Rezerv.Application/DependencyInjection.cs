using Microsoft.Extensions.DependencyInjection;
using Rezerv.Application.Services.Bookings;
using Rezerv.Application.Services.Businesses;
using Rezerv.Application.Services.Customers;
using Rezerv.Application.Services.Packages;
using Rezerv.Application.Services.Timetable;

namespace Rezerv.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IBookingRuleEngine, BookingRuleEngine>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IBusinessService, BusinessService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IPackageService, PackageService>();
        services.AddScoped<ITimetableService, TimetableService>();

        return services;
    }
}