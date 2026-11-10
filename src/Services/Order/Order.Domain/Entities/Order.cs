using BuildingBlocks.Common.Primitives;
using Order.Domain.Enums;
using Order.Domain.Events;
using Order.Domain.ValueObjects;

namespace Order.Domain.Entities;

public class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _items = [];

    public Guid CustomerId { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public Address ShippingAddress { get; private set; } = null!;
    public decimal TotalAmount { get; private set; }
    public Guid CorrelationId { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(
        Guid customerId,
        string customerEmail,
        Address shippingAddress,
        Guid correlationId)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new ArgumentException("CustomerEmail cannot be empty.", nameof(customerEmail));
        ArgumentNullException.ThrowIfNull(shippingAddress);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CustomerEmail = customerEmail.Trim().ToLowerInvariant(),
            Status = OrderStatus.Pending,
            ShippingAddress = shippingAddress,
            TotalAmount = 0m,
            CorrelationId = correlationId == Guid.Empty ? Guid.NewGuid() : correlationId,
            CreatedAtUtc = DateTime.UtcNow
        };

        return order;
    }

    public OrderItem AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot add items to an order with status '{Status}'.");
        }

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            throw new InvalidOperationException($"Product '{productId}' is already present in this order.");
        }

        var item = OrderItem.Create(Id, productId, productName, unitPrice, quantity);
        _items.Add(item);
        TotalAmount = _items.Sum(i => i.TotalPrice);
        UpdatedAtUtc = DateTime.UtcNow;

        return item;
    }

    public void FinalizePlacement()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Order must have at least one item.");
        }

        AddDomainEvent(new OrderPlacedDomainEvent(Id, CustomerId, TotalAmount, CorrelationId));
    }

    public void MarkAsStockReserved()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot transition to StockReserved from '{Status}'.");
        }

        Status = OrderStatus.StockReserved;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.StockReserved && Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot transition to Paid from '{Status}'.");
        }

        Status = OrderStatus.Paid;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot confirm a cancelled order.");
        }

        Status = OrderStatus.Confirmed;
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new OrderConfirmedDomainEvent(Id, CorrelationId));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Confirmed || Status == OrderStatus.Shipped)
        {
            throw new InvalidOperationException($"Cannot cancel an order that is already '{Status}'.");
        }

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new OrderCancelledDomainEvent(Id, reason, CorrelationId));
    }

    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Confirmed)
        {
            throw new InvalidOperationException($"Cannot ship an order that is not Confirmed (current status: {Status}).");
        }

        Status = OrderStatus.Shipped;
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new OrderShippedDomainEvent(Id, CorrelationId));
    }
}
