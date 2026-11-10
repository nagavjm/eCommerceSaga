using BuildingBlocks.Observability.Correlation;
using BuildingBlocks.Observability.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Observability;

public static class DependencyInjection
{
    public static IServiceCollection AddObservability(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
        return services;
    }

    public static IApplicationBuilder UseObservability(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();
        return app;
    }
}
