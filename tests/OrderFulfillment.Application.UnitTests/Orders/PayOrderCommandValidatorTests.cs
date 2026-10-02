using OrderFulfillment.Application.Orders.Commands.PayOrder;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Orders;

public sealed class PayOrderCommandValidatorTests
{
    private readonly PayOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var result = _validator.Validate(new PayOrderCommand(Guid.NewGuid(), 20m, "EUR"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyOrderId_ShouldFail()
    {
        var result = _validator.Validate(new PayOrderCommand(Guid.Empty, 20m, "EUR"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNegativeAmount_ShouldFail()
    {
        var result = _validator.Validate(new PayOrderCommand(Guid.NewGuid(), -1m, "EUR"));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("12E")]
    public void Validate_WithInvalidCurrency_ShouldFail(string currency)
    {
        var result = _validator.Validate(new PayOrderCommand(Guid.NewGuid(), 20m, currency));

        Assert.False(result.IsValid);
    }
}