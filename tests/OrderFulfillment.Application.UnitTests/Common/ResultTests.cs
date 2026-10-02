using OrderFulfillment.Application.Common;
using Xunit;

namespace OrderFulfillment.Application.UnitTests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldHaveNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(ApplicationError.None, result.Error);
    }

    [Fact]
    public void Failure_ShouldCarryTheError()
    {
        var error = new ApplicationError("Test.Code", "Opis", ErrorType.NotFound);

        var result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_WithNoneError_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(ApplicationError.None));
    }

    [Fact]
    public void Failure_WithNullError_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Failure(null!));
    }
}