using MediatR;
using OrderFulfillment.Application.Common;

namespace OrderFulfillment.Application.Orders.Commands.ShipOrder;

public sealed record ShipOrderCommand(Guid OrderId) : IRequest<Result>;