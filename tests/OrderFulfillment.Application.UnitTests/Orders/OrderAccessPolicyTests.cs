using OrderFulfillment.Application.Orders;
using OrderFulfillment.Application.UnitTests.Fakes;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Orders;

public sealed class OrderAccessPolicyTests
{
    [Fact]
    public void CanAccess_WhenUserIsOwner_ShouldBeTrue()
    {
        var owner = FakeCurrentUser.Customer();

        Assert.True(OrderAccessPolicy.CanAccess(owner, owner.UserId!.Value));
    }

    [Fact]
    public void CanAccess_WhenUserIsAnotherCustomer_ShouldBeFalse()
    {
        Assert.False(OrderAccessPolicy.CanAccess(FakeCurrentUser.Customer(), Guid.NewGuid()));
    }

    [Fact]
    public void CanAccess_WhenUserIsAdministrator_ShouldBeTrue()
    {
        Assert.True(OrderAccessPolicy.CanAccess(FakeCurrentUser.Administrator(), Guid.NewGuid()));
    }

    [Fact]
    public void CanAccess_WhenUserIsAnonymous_ShouldBeFalse()
    {
        Assert.False(OrderAccessPolicy.CanAccess(FakeCurrentUser.Anonymous(), Guid.NewGuid()));
    }

    [Fact]
    public void CanAccess_WhenAnonymousAndOwnerIdIsEmpty_ShouldStillBeFalse()
    {
        Assert.False(OrderAccessPolicy.CanAccess(FakeCurrentUser.Anonymous(), Guid.Empty));
    }

    [Fact]
    public void CanAccess_ForOrder_ShouldUseOrderCustomer()
    {
        var owner = FakeCurrentUser.Customer();
        var order = TestOrders.CreatePlaced(new Domain.Orders.CustomerId(owner.UserId!.Value));

        Assert.True(OrderAccessPolicy.CanAccess(owner, order));
        Assert.False(OrderAccessPolicy.CanAccess(FakeCurrentUser.Customer(), order));
    }
}