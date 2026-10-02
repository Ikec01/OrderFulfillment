using OrderFulfillment.Application.Orders.Commands.CancelOrder;
using OrderFulfillment.Domain.Orders;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Orders;

public sealed class CancelOrderCommandValidatorTests
{
    private readonly CancelOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var result = _validator.Validate(new CancelOrderCommand(Guid.NewGuid(), "Razlog"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyOrderId_ShouldFail()
    {
        var result = _validator.Validate(new CancelOrderCommand(Guid.Empty, "Razlog"));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithoutReason_ShouldFail(string reason)
    {
        var result = _validator.Validate(new CancelOrderCommand(Guid.NewGuid(), reason));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithTooLongReason_ShouldFail()
    {
        var reason = new string('a', Order.MaxCancellationReasonLength + 1);

        var result = _validator.Validate(new CancelOrderCommand(Guid.NewGuid(), reason));

        Assert.False(result.IsValid);
    }
}