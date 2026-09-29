using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OrderFulfillment.Domain.Orders
{
    public sealed class OrderItem : Entity<OrderItemId>
    {
        public const int MaxProductNameLenght = 200;
        public const int MaxQuantity = 1000;

        private OrderItem(
            OrderItemId id,
            ProductId productId,
            string productName,
            Money unitPrice,
            int quantity)
            : base(id)
        {
            ProductId = productId;
            ProductName = productName;
            UnitPrice = unitPrice;
            Quantity = quantity;
        }

        private OrderItem()
        { 
        }
        public ProductId ProductId { get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        public Money UnitPrice { get; private set; } = default!;
        public int Quantity { get; private set; }

        public Money ToralPrice => UnitPrice.Multiply(Quantity);

        internal static OrderItem Create(
            ProductId productId,
            string productName,
            Money unitPrice,
            int quantity)
        {
            if(productId == default)
            {
                throw new DomainException("Identifikator proizcoda je obavezan.");
            }
            ArgumentNullException.ThrowIfNull(unitPrice);

            if (string.IsNullOrWhiteSpace(productName))
            {
                throw new DomainException("Naziv proizvoda je obavezan.");
            }

            var normalizedName = productName.Trim();

            if(normalizedName.Length > MaxProductNameLenght)
            {
                throw new DomainException(
                    $"Naziv proizcoda ne sme biti duzi od {MaxProductNameLenght} karaktera.");
            }
            EnsureValidQuantity(quantity);

            return new OrderItem(OrderItemId.New(), productId, normalizedName, unitPrice, quantity);
        }

        internal void IncreaseQuantity(int additionalQuantity)
        {
            EnsureValidQuantity(additionalQuantity);

            var newQuantity = Quantity + additionalQuantity;

            EnsureValidQuantity(newQuantity);
            Quantity = newQuantity;
        }

        private static void EnsureValidQuantity(int quantity)
        {
            if(quantity <= 0)
            {
                throw new DomainException("Kolicina mora biti veca od nule.");
            }
            if(quantity > MaxQuantity)
            {
                throw new DomainException($"Kolicina ne moze biti veca od {MaxQuantity}.");
            }
        }
        
    }
}
