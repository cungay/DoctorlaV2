using Doctorla.Application.Events;
using Doctorla.Shared.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Doctorla.Infrastructure.Common.Services;

public class EventPublisher : IEventPublisher
{
    private readonly ILogger<EventPublisher> logger = null;
    private readonly IPublisher mediator = null;

    public EventPublisher(ILogger<EventPublisher> logger, IPublisher mediator) =>
        (this.logger, this.mediator) = (logger, mediator);

    public Task PublishAsync(IEvent @event)
    {
        logger.LogInformation("Publishing Event : {event}", @event.GetType().Name);
        return mediator.Publish(CreateEventNotification(@event));
    }

    private INotification CreateEventNotification(IEvent @event) =>
        (INotification)Activator.CreateInstance(
            typeof(EventNotification<>).MakeGenericType(@event.GetType()), @event)!;
}