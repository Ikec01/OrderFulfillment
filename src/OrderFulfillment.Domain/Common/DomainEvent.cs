using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFulfillment.Domain.Common
{
    public abstract record DomainEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }
}
