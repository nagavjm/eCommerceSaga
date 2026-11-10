using BuildingBlocks.Common.Primitives;

namespace Order.Domain.Events;

public sealed record OrderPlacedDomainEvent(Guid OrderId, Guid CustomerId, decimal TotalAmount, Guid CorrelationId) : IDomainEvent;
public sealed record OrderConfirmedDomainEvent(Guid OrderId, Guid CorrelationId) : IDomainEvent;
public sealed record OrderCancelledDomainEvent(Guid OrderId, string Reason, Guid CorrelationId) : IDomainEvent;
public sealed record OrderShippedDomainEvent(Guid OrderId, Guid CorrelationId) : IDomainEvent;
