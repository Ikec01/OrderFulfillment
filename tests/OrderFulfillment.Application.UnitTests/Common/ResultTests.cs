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

    [Fact]
    public void SuccessWithValue_ShouldExposeValue()
    {
        var result = Result.Success("vrednost");

        Assert.True(result.IsSuccess);
        Assert.Equal("vrednost", result.Value);
        Assert.Equal(ApplicationError.None, result.Error);
    }

    [Fact]
    public void FailureWithValueType_ShouldCarryErrorAndNotExposeValue()
    {
        var error = new ApplicationError("Test.Code", "Opis", ErrorType.NotFound);

        var result = Result.Failure<string>(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void FailureWithValueType_WithNoneError_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure<string>(ApplicationError.None));
    }
}