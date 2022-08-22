using Doctorla.Shared.Events;

namespace Doctorla.Application.Common.Events;

public interface IEventPublisher
{
    Task PublishAsync(IEvent @event);
}