namespace BuildingBlocks.Contracts.Events;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
    Guid CorrelationId { get; }
}

public abstract record IntegrationEvent(Guid Id, DateTime OccurredOnUtc, Guid CorrelationId) : IIntegrationEvent
{
    protected IntegrationEvent(Guid correlationId) 
        : this(Guid.NewGuid(), DateTime.UtcNow, correlationId)
    {
    }
}
