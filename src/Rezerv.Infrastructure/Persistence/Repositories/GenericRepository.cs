using Microsoft.EntityFrameworkCore;
using Rezerv.Application.Common.Interfaces;
using Rezerv.Domain.Common;

namespace Rezerv.Infrastructure.Persistence.Repositories;

public sealed class GenericRepository<TEntity>(RezervDbContext dbContext) : IGenericRepository<TEntity>
    where TEntity : Entity
{
    private readonly DbSet<TEntity> _entities = dbContext.Set<TEntity>();

    public Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _entities.FindAsync([id], cancellationToken).AsTask();

    public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default) =>
        await _entities.AsNoTracking().ToListAsync(cancellationToken);

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        _entities.AddAsync(entity, cancellationToken).AsTask();

    public void Update(TEntity entity) => _entities.Update(entity);

    public void Remove(TEntity entity) => _entities.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}