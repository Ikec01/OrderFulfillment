using FluentValidation;

namespace OrderFulfillment.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(query => query.OrderId).NotEmpty();
    }
}