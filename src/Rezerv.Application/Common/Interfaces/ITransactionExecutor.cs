namespace Rezerv.Application.Common.Interfaces;

public interface ITransactionExecutor
{
    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
}