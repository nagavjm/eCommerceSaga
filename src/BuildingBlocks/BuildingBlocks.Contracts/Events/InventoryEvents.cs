namespace BuildingBlocks.Contracts.Events;

public sealed record StockReservedIntegrationEvent(
    Guid OrderId,
    Guid CorrelationId) : IntegrationEvent(CorrelationId);

public sealed record StockReservationFailedIntegrationEvent(
    Guid OrderId,
    string Reason,
    Guid CorrelationId) : IntegrationEvent(CorrelationId);

public sealed record StockReleasedIntegrationEvent(
    Guid OrderId,
    Guid CorrelationId) : IntegrationEvent(CorrelationId);

public sealed record StockDeductedIntegrationEvent(
    Guid OrderId,
    Guid CorrelationId) : IntegrationEvent(CorrelationId);
