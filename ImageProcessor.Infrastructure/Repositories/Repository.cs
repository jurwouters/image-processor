using ImageProcessor.Application.Repositories;
using ImageProcessor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessor.Infrastructure.Repositories;

public abstract class Repository<TEntity>(ApplicationDbContext db) : IRepository<TEntity>
    where TEntity : class
{
    protected readonly ApplicationDbContext Context = db;
    protected readonly DbSet<TEntity> Set = db.Set<TEntity>();

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await Set.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Set.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Set.Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public virtual Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Context.SaveChangesAsync(cancellationToken);
}
