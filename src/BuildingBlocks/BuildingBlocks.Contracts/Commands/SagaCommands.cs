using BuildingBlocks.Contracts.Events;

namespace BuildingBlocks.Contracts.Commands;

public sealed record ReserveStockCommand(
    Guid SagaId,
    Guid OrderId,
    IReadOnlyList<OrderItemDto> Items,
    Guid CorrelationId) : SagaCommand(SagaId, CorrelationId);

public sealed record ReleaseStockCommand(
    Guid SagaId,
    Guid OrderId,
    IReadOnlyList<OrderItemDto> Items,
    Guid CorrelationId) : SagaCommand(SagaId, CorrelationId);

public sealed record ConfirmStockDeductionCommand(
    Guid SagaId,
    Guid OrderId,
    Guid CorrelationId) : SagaCommand(SagaId, CorrelationId);

public sealed record ProcessPaymentCommand(
    Guid SagaId,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    Guid CorrelationId) : SagaCommand(SagaId, CorrelationId);

public sealed record RefundPaymentCommand(
    Guid SagaId,
    Guid OrderId,
    decimal Amount,
    Guid CorrelationId) : SagaCommand(SagaId, CorrelationId);

public sealed record ConfirmOrderCommand(
    Guid SagaId,
    Guid OrderId,
    Guid CorrelationId) : SagaCommand(SagaId, CorrelationId);

public sealed record CancelOrderCommand(
    Guid SagaId,
    Guid OrderId,
    string Reason,
    Guid CorrelationId) : SagaCommand(SagaId, CorrelationId);

public sealed record CreateShipmentCommand(
    Guid SagaId,
    Guid OrderId,
    Guid CustomerId,
    string ShippingAddress,
    IReadOnlyList<OrderItemDto> Items,
    Guid CorrelationId) : SagaCommand(SagaId, CorrelationId);

public sealed record SendOrderConfirmationEmailCommand(
    Guid SagaId,
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    decimal TotalAmount,
    Guid CorrelationId) : SagaCommand(SagaId, CorrelationId);
