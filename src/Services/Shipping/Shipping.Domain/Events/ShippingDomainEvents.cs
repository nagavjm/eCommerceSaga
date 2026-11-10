using BuildingBlocks.Common.Primitives;

namespace Shipping.Domain.Events;

public sealed record ShipmentCreatedDomainEvent(
    Guid ShipmentId,
    Guid OrderId,
    string TrackingNumber,
    Guid CorrelationId) : IDomainEvent;

public sealed record ShipmentDispatchedDomainEvent(
    Guid ShipmentId,
    Guid OrderId,
    string TrackingNumber,
    Guid CorrelationId) : IDomainEvent;

public sealed record ShipmentDeliveredDomainEvent(
    Guid ShipmentId,
    Guid OrderId,
    Guid CorrelationId) : IDomainEvent;

public sealed record ShipmentFailedDomainEvent(
    Guid ShipmentId,
    Guid OrderId,
    string Reason,
    Guid CorrelationId) : IDomainEvent;
