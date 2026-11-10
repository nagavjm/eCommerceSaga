using System.Text.Json;
using BuildingBlocks.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Outbox;

public class OutboxProcessorBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

    public OutboxProcessorBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during Outbox processing cycle.");
            }
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxStore = scope.ServiceProvider.GetService<IOutboxStore>();
        var publisher = scope.ServiceProvider.GetService<IMessagePublisher>();

        if (outboxStore is null || publisher is null)
        {
            return;
        }

        var messages = await outboxStore.GetUnprocessedMessagesAsync(batchSize: 20, cancellationToken);
        if (messages.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} pending outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var targetType = Type.GetType(message.Type);
                if (targetType is null)
                {
                    _logger.LogError("Could not resolve message type: {Type}", message.Type);
                    await outboxStore.RecordFailureAsync(message.Id, $"Unknown type: {message.Type}", cancellationToken);
                    continue;
                }

                var deserializedPayload = JsonSerializer.Deserialize(message.Payload, targetType);
                if (deserializedPayload is null)
                {
                    await outboxStore.RecordFailureAsync(message.Id, "Deserialized payload is null", cancellationToken);
                    continue;
                }

                await publisher.PublishAsync(deserializedPayload, cancellationToken: cancellationToken);
                await outboxStore.MarkAsProcessedAsync(message.Id, cancellationToken);

                _logger.LogInformation("Successfully published outbox message {MessageId} of type {Type}",
                    message.Id, message.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);
                await outboxStore.RecordFailureAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }
}
