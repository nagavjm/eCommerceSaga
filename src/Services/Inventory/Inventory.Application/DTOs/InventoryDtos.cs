namespace Inventory.Application.DTOs;

public sealed record InventoryDto(
    Guid Id,
    Guid ProductId,
    int AvailableStock,
    int ReservedStock,
    int TotalStock,
    DateTime LastUpdatedUtc);

public sealed record StockReservationItemDto(
    Guid ProductId,
    int Quantity);

public sealed record StockReservationResultDto(
    bool IsSuccessful,
    string? FailureReason = null);
