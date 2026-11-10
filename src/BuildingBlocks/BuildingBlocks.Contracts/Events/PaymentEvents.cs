namespace BuildingBlocks.Contracts.Events;

public sealed record PaymentCompletedIntegrationEvent(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    string TransactionReference,
    Guid CorrelationId) : IntegrationEvent(CorrelationId);

public sealed record PaymentFailedIntegrationEvent(
    Guid OrderId,
    string Reason,
    Guid CorrelationId) : IntegrationEvent(CorrelationId);

public sealed record PaymentRefundedIntegrationEvent(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    Guid CorrelationId) : IntegrationEvent(CorrelationId);
