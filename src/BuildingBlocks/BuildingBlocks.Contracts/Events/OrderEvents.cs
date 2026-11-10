namespace BuildingBlocks.Contracts.Events;

public sealed record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

public sealed record OrderPlacedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    string ShippingAddress,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items,
    Guid CorrelationId) : IntegrationEvent(CorrelationId);

public sealed record OrderConfirmedIntegrationEvent(
    Guid OrderId,
    Guid CorrelationId) : IntegrationEvent(CorrelationId);

public sealed record OrderCancelledIntegrationEvent(
    Guid OrderId,
    string Reason,
    Guid CorrelationId) : IntegrationEvent(CorrelationId);
