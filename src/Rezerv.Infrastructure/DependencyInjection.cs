using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Infrastructure.Caching;
using Rezerv.Infrastructure.Configuration;
using Rezerv.Infrastructure.Locking;
using Rezerv.Infrastructure.Persistence;
using Rezerv.Infrastructure.Persistence.Repositories;
using StackExchange.Redis;

namespace Rezerv.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ConnectionStrings>(configuration.GetSection("ConnectionStrings"));
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));

        services.AddDbContext<RezervDbContext>((serviceProvider, options) =>
        {
            var connectionStrings = serviceProvider.GetRequiredService<IOptions<ConnectionStrings>>().Value;
            options.UseMySql(connectionStrings.DefaultConnection, new MySqlServerVersion(new Version(8, 4, 0)));
        });
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ITimetableScheduleRepository, TimetableScheduleRepository>();
        services.AddScoped<ITransactionExecutor, TransactionExecutor>();
        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
        {
            var redisOptions = serviceProvider.GetRequiredService<IOptions<RedisOptions>>().Value;
            return ConnectionMultiplexer.Connect(redisOptions.Configuration);
        });
        services.AddSingleton<IApplicationCache, RedisApplicationCache>();
        services.AddSingleton<IDistributedLock, RedisDistributedLock>();

        return services;
    }
}