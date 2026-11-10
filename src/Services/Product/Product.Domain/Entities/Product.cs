using BuildingBlocks.Common.Primitives;

namespace Product.Domain.Entities;

public class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private Product() { }

    public static Product Create(
        string name,
        string description,
        string sku,
        decimal price,
        Guid categoryId,
        string imageUrl)
    {
        if (price <= 0)
        {
            throw new ArgumentException("Price must be greater than zero.", nameof(price));
        }

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description.Trim(),
            SKU = sku.Trim().ToUpperInvariant(),
            Price = price,
            CategoryId = categoryId,
            ImageUrl = imageUrl.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdateDetails(string name, string description, decimal price, Guid categoryId, string imageUrl)
    {
        if (price <= 0)
        {
            throw new ArgumentException("Price must be greater than zero.", nameof(price));
        }

        Name = name.Trim();
        Description = description.Trim();
        Price = price;
        CategoryId = categoryId;
        ImageUrl = imageUrl.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
