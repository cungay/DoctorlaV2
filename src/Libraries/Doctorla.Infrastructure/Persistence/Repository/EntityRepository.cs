using Doctorla.Application.Persistence;
using Doctorla.Domain.Common.Contracts;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using System.Data;

namespace Doctorla.Infrastructure.Persistence.Repository;

public class EntityRepository<T> : IEntityRepository<T>
    where T : BaseEntity<T>, IAggregateRoot
{
    private readonly IDbConnectionFactory dbContext = null;

    public EntityRepository(IDbConnectionFactory dbContext)
    {
        this.dbContext = dbContext;
    }


    /// <inheritdoc/>
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        using var db = dbContext.Open();
        await db.InsertAsync(entity, token: cancellationToken);
        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        using (var db = dbContext.Open())
        {
            using IDbTransaction dbTrans = db.OpenTransaction(IsolationLevel.ReadCommitted);
            await db.InsertAllAsync(entities, token: cancellationToken);
        }

        return entities;
    }

    /// <inheritdoc/>
    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        switch (entity)
        {
            case null:
                throw new ArgumentNullException(nameof(entity));

            case ISoftDeletedEntity softDeletedEntity:
                if (softDeletedEntity.Deleted)
                    await UpdateAsync(entity, cancellationToken);
                break;
            default:
                await DeleteAsync(entity, cancellationToken);
                break;
        }
    }

    /// <inheritdoc/>
    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        if (entities.OfType<ISoftDeletedEntity>().Any())
        {
            foreach (var entity in entities)
            {
                if (entity is ISoftDeletedEntity softDeletedEntity && softDeletedEntity.Deleted)
                {
                    await UpdateAsync(entity, cancellationToken);
                }
            }
        }
        else
        {
            var ids = entities.Select(p => p.Id);
            if (!ids.Any())
                return;

            using var db = dbContext.Open();
            using IDbTransaction dbTrans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted);
            await db.DeleteByIdsAsync<T>(idValues: ids, token: cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
    {
        if (id == null || Equals(id, default(TId)))
            return null;

        using var db = dbContext.Open();
        var entity = await db.SingleByIdAsync<T>(id, cancellationToken);
        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        using var db = dbContext.Open();
        await db.UpdateAsync(entity, token: cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        if (!entities.Any())
            return;

        foreach (var entity in entities)
            await UpdateAsync(entity, cancellationToken);
    }
}
