using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFulfillment.Domain.Common;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}
