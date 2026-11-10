using BuildingBlocks.Common.Primitives;

namespace Payment.Domain.Events;

public sealed record PaymentCompletedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string TransactionReference,
    Guid CorrelationId) : IDomainEvent;

public sealed record PaymentFailedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    string Reason,
    Guid CorrelationId) : IDomainEvent;

public sealed record PaymentRefundedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    Guid CorrelationId) : IDomainEvent;
