using OrderFulfillment.Domain.Common;
using OrderFulfillment.Domain.ValueObjects;
using Xunit;

namespace OrderFulfillment.Domain.UnitTests.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var money = Money.Create(10.50m, "EUR");

        Assert.Equal(10.50m, money.Amount);
        Assert.Equal("EUR", money.Currency);
    }

    [Fact]
    public void Create_ShouldNormalizeCurrency()
    {
        var money = Money.Create(5m, "  eur ");

        Assert.Equal("EUR", money.Currency);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrow()
    {
        Assert.Throws<DomainException>(() => Money.Create(-1m, "EUR"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("EU")]
    [InlineData("EURO")]
    [InlineData("12E")]
    public void Create_WithInvalidCurrency_ShouldThrow(string currency)
    {
        Assert.Throws<DomainException>(() => Money.Create(10m, currency));
    }

    [Theory]
    [InlineData(2.345, 2.34)]
    [InlineData(2.355, 2.36)]
    [InlineData(2.344, 2.34)]
    [InlineData(2.346, 2.35)]
    public void Create_ShouldRoundToTwoDecimals_UsingBankersRounding(double input, double expected)
    {
        var money = Money.Create((decimal)input, "EUR");

        Assert.Equal((decimal)expected, money.Amount);
    }

    [Fact]
    public void Money_WithSameAmountAndCurrency_ShouldBeEqual()
    {
        var first = Money.Create(5m, "EUR");
        var second = Money.Create(5m, "eur");

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Money_WithDifferentCurrency_ShouldNotBeEqual()
    {
        var euros = Money.Create(5m, "EUR");
        var dinars = Money.Create(5m, "RSD");

        Assert.NotEqual(euros, dinars);
    }

    [Fact]
    public void Zero_ShouldHaveZeroAmount()
    {
        var zero = Money.Zero("EUR");

        Assert.Equal(0m, zero.Amount);
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldSumAmounts()
    {
        var result = Money.Create(5m, "EUR").Add(Money.Create(7.25m, "EUR"));

        Assert.Equal(12.25m, result.Amount);
        Assert.Equal("EUR", result.Currency);
    }

    [Fact]
    public void Add_ShouldNotModifyOriginalValues()
    {
        var original = Money.Create(5m, "EUR");

        _ = original.Add(Money.Create(3m, "EUR"));

        Assert.Equal(5m, original.Amount);
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrow()
    {
        var euros = Money.Create(5m, "EUR");
        var dinars = Money.Create(5m, "RSD");

        Assert.Throws<DomainException>(() => euros.Add(dinars));
    }

    [Fact]
    public void Multiply_ShouldScaleAmount()
    {
        var result = Money.Create(4.50m, "EUR").Multiply(3);

        Assert.Equal(13.50m, result.Amount);
    }

    [Fact]
    public void Multiply_WithNegativeQuantity_ShouldThrow()
    {
        var money = Money.Create(4.50m, "EUR");

        Assert.Throws<DomainException>(() => money.Multiply(-1));
    }
}