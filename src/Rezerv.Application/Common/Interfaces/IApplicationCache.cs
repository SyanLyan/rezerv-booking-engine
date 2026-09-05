namespace Rezerv.Application.Common.Interfaces;

public interface IApplicationCache
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan expiration,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}