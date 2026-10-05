using OrderFulfillment.Application.Abstractions;
using OrderFulfillment.Domain.Orders;

namespace OrderFulfillment.Application.Orders;

public static class OrderAccessPolicy
{
    public static bool CanAccess(ICurrentUser currentUser, Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return CanAccess(currentUser, order.CustomerId.Value);
    }

    public static bool CanAccess(ICurrentUser currentUser, Guid ownerId)
    {
        ArgumentNullException.ThrowIfNull(currentUser);

        if (currentUser.IsAdministrator)
        {
            return true;
        }

        return currentUser.UserId is { } userId && userId == ownerId;
    }
}