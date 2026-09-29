using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.Orders;
using Xunit;

namespace OrderFulfillment.Domain.UnitTests.Common;

public sealed class EntityTests
{
    [Fact]
    public void Entities_WithSameId_ShouldBeEqual()
    {
        var id = OrderId.New();

        var first = new TestEntity(id);
        var second = new TestEntity(id);

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Entities_WithDifferentIds_ShouldNotBeEqual()
    {
        var first = new TestEntity(OrderId.New());
        var second = new TestEntity(OrderId.New());

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }

    [Fact]
    public void Entities_OfDifferentTypes_ShouldNotBeEqual_EvenWithSameId()
    {
        var id = OrderId.New();

        var first = new TestEntity(id);
        var second = new OtherTestEntity(id);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Constructor_WithDefaultId_ShouldThrowDomainException()
    {
        Assert.Throws<DomainException>(() => new TestEntity(default));
    }

    private sealed class TestEntity : Entity<OrderId>
    {
        public TestEntity(OrderId id)
            : base(id)
        {
        }
    }

    private sealed class OtherTestEntity : Entity<OrderId>
    {
        public OtherTestEntity(OrderId id)
            : base(id)
        {
        }
    }
}