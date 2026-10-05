using MediatR;
using OrderFulfillment.Application.Common;

namespace OrderFulfillment.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<OrderDetailsDto>>;