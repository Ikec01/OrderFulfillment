using OrderFulfillment.Contracts.Orders;
using OrderFulfillment.Infrastructure.Persistence.Outbox;
using Xunit;

namespace OrderFulfillment.IntegrationTests.Outbox;

public sealed class OutboxSerializerTests
{
    [Fact]
    public void SerializeThenDeserialize_ShouldRestoreTheSameEvent()
    {
        var original = new OrderPaidIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Guid.NewGuid(),
            "EUR",
            20m);

        var json = OutboxSerializer.Serialize(original);
        var restored = OutboxSerializer.Deserialize(typeof(OrderPaidIntegrationEvent).FullName!, json);

        Assert.Equal(original, restored);
    }

    [Fact]
    public void Deserialize_WithTypeOutsideContracts_ShouldBeRejected()
    {
        Assert.Throws<InvalidOperationException>(() => OutboxSerializer.Deserialize("System.IO.File", "{}"));
    }

    [Fact]
    public void Deserialize_WithUnknownTypeName_ShouldBeRejected()
    {
        Assert.Throws<InvalidOperationException>(
            () => OutboxSerializer.Deserialize("OrderFulfillment.Contracts.Orders.DoesNotExist", "{}"));
    }
}