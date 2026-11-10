using BuildingBlocks.Common.Behaviors;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Xunit;
using AppValidationException = BuildingBlocks.Common.Exceptions.ValidationException;

namespace BuildingBlocks.Tests;

public class ValidationBehaviorTests
{
    public record SampleCommand(string Name, decimal Price) : IRequest<string>;

    public class SampleCommandValidator : AbstractValidator<SampleCommand>
    {
        public SampleCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be positive.");
        }
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallNextDelegate()
    {
        // Arrange
        var validators = new IValidator<SampleCommand>[] { new SampleCommandValidator() };
        var behavior = new ValidationBehavior<SampleCommand, string>(validators);
        var command = new SampleCommand("Product A", 19.99m);

        // Act
        var result = await behavior.Handle(command, (ct) => Task.FromResult("Success"), CancellationToken.None);

        // Assert
        result.Should().Be("Success");
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var validators = new IValidator<SampleCommand>[] { new SampleCommandValidator() };
        var behavior = new ValidationBehavior<SampleCommand, string>(validators);
        var command = new SampleCommand(string.Empty, -5m);

        // Act
        var act = async () => await behavior.Handle(command, (ct) => Task.FromResult("Success"), CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<AppValidationException>();
        exception.Which.Errors.Should().ContainKey("Name");
        exception.Which.Errors.Should().ContainKey("Price");
    }
}
