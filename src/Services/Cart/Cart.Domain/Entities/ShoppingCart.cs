using BuildingBlocks.Common.Primitives;

namespace Cart.Domain.Entities;

public class ShoppingCart : AggregateRoot<Guid>
{
    private readonly List<CartItem> _items = [];

    public Guid CustomerId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    public decimal TotalPrice => _items.Sum(item => item.TotalPrice);

    private ShoppingCart() { }

    public static ShoppingCart Create(Guid customerId)
    {
        return new ShoppingCart
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public CartItem AddItem(Guid productId, string productName, decimal unitPrice, int quantity, string imageUrl)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            UpdatedAtUtc = DateTime.UtcNow;
            return existingItem;
        }

        var newItem = CartItem.Create(Id, productId, productName, unitPrice, quantity, imageUrl);
        _items.Add(newItem);
        UpdatedAtUtc = DateTime.UtcNow;
        return newItem;
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem is null)
        {
            return;
        }

        if (quantity <= 0)
        {
            _items.Remove(existingItem);
        }
        else
        {
            existingItem.UpdateQuantity(quantity);
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            _items.Remove(existingItem);
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
