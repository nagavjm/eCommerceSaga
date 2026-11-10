namespace BuildingBlocks.Messaging.Outbox;

public interface IOutboxStore
{
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 20, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task RecordFailureAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
}
