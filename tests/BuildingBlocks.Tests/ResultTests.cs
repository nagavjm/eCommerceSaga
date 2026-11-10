using BuildingBlocks.Common.Results;
using FluentAssertions;
using Xunit;

namespace BuildingBlocks.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailureResult_WithError()
    {
        // Arrange
        var error = Error.NotFound("Product.NotFound", "Product does not exist.");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void GenericResult_Success_ShouldContainValue()
    {
        // Arrange
        var testValue = "Test Order";

        // Act
        var result = Result.Success(testValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(testValue);
    }

    [Fact]
    public void GenericResult_Failure_AccessingValue_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var error = Error.Failure("Order.Invalid", "Invalid order");
        var result = Result.Failure<string>(error);

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GenericResult_ImplicitConversion_ShouldWrapValueInSuccessResult()
    {
        // Act
        Result<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }
}
