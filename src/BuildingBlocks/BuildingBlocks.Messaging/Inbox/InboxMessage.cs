namespace BuildingBlocks.Messaging.Inbox;

public class InboxMessage
{
    public Guid Id { get; set; }
    public string HandlerName { get; set; } = string.Empty;
    public DateTime ReceivedOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}

public interface IInboxStore
{
    Task<bool> HasBeenProcessedAsync(Guid messageId, string handlerName, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid messageId, string handlerName, CancellationToken cancellationToken = default);
}
