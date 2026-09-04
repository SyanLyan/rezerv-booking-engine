using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Infrastructure.Configuration;
using Rezerv.Infrastructure.Persistence;
using Rezerv.Infrastructure.Persistence.Repositories;

namespace Rezerv.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ConnectionStrings>(configuration.GetSection("ConnectionStrings"));

        services.AddDbContext<RezervDbContext>((serviceProvider, options) =>
        {
            var connectionStrings = serviceProvider.GetRequiredService<IOptions<ConnectionStrings>>().Value;
            options.UseMySql(connectionStrings.DefaultConnection, new MySqlServerVersion(new Version(8, 4, 0)));
        });
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        return services;
    }
}