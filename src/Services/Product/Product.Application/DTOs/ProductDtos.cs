namespace Product.Application.DTOs;

public sealed record CategoryDto(Guid Id, string Name, string Description);

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    string SKU,
    decimal Price,
    Guid CategoryId,
    string? CategoryName,
    string ImageUrl,
    bool IsActive);
