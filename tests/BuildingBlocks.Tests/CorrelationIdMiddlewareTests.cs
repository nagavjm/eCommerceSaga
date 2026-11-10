using BuildingBlocks.Observability.Correlation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BuildingBlocks.Tests;

public class CorrelationIdMiddlewareTests
{
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _loggerMock = new();
    private readonly CorrelationIdProvider _provider = new();

    [Fact]
    public async Task InvokeAsync_WithExistingCorrelationHeader_ShouldPropagateSameId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.CorrelationHeaderKey] = expectedId.ToString();

        var middleware = new CorrelationIdMiddleware(
            next: (ctx) => Task.CompletedTask,
            logger: _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _provider);

        // Assert
        _provider.CorrelationId.Should().Be(expectedId);
    }

    [Fact]
    public async Task InvokeAsync_WithoutCorrelationHeader_ShouldGenerateNewGuid()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var middleware = new CorrelationIdMiddleware(
            next: (ctx) => Task.CompletedTask,
            logger: _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context, _provider);

        // Assert
        _provider.CorrelationId.Should().NotBeEmpty();
    }
}
