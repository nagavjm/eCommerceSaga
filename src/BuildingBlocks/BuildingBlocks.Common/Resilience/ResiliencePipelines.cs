using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace BuildingBlocks.Common.Resilience;

public static class ResiliencePipelines
{
    public static ResiliencePipeline CreateDefaultHttpPipeline(
        int maxRetries = 3,
        TimeSpan? medianFirstRetryDelay = null,
        TimeSpan? circuitBreakerDuration = null,
        TimeSpan? requestTimeout = null)
    {
        var retryDelay = medianFirstRetryDelay ?? TimeSpan.FromMilliseconds(200);
        var breakDuration = circuitBreakerDuration ?? TimeSpan.FromSeconds(15);
        var timeout = requestTimeout ?? TimeSpan.FromSeconds(10);

        return new ResiliencePipelineBuilder()
            // 1. Total request timeout
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = timeout
            })
            // 2. Retry with exponential backoff and jitter
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = maxRetries,
                Delay = retryDelay,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex => ex is not TimeoutRejectedException)
            })
            // 3. Circuit breaker
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = breakDuration,
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .Build();
    }

    public static ResiliencePipeline CreateDefaultDatabasePipeline(
        int maxRetries = 3,
        TimeSpan? initialDelay = null)
    {
        var delay = initialDelay ?? TimeSpan.FromMilliseconds(100);

        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = maxRetries,
                Delay = delay,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(15)
            })
            .Build();
    }
}
