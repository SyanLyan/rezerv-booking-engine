using Rezerv.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Rezerv.Infrastructure.Locking;

public sealed class RedisDistributedLock(IConnectionMultiplexer connectionMultiplexer) : IDistributedLock
{
    private const string ReleaseLockScript = """
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        end
        return 0
        """;

    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    public async Task<IAsyncDisposable?> TryAcquireAsync(
        string resource,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var token = Guid.NewGuid().ToString("N");
        var acquired = await _database.StringSetAsync(resource, token, expiration, When.NotExists);

        return acquired ? new AcquiredLock(_database, resource, token) : null;
    }

    private sealed class AcquiredLock(IDatabase database, RedisKey resource, RedisValue token) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await database.ScriptEvaluateAsync(ReleaseLockScript, [resource], [token]);
        }
    }
}