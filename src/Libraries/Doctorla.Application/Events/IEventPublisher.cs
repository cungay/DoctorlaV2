using Doctorla.Shared.Events;

namespace Doctorla.Application.Events;

public interface IEventPublisher : ITransientService
{
    Task PublishAsync(IEvent @event);
}