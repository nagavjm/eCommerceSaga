namespace BuildingBlocks.Contracts.Events;

public sealed record ShipmentCreatedIntegrationEvent(
    Guid OrderId,
    Guid ShipmentId,
    string TrackingNumber,
    string Carrier,
    Guid CorrelationId) : IntegrationEvent(CorrelationId);

public sealed record ShipmentCreationFailedIntegrationEvent(
    Guid OrderId,
    string Reason,
    Guid CorrelationId) : IntegrationEvent(CorrelationId);
