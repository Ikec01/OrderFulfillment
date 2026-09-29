using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.Orders.Events;
using OrderFulfillment.Domain.ValueObjects;

namespace OrderFulfillment.Domain.Orders
{
    public sealed class Order : AggregateRoot<OrderId>
    {
        public const int MaxItems = 100;
        public const int MaxCancellationReasonLength = 500;

        private readonly List<OrderItem> _items = [];

        private Order(
            OrderId id,
            CustomerId customerId,
            Address shippingAddress,
            string currency,
            DateTime createdAtUtc) : base(id)
        {
            CustomerId = customerId;
            ShippingAddress = shippingAddress;
            Currency = currency;
            CreatedAtUtc = createdAtUtc;
            Status = OrderStatus.Draft;
        }
        private Order()
        {

        }
        public CustomerId CustomerId { get; private set; }
        public Address ShippingAddress { get; private set; } = default!;
        public string Currency { get; private set; } = string.Empty;
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        public Money TotalAmount => _items.Aggregate(Money.Zero(Currency), (total, item) => total.Add(item.ToralPrice));

        public static Order Create(CustomerId customerId, Address shippingAddress, string currency)
        {
            if (customerId == default)
            {
                throw new DomainException("Identifikator kupca je obavezan.");
            }
            ArgumentNullException.ThrowIfNull(shippingAddress);

            var normalizedCurrency = Money.Zero(currency).Currency;

            return new Order(
                OrderId.New(),
                customerId,
                shippingAddress,
                normalizedCurrency,
                DateTime.UtcNow);
        }

        public void AddItem(ProductId productId, string productName, Money unitPrice, int quantity)
        {
            ArgumentNullException.ThrowIfNull(unitPrice);
            EnsureStatus(OrderStatus.Draft, "dodati stavku");

            if(unitPrice.Currency != Currency)
            {
                throw new DomainException($"Valuta stavke ({unitPrice.Currency}) mora biti ista kao valuta porudžbine ({Currency}).");
            }

            var existingItem = _items.Find(item => item.ProductId == productId);

            if(existingItem is not null)
            {
                if(existingItem.UnitPrice != unitPrice)
                {
                    throw new DomainException("Isti proizvod ne može biti dodat sa različitom cenom u istoj porudžbini.");
                }
                existingItem.IncreaseQuantity(quantity);
                return;
            }
            if(_items.Count >= MaxItems)
            {
                throw new DomainException($"Porudžbina ne može imati više od {MaxItems} stavki.");
            }
            _items.Add(OrderItem.Create(productId, productName, unitPrice, quantity));
        }

        public void RemoveItem(OrderItemId itemId)
        {
            EnsureStatus(OrderStatus.Draft, "ukloniti stavku");

            var item = _items.Find(existing => existing.Id == itemId)
                ?? throw new DomainException("Stavka ne postoji u porudžbini.");

            _items.Remove(item);
        }
        public void Place()
        {
            EnsureStatus(OrderStatus.Draft, "potvrditi porudžbinu");

            if (_items.Count == 0)
            {
                throw new DomainException("Porudžbina mora imati bar jednu stavku.");
            }

            Status = OrderStatus.Placed;

            var itemsSnapshot = _items
                .Select(item => new OrderPlacedItem(item.ProductId, item.Quantity))
                .ToList()
                .AsReadOnly();

            RaiseDomainEvent(new OrderPlacedDomainEvent(Id, CustomerId, TotalAmount, itemsSnapshot));
        }

        public void MarkAsPaid(Money amount)
        {
            ArgumentNullException.ThrowIfNull(amount);

            EnsureStatus(OrderStatus.Placed, "označiti porudžbinu kao plaćenu");

            var total = TotalAmount;

            if (amount != total)
            {
                throw new DomainException(
                    $"Uplaćeni iznos ({amount}) ne odgovara ukupnom iznosu porudžbine ({total}).");
            }

            Status = OrderStatus.Paid;

            RaiseDomainEvent(new OrderPaidDomainEvent(Id, amount));
        }

        public void Ship()
        {
            EnsureStatus(OrderStatus.Paid, "otpremiti porudžbinu");

            Status = OrderStatus.Shipped;

            RaiseDomainEvent(new OrderShippedDomainEvent(Id));
        }

        public void Deliver()
        {
            EnsureStatus(OrderStatus.Shipped, "označiti porudžbinu kao isporučenu");

            Status = OrderStatus.Delivered;

            RaiseDomainEvent(new OrderDeliveredDomainEvent(Id));
        }

        public void Cancel(string reason)
        {
            if (Status is not (OrderStatus.Placed or OrderStatus.Paid))
            {
                throw new DomainException(
                    $"Nije moguće otkazati porudžbinu dok je u statusu '{Status}'.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new DomainException("Razlog otkazivanja je obavezan.");
            }

            var normalizedReason = reason.Trim();

            if (normalizedReason.Length > MaxCancellationReasonLength)
            {
                throw new DomainException(
                    $"Razlog otkazivanja ne sme biti duži od {MaxCancellationReasonLength} karaktera.");
            }

            var previousStatus = Status;

            Status = OrderStatus.Cancelled;

            RaiseDomainEvent(new OrderCancelledDomainEvent(Id, previousStatus, normalizedReason));
        }




        private void EnsureStatus(OrderStatus expected, string action)
        {
            if (Status != expected)
            {
                throw new DomainException(
                    $"Nije moguće {action} dok je porudžbina u statusu '{Status}'.");
            }
        }
    }
}
