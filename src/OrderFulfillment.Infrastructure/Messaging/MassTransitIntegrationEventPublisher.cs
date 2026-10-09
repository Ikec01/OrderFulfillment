using MassTransit;

namespace OrderFulfillment.Infrastructure.Messaging;

public sealed class MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint)
    : IIntegrationEventPublisher
{
    public Task PublishAsync(
        Guid messageId,
        object integrationEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        // Identifikator Outbox poruke postaje identifikator poruke u brokeru: potrošači ga koriste za deduplikaciju.
        var pipe = Pipe.Execute<PublishContext>(context => context.MessageId = messageId);

        return publishEndpoint.Publish(integrationEvent, integrationEvent.GetType(), pipe, cancellationToken);
    }
}