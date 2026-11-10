using BuildingBlocks.Common.Resilience;
using FluentAssertions;
using Polly.Timeout;

namespace BuildingBlocks.Tests;

public class ResiliencePipelineTests
{
    [Fact]
    public async Task HttpPipeline_WhenTransientFailureOccurs_ShouldRetryAndSucceed()
    {
        // Arrange
        var pipeline = ResiliencePipelines.CreateDefaultHttpPipeline(
            maxRetries: 3,
            medianFirstRetryDelay: TimeSpan.FromMilliseconds(10),
            circuitBreakerDuration: TimeSpan.FromSeconds(5),
            requestTimeout: TimeSpan.FromSeconds(5));

        var attempts = 0;

        // Act
        var result = await pipeline.ExecuteAsync(async ct =>
        {
            attempts++;
            if (attempts < 3)
            {
                throw new HttpRequestException("Simulated transient connection reset");
            }
            await Task.Yield();
            return "Success";
        });

        // Assert
        result.Should().Be("Success");
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task HttpPipeline_WhenFailuresExceedMaxRetries_ShouldThrowOriginalException()
    {
        // Arrange
        var pipeline = ResiliencePipelines.CreateDefaultHttpPipeline(
            maxRetries: 2,
            medianFirstRetryDelay: TimeSpan.FromMilliseconds(10));

        var attempts = 0;

        // Act
        var act = async () =>
        {
            await pipeline.ExecuteAsync(async ct =>
            {
                attempts++;
                await Task.Yield();
                throw new HttpRequestException("Permanent downstream failure");
            });
        };

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
        attempts.Should().Be(3); // Initial attempt + 2 retries
    }

    [Fact]
    public async Task HttpPipeline_WhenTimeoutExceeded_ShouldThrowTimeoutRejectedException()
    {
        // Arrange
        var pipeline = ResiliencePipelines.CreateDefaultHttpPipeline(
            maxRetries: 1,
            requestTimeout: TimeSpan.FromMilliseconds(50));

        // Act
        var act = async () =>
        {
            await pipeline.ExecuteAsync(async ct =>
            {
                await Task.Delay(500, ct);
                return "Completed";
            });
        };

        // Assert
        await act.Should().ThrowAsync<TimeoutRejectedException>();
    }

    [Fact]
    public async Task DatabasePipeline_WhenTransientDbError_ShouldRetryAndSucceed()
    {
        // Arrange
        var pipeline = ResiliencePipelines.CreateDefaultDatabasePipeline(
            maxRetries: 3,
            initialDelay: TimeSpan.FromMilliseconds(10));

        var attempts = 0;

        // Act
        var result = await pipeline.ExecuteAsync(async ct =>
        {
            attempts++;
            if (attempts == 1)
            {
                throw new InvalidOperationException("Deadlock detected");
            }
            await Task.Yield();
            return 42;
        });

        // Assert
        result.Should().Be(42);
        attempts.Should().Be(2);
    }
}
