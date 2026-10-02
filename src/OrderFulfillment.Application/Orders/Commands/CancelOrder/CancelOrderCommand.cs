using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using OrderFulfillment.Application.Common;

namespace OrderFulfillment.Application.Orders.Commands.CancelOrder
{
    public sealed record CancelOrderCommand(Guid OrderId, string Reason) : IRequest<Result>;
    
}
