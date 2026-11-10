namespace BuildingBlocks.Messaging.Abstractions;

public class AzureServiceBusOptions
{
    public const string SectionName = "AzureServiceBus";

    public string ConnectionString { get; set; } = string.Empty;
    public string TopicName { get; set; } = "ecommerce.events.topic";
    public string SubscriptionName { get; set; } = "sub-all-events";
}

public interface IMessagePublisher
{
    Task PublishAsync<T>(
        T message, 
        string? queueOrTopicName = null, 
        Guid? correlationId = null,
        CancellationToken cancellationToken = default) 
        where T : class;
}
