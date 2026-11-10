namespace BuildingBlocks.Observability.Correlation;

public interface ICorrelationIdProvider
{
    Guid CorrelationId { get; }
    void SetCorrelationId(Guid correlationId);
}

public class CorrelationIdProvider : ICorrelationIdProvider
{
    private Guid _correlationId = Guid.NewGuid();

    public Guid CorrelationId => _correlationId;

    public void SetCorrelationId(Guid correlationId)
    {
        _correlationId = correlationId;
    }
}
