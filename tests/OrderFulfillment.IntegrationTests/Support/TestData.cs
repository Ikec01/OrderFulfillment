using OrderFulfillment.Domain.Orders;
using OrderFulfillment.Domain.ValueObjects;

namespace OrderFulfillment.IntegrationTests.Support;

internal static class TestData
{
    public static Address CreateAddress() =>
        Address.Create("Knez Mihailova 1", "Beograd", "11000", "Srbija");

    public static Order CreateDraftOrder(Guid customerId, bool withItem = true)
    {
        var order = Order.Create(new CustomerId(customerId), CreateAddress(), "EUR");

        if (withItem)
        {
            order.AddItem(ProductId.New(), "Laptop stand", Money.Create(10m, "EUR"), 2);
        }

        return order;
    }

    public static Order CreatePlacedOrder(Guid customerId)
    {
        var order = CreateDraftOrder(customerId);
        order.Place();

        return order;
    }
}