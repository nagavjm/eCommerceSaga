using BuildingBlocks.Observability.Correlation;
using BuildingBlocks.Observability.HealthChecks;
using BuildingBlocks.Observability.Middlewares;
using BuildingBlocks.Observability.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Observability;

public static class DependencyInjection
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        string serviceName = "EcommerceService")
    {
        var config = configuration ?? new ConfigurationBuilder().Build();

        services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
        services.AddCustomHealthChecks(config, serviceName);
        services.AddCustomCors(config);

        return services;
    }

    public static IApplicationBuilder UseObservability(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseCustomCors();
        app.UseMiddleware<RateLimitingMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();

        return app;
    }
}
