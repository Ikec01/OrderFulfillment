using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Application.Orders;

public static class OrderAccessPolicy
{
    public static bool CanAccess(ICurrentUser currentUser, Order order)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(order);

        if (currentUser.IsAdministrator)
        {
            return true;
        }

        return currentUser.UserId is { } userId && order.CustomerId.Value == userId;
    }
}