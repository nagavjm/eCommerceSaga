using System.Net;
using BuildingBlocks.Observability.Security;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace BuildingBlocks.Tests;

public class RateLimitingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenWithinLimit_ShouldPassThrough()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        var passedThrough = false;
        var middleware = new RateLimitingMiddleware(
            ctx =>
            {
                passedThrough = true;
                return Task.CompletedTask;
            },
            NullLogger<RateLimitingMiddleware>.Instance,
            new RateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        passedThrough.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_WhenLimitExceeded_ShouldReturn429TooManyRequests()
    {
        // Arrange
        var options = new RateLimiterOptions { PermitLimit = 2, Window = TimeSpan.FromMinutes(1) };
        var middleware = new RateLimitingMiddleware(
            ctx => Task.CompletedTask,
            NullLogger<RateLimitingMiddleware>.Instance,
            options);

        var testIp = IPAddress.Parse("192.168.1.99");

        // Act - Call 2 times within limit
        for (var i = 0; i < 2; i++)
        {
            var ctx = new DefaultHttpContext();
            ctx.Connection.RemoteIpAddress = testIp;
            await middleware.InvokeAsync(ctx);
            ctx.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        // 3rd call exceeds limit
        var exceededContext = new DefaultHttpContext();
        exceededContext.Connection.RemoteIpAddress = testIp;
        await middleware.InvokeAsync(exceededContext);

        // Assert
        exceededContext.Response.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        exceededContext.Response.Headers.Should().ContainKey("Retry-After");
    }

    [Fact]
    public async Task InvokeAsync_WhenSwaggerOrHealthPath_ShouldBypassRateLimiter()
    {
        // Arrange
        var options = new RateLimiterOptions { PermitLimit = 0, Window = TimeSpan.FromMinutes(1) };
        var passed = false;
        var middleware = new RateLimitingMiddleware(
            ctx =>
            {
                passed = true;
                return Task.CompletedTask;
            },
            NullLogger<RateLimitingMiddleware>.Instance,
            options);

        var context = new DefaultHttpContext();
        context.Request.Path = "/health/live";
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        passed.Should().BeTrue();
    }
}
