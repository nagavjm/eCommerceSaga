using BuildingBlocks.Common.Primitives;

namespace Cart.Domain.Entities;

public class CartItem : Entity<Guid>
{
    public Guid ShoppingCartId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;

    public decimal TotalPrice => UnitPrice * Quantity;

    private CartItem() { }

    public static CartItem Create(
        Guid shoppingCartId,
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity,
        string imageUrl)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        return new CartItem
        {
            Id = Guid.NewGuid(),
            ShoppingCartId = shoppingCartId,
            ProductId = productId,
            ProductName = productName.Trim(),
            UnitPrice = unitPrice,
            Quantity = quantity,
            ImageUrl = imageUrl.Trim()
        };
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        Quantity = quantity;
    }
}
