using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFulfillment.Domain.Common
{
    public abstract class Entity<TId> : IEquatable<Entity<TId>>
        where TId : notnull
    {
        protected Entity(TId id)
        {
            if(EqualityComparer<TId>.Default.Equals(id, default))
            {
                throw new DomainException("Identifikator entiteta ne sme biti podrazumevana vrednost.");
            }
            Id = id;
        }

        protected Entity() { }

        public TId Id { get; protected set; } = default!;

        public bool Equals(Entity<TId>? other)
        {
            if(other is null || other.GetType() != GetType())
            {
                return false;
            }

            return ReferenceEquals(this, other) ||
                EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override bool Equals(object? obj) => obj is Entity<TId> other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(GetType(), Id);

        public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
            left is null ? right is null : left.Equals(right);

        public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
            !(left == right);
        
    }
}
