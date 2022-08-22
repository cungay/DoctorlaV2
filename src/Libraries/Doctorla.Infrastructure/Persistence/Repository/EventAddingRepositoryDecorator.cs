using Doctorla.Application.Common.Persistence;
using Doctorla.Domain.Common.Contracts;

namespace Doctorla.Infrastructure.Persistence.Repository;

/// <summary>
/// The repository that implements IRepositoryWithEvents.
/// Implemented as a decorator. It only augments the Add,
/// Update and Delete calls where it adds the respective
/// EntityCreated, EntityUpdated or EntityDeleted event
/// before delegating to the decorated repository.
/// </summary>
public class EventAddingRepositoryDecorator<T> : IRepositoryWithEvents<T>
    where T : class, IAggregateRoot
{
    private readonly IRepository<T> _decorated;

    public EventAddingRepositoryDecorator(IRepository<T> decorated) => _decorated = decorated;

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}