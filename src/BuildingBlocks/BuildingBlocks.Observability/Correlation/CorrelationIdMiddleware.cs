using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Observability.Correlation;

namespace BuildingBlocks.Observability.Correlation;

public class CorrelationIdMiddleware
{
    public const string CorrelationHeaderKey = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdProvider correlationIdProvider)
    {
        Guid correlationId;

        if (context.Request.Headers.TryGetValue(CorrelationHeaderKey, out var headerValues) &&
            Guid.TryParse(headerValues.FirstOrDefault(), out var parsedId))
        {
            correlationId = parsedId;
        }
        else
        {
            correlationId = Guid.NewGuid();
        }

        correlationIdProvider.SetCorrelationId(correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationHeaderKey] = correlationId.ToString();
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
