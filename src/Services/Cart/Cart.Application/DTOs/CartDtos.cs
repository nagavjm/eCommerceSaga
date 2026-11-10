namespace Cart.Application.DTOs;

public sealed record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice,
    string ImageUrl);

public sealed record CartDto(
    Guid Id,
    Guid CustomerId,
    decimal TotalPrice,
    IReadOnlyList<CartItemDto> Items);
