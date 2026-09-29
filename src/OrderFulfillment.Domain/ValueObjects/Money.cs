using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using OrderFulfillment.Domain.Common;

namespace OrderFulfillment.Domain.ValueObjects
{
    public sealed class Money : ValueObject
    {
        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public decimal Amount { get; }
        public string Currency { get; }

        public static Money Create(decimal amount, string currency)
        {
            if(amount < 0)
            {
                throw new DomainException("Iznos ne moze biti negativan.");
            }

            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new DomainException("Valuta je obavezna");
            }

            var normalizedCurrency = currency.Trim().ToUpperInvariant();

            if(normalizedCurrency.Length != 3 || !normalizedCurrency.All(char.IsAsciiLetter))
            {
                throw new DomainException("Valuta mora biti ISO 4217 kod od tri slova (npr. EUR).");
            }

            var roundAmount = Math.Round(amount, 2, MidpointRounding.ToEven);

            return new Money(roundAmount, normalizedCurrency);
        }

        public static Money Zero(string currency) => Create(0m, currency);

        public Money Add(Money other)
        {
            ArgumentNullException.ThrowIfNull(other);
            EnsureSameCurrency(other);

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Multiply(int quantity)
        {
            if(quantity < 0)
            {
                throw new DomainException("Kolicina ne moze biti negativna.");
            }
            return new Money(Amount * quantity, Currency);
        }

        public override string ToString() =>
            $"{Amount.ToString("F2", CultureInfo.InvariantCulture)} {Currency}";

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }

        private void EnsureSameCurrency(Money other)
        {
            if(Currency != other.Currency)
            {
                throw new DomainException($"Nije moguce kombinovati razlicite valute: {Currency} i {other.Currency}.");
            }
        }
    }
}
