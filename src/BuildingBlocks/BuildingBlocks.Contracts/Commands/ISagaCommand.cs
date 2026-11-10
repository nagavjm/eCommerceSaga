namespace BuildingBlocks.Contracts.Commands;

public interface ISagaCommand
{
    Guid CommandId { get; }
    Guid SagaId { get; }
    Guid CorrelationId { get; }
    DateTime TimestampUtc { get; }
}

public abstract record SagaCommand(Guid CommandId, Guid SagaId, Guid CorrelationId, DateTime TimestampUtc) : ISagaCommand
{
    protected SagaCommand(Guid sagaId, Guid correlationId)
        : this(Guid.NewGuid(), sagaId, correlationId, DateTime.UtcNow)
    {
    }
}
