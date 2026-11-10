using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BuildingBlocks.Contracts.Commands;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Messaging.ServiceBus;

public class AzureServiceBusMessagePublisher : IMessagePublisher
{
    private readonly ServiceBusClient _client;
    private readonly AzureServiceBusOptions _options;
    private readonly ILogger<AzureServiceBusMessagePublisher> _logger;

    public AzureServiceBusMessagePublisher(
        ServiceBusClient client,
        IOptions<AzureServiceBusOptions> options,
        ILogger<AzureServiceBusMessagePublisher> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync<T>(
        T message,
        string? queueOrTopicName = null,
        Guid? correlationId = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var targetDestination = queueOrTopicName ?? _options.TopicName;
        var sender = _client.CreateSender(targetDestination);

        var payloadJson = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(payloadJson)
        {
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString()
        };

        var resolvedCorrelationId = correlationId;

        if (message is IIntegrationEvent integrationEvent)
        {
            serviceBusMessage.MessageId = integrationEvent.Id.ToString();
            resolvedCorrelationId = integrationEvent.CorrelationId;
            serviceBusMessage.ApplicationProperties["EventType"] = integrationEvent.GetType().Name;
        }
        else if (message is ISagaCommand sagaCommand)
        {
            serviceBusMessage.MessageId = sagaCommand.CommandId.ToString();
            resolvedCorrelationId = sagaCommand.CorrelationId;
            serviceBusMessage.ApplicationProperties["CommandType"] = sagaCommand.GetType().Name;
            serviceBusMessage.ApplicationProperties["SagaId"] = sagaCommand.SagaId.ToString();
        }

        if (resolvedCorrelationId.HasValue)
        {
            serviceBusMessage.CorrelationId = resolvedCorrelationId.Value.ToString();
            serviceBusMessage.ApplicationProperties["CorrelationId"] = resolvedCorrelationId.Value.ToString();
        }

        serviceBusMessage.ApplicationProperties["MessageType"] = typeof(T).AssemblyQualifiedName;

        _logger.LogInformation("Publishing message {MessageId} of type {MessageType} to destination {Destination}",
            serviceBusMessage.MessageId, typeof(T).Name, targetDestination);

        await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }
}
