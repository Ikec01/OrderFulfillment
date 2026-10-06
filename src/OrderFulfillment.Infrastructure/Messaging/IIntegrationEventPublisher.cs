namespace OrderFulfillment.Infrastructure.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(Guid messageId, object integrationEvent, CancellationToken cancellationToken = default);
}