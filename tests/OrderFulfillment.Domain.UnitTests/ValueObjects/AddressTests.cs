using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.ValueObjects;
using Xunit;

namespace OrderFulfillment.Domain.UnitTests.ValueObjects;

public sealed class AddressTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var address = Address.Create("Knez Mihailova 1", "Beograd", "11000", "Srbija");

        Assert.Equal("Knez Mihailova 1", address.Street);
        Assert.Equal("Beograd", address.City);
        Assert.Equal("11000", address.PostalCode);
        Assert.Equal("Srbija", address.Country);
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        var address = Address.Create("  Knez Mihailova 1 ", " Beograd", "11000 ", " Srbija ");

        Assert.Equal("Knez Mihailova 1", address.Street);
        Assert.Equal("Beograd", address.City);
        Assert.Equal("11000", address.PostalCode);
        Assert.Equal("Srbija", address.Country);
    }

    [Theory]
    [InlineData("", "Beograd", "11000", "Srbija")]
    [InlineData("Knez Mihailova 1", "", "11000", "Srbija")]
    [InlineData("Knez Mihailova 1", "Beograd", "   ", "Srbija")]
    [InlineData("Knez Mihailova 1", "Beograd", "11000", "")]
    public void Create_WithMissingField_ShouldThrow(
        string street,
        string city,
        string postalCode,
        string country)
    {
        Assert.Throws<DomainException>(() => Address.Create(street, city, postalCode, country));
    }

    [Fact]
    public void Create_WithTooLongStreet_ShouldThrow()
    {
        var tooLong = new string('a', 201);

        Assert.Throws<DomainException>(
            () => Address.Create(tooLong, "Beograd", "11000", "Srbija"));
    }

    [Fact]
    public void Create_WithStreetAtMaxLength_ShouldSucceed()
    {
        var maxLength = new string('a', 200);

        var address = Address.Create(maxLength, "Beograd", "11000", "Srbija");

        Assert.Equal(200, address.Street.Length);
    }

    [Fact]
    public void Addresses_WithSameValues_ShouldBeEqual()
    {
        var first = Address.Create("Knez Mihailova 1", "Beograd", "11000", "Srbija");
        var second = Address.Create(" Knez Mihailova 1", "Beograd ", "11000", "Srbija");

        Assert.Equal(first, second);
        Assert.True(first == second);
    }

    [Fact]
    public void Addresses_WithDifferentValues_ShouldNotBeEqual()
    {
        var first = Address.Create("Knez Mihailova 1", "Beograd", "11000", "Srbija");
        var second = Address.Create("Knez Mihailova 2", "Beograd", "11000", "Srbija");

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }
}