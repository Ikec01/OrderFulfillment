using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using OrderFulfillment.Contracts.Orders;
using OrderFulfillment.Infrastructure;
using OrderFulfillment.Infrastructure.Messaging;
using OrderFulfillment.Infrastructure.Persistence.Outbox;
using OrderFulfillment.IntegrationTests.Support;
using Xunit;

namespace OrderFulfillment.IntegrationTests.Outbox;

public sealed record ConsumedMessage(Guid? MessageId, OrderPlacedIntegrationEvent Event);

public sealed class MessageProbe
{
    public TaskCompletionSource<ConsumedMessage> Received { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}

public sealed class ProbeConsumer(MessageProbe probe) : IConsumer<OrderPlacedIntegrationEvent>
{
    public Task Consume(ConsumeContext<OrderPlacedIntegrationEvent> context)
    {
        ArgumentNullException.ThrowIfNull(context);

        probe.Received.TrySetResult(new ConsumedMessage(context.MessageId, context.Message));

        return Task.CompletedTask;
    }
}

[Collection(PostgresCollectionDefinition.Name)]
public sealed class OutboxToRabbitMqTests(PostgresFixture postgres, RabbitMqFixture rabbitMq)
    : IClassFixture<RabbitMqFixture>
{
    [Fact]
    public async Task PlacedOrder_ShouldTravelFromOutboxThroughRabbitMqToConsumer()
    {
        await ClearPendingMessagesAsync();

        var probe = new MessageProbe();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(probe);
        services.AddRabbitMessaging(rabbitMq.Options, configurator => configurator.AddConsumer<ProbeConsumer>());

        await using var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<IBusControl>();
        await bus.StartAsync();

        try
        {
            var order = TestData.CreatePlacedOrder(Guid.NewGuid());
            var eventId = Assert.Single(order.DomainEvents).EventId;
            await postgres.SaveOrderAsync(order);

            await using var scope = provider.CreateAsyncScope();
            var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();
            await using var context = postgres.CreateDbContext();
            var processor = new OutboxProcessor(
                context,
                publisher,
                TimeProvider.System,
                new OutboxProcessorOptions(),
                NullLogger<OutboxProcessor>.Instance);

            var processed = await processor.ProcessBatchAsync();

            Assert.Equal(1, processed);

            var consumed = await probe.Received.Task.WaitAsync(TimeSpan.FromSeconds(30));
            Assert.Equal(eventId, consumed.MessageId);
            Assert.Equal(order.Id.Value, consumed.Event.OrderId);
            Assert.Equal("EUR", consumed.Event.Currency);
            Assert.Equal(20m, consumed.Event.TotalAmount);

            await using var verifyContext = postgres.CreateDbContext();
            var stored = await verifyContext.OutboxMessages.SingleAsync(message => message.Id == eventId);
            Assert.NotNull(stored.ProcessedOnUtc);
        }
        finally
        {
            await bus.StopAsync();
        }
    }

    private async Task ClearPendingMessagesAsync()
    {
        await using var context = postgres.CreateDbContext();

        await context.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(
                message => message.ProcessedOnUtc,
                (DateTime?)DateTime.UtcNow));
    }
}