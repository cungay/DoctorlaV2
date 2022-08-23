namespace Doctorla.Application.Common.Persistence;
/// <summary>
/// A special (read/write) repository for an aggregate root,
/// that also adds EntityCreated, EntityUpdated or EntityDeleted
/// events to the DomainEvents of the entities before adding,
/// updating or deleting them.
/// </summary>
public interface IEntityRepository<T> : IRepository<T>
    where T : IEntity, IAggregateRoot
{
}
