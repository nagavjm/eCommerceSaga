namespace Shipping.Application.DTOs;

public sealed record AddressDto(
    string Street,
    string City,
    string State,
    string Country,
    string ZipCode);

public sealed record ShipmentItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity);

public sealed record CreateShipmentRequest(
    Guid OrderId,
    Guid CustomerId,
    AddressDto DeliveryAddress,
    string? Carrier,
    IReadOnlyList<ShipmentItemDto> Items);

public sealed record ShipmentDto(
    Guid Id,
    Guid OrderId,
    Guid CustomerId,
    string TrackingNumber,
    string Status,
    AddressDto DeliveryAddress,
    string Carrier,
    DateTime? EstimatedDeliveryUtc,
    DateTime? DeliveredAtUtc,
    string? FailureReason,
    IReadOnlyList<ShipmentItemDto> Items,
    DateTime CreatedAtUtc);
