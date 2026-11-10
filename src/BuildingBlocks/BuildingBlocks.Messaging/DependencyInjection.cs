using Azure.Messaging.ServiceBus;
using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Messaging.Outbox;
using BuildingBlocks.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AzureServiceBusOptions>(
            configuration.GetSection(AzureServiceBusOptions.SectionName));

        var connectionString = configuration.GetSection(AzureServiceBusOptions.SectionName)["ConnectionString"];

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddSingleton(_ => new ServiceBusClient(connectionString));
            services.AddScoped<IMessagePublisher, AzureServiceBusMessagePublisher>();
        }

        services.AddHostedService<OutboxProcessorBackgroundService>();

        return services;
    }
}
