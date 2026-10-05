using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.ValueObjects;

namespace OrderFulfillment.Application.UnitTests.Fakes;

internal static class TestOrders
{
    public static Order CreatePlaced(CustomerId? customerId = null)
    {
        var order = Order.Create(
            customerId ?? CustomerId.New(),
            Address.Create("Knez Mihailova 1", "Beograd", "11000", "Srbija"),
            "EUR");
        order.AddItem(ProductId.New(), "Laptop stand", Money.Create(10m, "EUR"), 2);
        order.Place();
        order.ClearDomainEvents();

        return order;
    }

    public static Order CreatePaid(CustomerId? customerId = null)
    {
        var order = CreatePlaced(customerId);
        order.MarkAsPaid(order.TotalAmount);
        order.ClearDomainEvents();

        return order;
    }

    public static Order CreateShipped(CustomerId? customerId = null)
    {
        var order = CreatePaid(customerId);
        order.Ship();
        order.ClearDomainEvents();

        return order;
    }
}