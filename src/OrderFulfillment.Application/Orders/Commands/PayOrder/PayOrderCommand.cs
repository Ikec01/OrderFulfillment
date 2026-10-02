using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using OrderFulfillment.Application.Common;

namespace OrderFulfillment.Application.Orders.Commands.PayOrder
{
    public sealed record PayOrderCommand(Guid OrderId, decimal Amount, string Currency) : IRequest<Result>;
    
}
