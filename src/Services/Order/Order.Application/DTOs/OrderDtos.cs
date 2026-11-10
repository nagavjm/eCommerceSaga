namespace Order.Application.DTOs;

public sealed record AddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);

public sealed record CreateOrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);

public sealed record CreateOrderRequest(
    AddressDto ShippingAddress,
    IReadOnlyList<CreateOrderItemDto> Items);

public sealed record OrderItemResponseDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

public sealed record OrderResponseDto(
    Guid Id,
    Guid CustomerId,
    string CustomerEmail,
    string Status,
    AddressDto ShippingAddress,
    decimal TotalAmount,
    Guid CorrelationId,
    string? CancellationReason,
    IReadOnlyList<OrderItemResponseDto> Items,
    DateTime CreatedAtUtc);

public sealed record OrderCreatedResponseDto(
    Guid OrderId,
    Guid CorrelationId,
    string Status,
    decimal TotalAmount);
