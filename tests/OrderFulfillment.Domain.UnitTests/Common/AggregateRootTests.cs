using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.Orders;
using Xunit;

namespace OrderFulfillment.Domain.UnitTests.Common;

public sealed class AggregateRootTests
{
    [Fact]
    public void NewAggregate_ShouldHaveNoDomainEvents()
    {
        var aggregate = new TestAggregate(OrderId.New());

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void RaisingEvent_ShouldAddItToDomainEvents()
    {
        var aggregate = new TestAggregate(OrderId.New());

        aggregate.DoSomething();

        var domainEvent = Assert.Single(aggregate.DomainEvents);
        Assert.IsType<TestEvent>(domainEvent);
    }

    [Fact]
    public void RaisingEvent_MultipleTimes_ShouldKeepAllEvents()
    {
        var aggregate = new TestAggregate(OrderId.New());

        aggregate.DoSomething();
        aggregate.DoSomething();

        Assert.Equal(2, aggregate.DomainEvents.Count);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var aggregate = new TestAggregate(OrderId.New());
        aggregate.DoSomething();

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void DomainEvent_ShouldHaveUniqueIdAndUtcTimestamp()
    {
        var first = new TestEvent();
        var second = new TestEvent();

        Assert.NotEqual(Guid.Empty, first.EventId);
        Assert.NotEqual(first.EventId, second.EventId);
        Assert.Equal(DateTimeKind.Utc, first.OccurredOnUtc.Kind);
    }

    private sealed record TestEvent : DomainEvent;

    private sealed class TestAggregate : AggregateRoot<OrderId>
    {
        public TestAggregate(OrderId id)
            : base(id)
        {
        }

        public void DoSomething() => RaiseDomainEvent(new TestEvent());
    }
}