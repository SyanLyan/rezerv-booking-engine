using System.Text.Json;
using Rezerv.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Rezerv.Infrastructure.Caching;

public sealed class RedisApplicationCache(IConnectionMultiplexer connectionMultiplexer) : IApplicationCache
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan expiration,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var cachedValue = await _database.StringGetAsync(key);
        if (cachedValue.HasValue)
        {
            var value = JsonSerializer.Deserialize<T>(cachedValue!, SerializerOptions);
            if (value is not null)
            {
                return value;
            }
        }

        var result = await factory(cancellationToken);
        var serializedResult = JsonSerializer.Serialize(result, SerializerOptions);
        await _database.StringSetAsync(key, serializedResult, expiration);

        return result;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _database.KeyDeleteAsync(key);
    }
}