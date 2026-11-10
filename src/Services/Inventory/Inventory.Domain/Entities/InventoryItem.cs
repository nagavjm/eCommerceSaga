using BuildingBlocks.Common.Primitives;

namespace Inventory.Domain.Entities;

public class InventoryItem : AggregateRoot<Guid>
{
    public Guid ProductId { get; private set; }
    public int AvailableStock { get; private set; }
    public int ReservedStock { get; private set; }
    public int TotalStock => AvailableStock + ReservedStock;
    public DateTime LastUpdatedUtc { get; private set; }

    private InventoryItem() { }

    public static InventoryItem Create(Guid productId, int initialStock)
    {
        if (initialStock < 0)
        {
            throw new ArgumentException("Initial stock cannot be negative.", nameof(initialStock));
        }

        return new InventoryItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            AvailableStock = initialStock,
            ReservedStock = 0,
            LastUpdatedUtc = DateTime.UtcNow
        };
    }

    public bool Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Reserved quantity must be greater than zero.", nameof(quantity));
        }

        if (AvailableStock < quantity)
        {
            return false;
        }

        AvailableStock -= quantity;
        ReservedStock += quantity;
        LastUpdatedUtc = DateTime.UtcNow;
        return true;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Released quantity must be greater than zero.", nameof(quantity));
        }

        var amountToRelease = Math.Min(quantity, ReservedStock);
        ReservedStock -= amountToRelease;
        AvailableStock += amountToRelease;
        LastUpdatedUtc = DateTime.UtcNow;
    }

    public void CommitDeduction(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Deducted quantity must be greater than zero.", nameof(quantity));
        }

        var amountToDeduct = Math.Min(quantity, ReservedStock);
        ReservedStock -= amountToDeduct;
        LastUpdatedUtc = DateTime.UtcNow;
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Restock quantity must be greater than zero.", nameof(quantity));
        }

        AvailableStock += quantity;
        LastUpdatedUtc = DateTime.UtcNow;
    }
}
