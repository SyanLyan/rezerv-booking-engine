namespace Rezerv.Application.Common.Interfaces;

public interface IDistributedLock
{
    Task<IAsyncDisposable?> TryAcquireAsync(
        string resource,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);
}