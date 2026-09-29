using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using OrderFulfillment.Domain.Common;

namespace OrderFulfillment.Domain.ValueObjects
{
    public sealed class Address : ValueObject
    {
        public const int MaxStreetLenght = 200;
        public const int MaxCityLength = 100;
        public const int MaxPostalCodeLength = 20;
        public const int MaxCountryLength = 100;

        private Address(string street, string city, string postalCode, string country)
        {
            Street = street;
            City = city;
            PostalCode = postalCode;
            Country = country;
        }

        public string Street { get; }
        public string City { get; }
        public string PostalCode { get; }
        public string Country { get; }

        public static Address Create(string street, string city, string postalCode, string country)
        {
            var normalizedStreet = Normailze(street, "Ulica", MaxStreetLenght);
            var normalizedCity = Normailze(city, "Grad", MaxCityLength);
            var normalizedPostalCode = Normailze(postalCode, "Postanski broj", MaxPostalCodeLength);
            var normalizedCountry = Normailze(country, "Drzava", MaxCountryLength);

            return new Address(normalizedStreet, normalizedCity, normalizedPostalCode, normalizedCountry);
        }

        public override string ToString() => $"{Street}, {PostalCode} {City}, {Country}";

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return PostalCode;
            yield return Country;
        }

        private static string Normailze(string value, string fieldName, int maxLenght)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException($"Polje '{fieldName}' je obavezno.");
            }

            var trimmed = value.Trim();

            if(trimmed.Length > maxLenght)
            {
                throw new DomainException($"Polje '{fieldName}' ne sme biti duze od {maxLenght} karaktera.");
            }
            return trimmed;
        }
    }
}
