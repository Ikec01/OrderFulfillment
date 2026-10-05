using FluentValidation;

namespace OrderFulfillment.Application.Orders.Queries.ListMyOrders;

public sealed class ListMyOrdersQueryValidator : AbstractValidator<ListMyOrdersQuery>
{
    public const int MaxPageSize = 100;

    public ListMyOrdersQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}