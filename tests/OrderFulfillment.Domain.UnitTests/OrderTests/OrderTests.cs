using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.Orders.Events;
using OrderFulfillment.Domain.ValueObjects;
using Xunit;

namespace OrderFulfillment.Domain.UnitTests.Orders;

public sealed class OrderTests
{

    [Fact]
    public void Create_ShouldStartAsDraftWithNoItems()
    {
        var customerId = CustomerId.New();
        var address = CreateAddress();

        var order = Order.Create(customerId, address, "EUR");

        Assert.NotEqual(default, order.Id);
        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal(address, order.ShippingAddress);
        Assert.Equal(OrderStatus.Draft, order.Status);
        Assert.Empty(order.Items);
        Assert.Empty(order.DomainEvents);
        Assert.Equal(Money.Zero("EUR"), order.TotalAmount);
        Assert.Equal(DateTimeKind.Utc, order.CreatedAtUtc.Kind);
    }

    [Fact]
    public void Create_ShouldNormalizeCurrency()
    {
        var order = Order.Create(CustomerId.New(), CreateAddress(), " eur ");

        Assert.Equal("EUR", order.Currency);
    }

    [Fact]
    public void Create_WithDefaultCustomerId_ShouldThrow()
    {
        Assert.Throws<DomainException>(() => Order.Create(default, CreateAddress(), "EUR"));
    }

    [Fact]
    public void Create_WithNullAddress_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(
            () => Order.Create(CustomerId.New(), null!, "EUR"));
    }

    [Fact]
    public void Create_WithInvalidCurrency_ShouldThrow()
    {
        Assert.Throws<DomainException>(
            () => Order.Create(CustomerId.New(), CreateAddress(), "EU"));
    }

    

    [Fact]
    public void AddItem_ShouldAddItemsAndCalculateTotal()
    {
        var order = CreateDraftOrder();

        order.AddItem(ProductId.New(), "Laptop stand", Money.Create(10m, "EUR"), 2);
        order.AddItem(ProductId.New(), "Mouse pad", Money.Create(5.50m, "EUR"), 1);

        Assert.Equal(2, order.Items.Count);
        Assert.Equal(Money.Create(25.50m, "EUR"), order.TotalAmount);
    }

    [Fact]
    public void AddItem_SameProductTwice_ShouldIncreaseQuantityInsteadOfDuplicating()
    {
        var order = CreateDraftOrder();
        var productId = ProductId.New();

        order.AddItem(productId, "Laptop stand", Money.Create(10m, "EUR"), 2);
        order.AddItem(productId, "Laptop stand", Money.Create(10m, "EUR"), 3);

        var item = Assert.Single(order.Items);
        Assert.Equal(5, item.Quantity);
        Assert.Equal(Money.Create(50m, "EUR"), order.TotalAmount);
    }

    [Fact]
    public void AddItem_SameProductWithDifferentPrice_ShouldThrow()
    {
        var order = CreateDraftOrder();
        var productId = ProductId.New();
        order.AddItem(productId, "Laptop stand", Money.Create(10m, "EUR"), 1);

        Assert.Throws<DomainException>(
            () => order.AddItem(productId, "Laptop stand", Money.Create(12m, "EUR"), 1));
    }

    [Fact]
    public void AddItem_WithDifferentCurrency_ShouldThrow()
    {
        var order = CreateDraftOrder();

        Assert.Throws<DomainException>(
            () => order.AddItem(ProductId.New(), "Laptop stand", Money.Create(10m, "RSD"), 1));
    }

    [Fact]
    public void AddItem_WithDefaultProductId_ShouldThrow()
    {
        var order = CreateDraftOrder();

        Assert.Throws<DomainException>(
            () => order.AddItem(default, "Laptop stand", Money.Create(10m, "EUR"), 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public void AddItem_WithInvalidQuantity_ShouldThrow(int quantity)
    {
        var order = CreateDraftOrder();

        Assert.Throws<DomainException>(
            () => order.AddItem(ProductId.New(), "Laptop stand", Money.Create(10m, "EUR"), quantity));
    }

    [Fact]
    public void AddItem_WhenMergedQuantityExceedsMax_ShouldThrowAndKeepPreviousState()
    {
        var order = CreateDraftOrder();
        var productId = ProductId.New();
        order.AddItem(productId, "Laptop stand", Money.Create(10m, "EUR"), 600);

        Assert.Throws<DomainException>(
            () => order.AddItem(productId, "Laptop stand", Money.Create(10m, "EUR"), 600));

        var item = Assert.Single(order.Items);
        Assert.Equal(600, item.Quantity);
    }

    [Fact]
    public void AddItem_BeyondMaxItems_ShouldThrow()
    {
        var order = CreateDraftOrder();

        for (var i = 0; i < Order.MaxItems; i++)
        {
            order.AddItem(ProductId.New(), "Product", Money.Create(1m, "EUR"), 1);
        }

        Assert.Throws<DomainException>(
            () => order.AddItem(ProductId.New(), "Product", Money.Create(1m, "EUR"), 1));
        Assert.Equal(Order.MaxItems, order.Items.Count);
    }

    [Fact]
    public void AddItem_AfterPlaced_ShouldThrow()
    {
        var order = CreatePlacedOrder();

        Assert.Throws<DomainException>(
            () => order.AddItem(ProductId.New(), "Laptop stand", Money.Create(10m, "EUR"), 1));
    }

    

    [Fact]
    public void RemoveItem_ShouldRemoveItem()
    {
        var order = CreateDraftOrder();
        order.AddItem(ProductId.New(), "Laptop stand", Money.Create(10m, "EUR"), 2);
        var itemId = Assert.Single(order.Items).Id;

        order.RemoveItem(itemId);

        Assert.Empty(order.Items);
        Assert.Equal(Money.Zero("EUR"), order.TotalAmount);
    }

    [Fact]
    public void RemoveItem_UnknownItem_ShouldThrow()
    {
        var order = CreateOrderWithItem();

        Assert.Throws<DomainException>(() => order.RemoveItem(OrderItemId.New()));
    }

    [Fact]
    public void RemoveItem_AfterPlaced_ShouldThrow()
    {
        var order = CreatePlacedOrder();
        var itemId = Assert.Single(order.Items).Id;

        Assert.Throws<DomainException>(() => order.RemoveItem(itemId));
    }

    

    [Fact]
    public void Place_ShouldChangeStatusAndRaiseEvent()
    {
        var order = CreateOrderWithItem();
        var item = Assert.Single(order.Items);

        order.Place();

        Assert.Equal(OrderStatus.Placed, order.Status);

        var domainEvent = Assert.IsType<OrderPlacedDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, domainEvent.OrderId);
        Assert.Equal(order.CustomerId, domainEvent.CustomerId);
        Assert.Equal(Money.Create(20m, "EUR"), domainEvent.TotalAmount);

        var placedItem = Assert.Single(domainEvent.Items);
        Assert.Equal(item.ProductId, placedItem.ProductId);
        Assert.Equal(2, placedItem.Quantity);
    }

    [Fact]
    public void Place_WithoutItems_ShouldThrowAndStayDraft()
    {
        var order = CreateDraftOrder();

        Assert.Throws<DomainException>(() => order.Place());

        Assert.Equal(OrderStatus.Draft, order.Status);
        Assert.Empty(order.DomainEvents);
    }

    [Fact]
    public void Place_Twice_ShouldThrow()
    {
        var order = CreatePlacedOrder();

        Assert.Throws<DomainException>(() => order.Place());
    }

    

    [Fact]
    public void MarkAsPaid_ShouldChangeStatusAndRaiseEvent()
    {
        var order = CreatePlacedOrder();

        order.MarkAsPaid(order.TotalAmount);

        Assert.Equal(OrderStatus.Paid, order.Status);

        var domainEvent = Assert.IsType<OrderPaidDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, domainEvent.OrderId);
        Assert.Equal(Money.Create(20m, "EUR"), domainEvent.Amount);
    }

    [Fact]
    public void MarkAsPaid_WithWrongAmount_ShouldThrowAndKeepStatus()
    {
        var order = CreatePlacedOrder();

        Assert.Throws<DomainException>(() => order.MarkAsPaid(Money.Create(1m, "EUR")));

        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Empty(order.DomainEvents);
    }

    [Fact]
    public void MarkAsPaid_WhenDraft_ShouldThrow()
    {
        var order = CreateOrderWithItem();

        Assert.Throws<DomainException>(() => order.MarkAsPaid(order.TotalAmount));
    }

    

    [Fact]
    public void Ship_WhenPaid_ShouldChangeStatusAndRaiseEvent()
    {
        var order = CreatePaidOrder();

        order.Ship();

        Assert.Equal(OrderStatus.Shipped, order.Status);

        var domainEvent = Assert.IsType<OrderShippedDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, domainEvent.OrderId);
    }

    [Fact]
    public void Ship_WhenNotPaid_ShouldThrow()
    {
        var order = CreatePlacedOrder();

        Assert.Throws<DomainException>(() => order.Ship());

        Assert.Equal(OrderStatus.Placed, order.Status);
    }

    [Fact]
    public void Deliver_WhenShipped_ShouldChangeStatusAndRaiseEvent()
    {
        var order = CreateShippedOrder();

        order.Deliver();

        Assert.Equal(OrderStatus.Delivered, order.Status);

        var domainEvent = Assert.IsType<OrderDeliveredDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, domainEvent.OrderId);
    }

    [Fact]
    public void Deliver_WhenNotShipped_ShouldThrow()
    {
        var order = CreatePaidOrder();

        Assert.Throws<DomainException>(() => order.Deliver());

        Assert.Equal(OrderStatus.Paid, order.Status);
    }

  

    [Fact]
    public void Cancel_WhenPlaced_ShouldChangeStatusAndRaiseEventWithPreviousStatus()
    {
        var order = CreatePlacedOrder();

        order.Cancel("  Kupac se predomislio  ");

        Assert.Equal(OrderStatus.Cancelled, order.Status);

        var domainEvent = Assert.IsType<OrderCancelledDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, domainEvent.OrderId);
        Assert.Equal(OrderStatus.Placed, domainEvent.PreviousStatus);
        Assert.Equal("Kupac se predomislio", domainEvent.Reason);
    }

    [Fact]
    public void Cancel_WhenPaid_ShouldRaiseEventWithPaidAsPreviousStatus()
    {
        var order = CreatePaidOrder();

        order.Cancel("Kupac se predomislio");

        var domainEvent = Assert.IsType<OrderCancelledDomainEvent>(Assert.Single(order.DomainEvents));
        Assert.Equal(OrderStatus.Paid, domainEvent.PreviousStatus);
    }

    [Fact]
    public void Cancel_WhenDraft_ShouldThrow()
    {
        var order = CreateOrderWithItem();

        Assert.Throws<DomainException>(() => order.Cancel("Razlog"));

        Assert.Equal(OrderStatus.Draft, order.Status);
    }

    [Fact]
    public void Cancel_WhenShipped_ShouldThrow()
    {
        var order = CreateShippedOrder();

        Assert.Throws<DomainException>(() => order.Cancel("Razlog"));

        Assert.Equal(OrderStatus.Shipped, order.Status);
    }

    [Fact]
    public void Cancel_WhenDelivered_ShouldThrow()
    {
        var order = CreateShippedOrder();
        order.Deliver();

        Assert.Throws<DomainException>(() => order.Cancel("Razlog"));

        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrow()
    {
        var order = CreatePlacedOrder();
        order.Cancel("Prvi razlog");

        Assert.Throws<DomainException>(() => order.Cancel("Drugi razlog"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancel_WithoutReason_ShouldThrowAndKeepStatus(string reason)
    {
        var order = CreatePlacedOrder();

        Assert.Throws<DomainException>(() => order.Cancel(reason));

        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Empty(order.DomainEvents);
    }

    [Fact]
    public void Cancel_WithTooLongReason_ShouldThrow()
    {
        var order = CreatePlacedOrder();
        var tooLong = new string('a', Order.MaxCancellationReasonLength + 1);

        Assert.Throws<DomainException>(() => order.Cancel(tooLong));
    }


    [Fact]
    public void FullLifecycle_ShouldRaiseEventsInOrder()
    {
        var order = CreateOrderWithItem();

        order.Place();
        order.MarkAsPaid(order.TotalAmount);
        order.Ship();
        order.Deliver();

        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Collection(
            order.DomainEvents,
            first => Assert.IsType<OrderPlacedDomainEvent>(first),
            second => Assert.IsType<OrderPaidDomainEvent>(second),
            third => Assert.IsType<OrderShippedDomainEvent>(third),
            fourth => Assert.IsType<OrderDeliveredDomainEvent>(fourth));
    }

   

    private static Address CreateAddress() =>
        Address.Create("Knez Mihailova 1", "Beograd", "11000", "Srbija");

    private static Order CreateDraftOrder() =>
        Order.Create(CustomerId.New(), CreateAddress(), "EUR");

    private static Order CreateOrderWithItem()
    {
        var order = CreateDraftOrder();
        order.AddItem(ProductId.New(), "Laptop stand", Money.Create(10m, "EUR"), 2);

        return order;
    }

    private static Order CreatePlacedOrder()
    {
        var order = CreateOrderWithItem();
        order.Place();
        order.ClearDomainEvents();

        return order;
    }

    private static Order CreatePaidOrder()
    {
        var order = CreatePlacedOrder();
        order.MarkAsPaid(order.TotalAmount);
        order.ClearDomainEvents();

        return order;
    }

    private static Order CreateShippedOrder()
    {
        var order = CreatePaidOrder();
        order.Ship();
        order.ClearDomainEvents();

        return order;
    }
}