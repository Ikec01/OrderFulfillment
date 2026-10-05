using OrderFulfillment.Application.Orders.Queries.ListMyOrders;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Orders;

public sealed class ListMyOrdersQueryValidatorTests
{
    private readonly ListMyOrdersQueryValidator _validator = new();

    [Fact]
    public void Validate_WithDefaults_ShouldPass()
    {
        var result = _validator.Validate(new ListMyOrdersQuery());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidPage_ShouldFail(int page)
    {
        var result = _validator.Validate(new ListMyOrdersQuery(Page: page));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(101)]
    public void Validate_WithInvalidPageSize_ShouldFail(int pageSize)
    {
        var result = _validator.Validate(new ListMyOrdersQuery(PageSize: pageSize));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Validate_WithPageSizeAtBoundary_ShouldPass(int pageSize)
    {
        var result = _validator.Validate(new ListMyOrdersQuery(PageSize: pageSize));

        Assert.True(result.IsValid);
    }
}