using MediatR;
using OrderFulfillment.Application.Common;

namespace OrderFulfillment.Application.Orders.Commands.DeliverOrder;

public sealed record DeliverOrderCommand(Guid OrderId) : IRequest<Result>;