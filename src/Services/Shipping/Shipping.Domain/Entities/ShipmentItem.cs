using BuildingBlocks.Common.Primitives;

namespace Shipping.Domain.Entities;

public class ShipmentItem : Entity<Guid>
{
    public Guid ShipmentId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }

    private ShipmentItem() { }

    internal static ShipmentItem Create(Guid shipmentId, Guid productId, string productName, int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("ProductName cannot be empty.", nameof(productName));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        return new ShipmentItem
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            ProductId = productId,
            ProductName = productName.Trim(),
            Quantity = quantity
        };
    }
}
