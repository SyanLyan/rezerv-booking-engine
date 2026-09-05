using System.Data;
using Microsoft.EntityFrameworkCore;
using Rezerv.Application.Common.Interfaces;

namespace Rezerv.Infrastructure.Persistence;

public sealed class TransactionExecutor(RezervDbContext dbContext) : ITransactionExecutor
{
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var result = await operation(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}