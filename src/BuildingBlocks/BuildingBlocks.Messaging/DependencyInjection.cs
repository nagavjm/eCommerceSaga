using Azure.Messaging.ServiceBus;
using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Messaging.Inbox;
using BuildingBlocks.Messaging.Outbox;
using BuildingBlocks.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
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

        // Always register in-memory bus for local testing and dual-dispatch
        var inMemoryBus = new InMemoryMessageBus(
            services.BuildServiceProvider().GetService<Microsoft.Extensions.Logging.ILogger<InMemoryMessageBus>>()
            ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryMessageBus>.Instance);

        services.AddSingleton(inMemoryBus);
        services.AddSingleton<IInMemoryMessageSubscriber>(inMemoryBus);

        var connectionString = configuration.GetSection(AzureServiceBusOptions.SectionName)["ConnectionString"];

        if (!string.IsNullOrWhiteSpace(connectionString) && !connectionString.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton(_ => new ServiceBusClient(connectionString));
            services.AddScoped<IMessagePublisher, AzureServiceBusMessagePublisher>();
        }
        else
        {
            services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<InMemoryMessageBus>());
        }

        services.AddHostedService<OutboxProcessorBackgroundService>();

        return services;
    }

    public static IServiceCollection AddOutboxStore<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IOutboxStore, EfOutboxStore<TDbContext>>();
        return services;
    }

    public static IServiceCollection AddInboxStore<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IInboxStore, EfInboxStore<TDbContext>>();
        return services;
    }
}
