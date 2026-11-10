using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Observability.Security;

public class RateLimiterOptions
{
    public int PermitLimit { get; set; } = 300;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
}

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimiterOptions _options;
    private static readonly ConcurrentDictionary<string, ClientRequestTracker> Trackers = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        RateLimiterOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new RateLimiterOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Exclude Swagger and health check endpoints from rate limiting
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (path.StartsWith("/swagger") || path.StartsWith("/health"))
        {
            await _next(context);
            return;
        }

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var tracker = Trackers.GetOrAdd(clientIp, _ => new ClientRequestTracker());

        if (!tracker.TryAcquire(_options.PermitLimit, _options.Window))
        {
            _logger.LogWarning("Rate limit exceeded for client IP: {ClientIp}", clientIp);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/problem+json";
            context.Response.Headers["Retry-After"] = ((int)_options.Window.TotalSeconds).ToString();

            var problemDetails = new
            {
                type = "https://httpstatuses.com/429",
                title = "Too Many Requests",
                status = 429,
                detail = $"Request quota of {_options.PermitLimit} requests per {_options.Window.TotalSeconds}s exceeded. Please retry later.",
                instance = context.Request.Path.Value
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
            return;
        }

        await _next(context);
    }

    private class ClientRequestTracker
    {
        private readonly object _lock = new();
        private int _counter;
        private DateTime _windowStart = DateTime.UtcNow;

        public bool TryAcquire(int limit, TimeSpan window)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                if (now - _windowStart >= window)
                {
                    _windowStart = now;
                    _counter = 0;
                }

                if (_counter < limit)
                {
                    _counter++;
                    return true;
                }

                return false;
            }
        }
    }
}
