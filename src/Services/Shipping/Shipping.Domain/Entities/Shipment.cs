using BuildingBlocks.Common.Primitives;
using Shipping.Domain.Enums;
using Shipping.Domain.Events;
using Shipping.Domain.ValueObjects;

namespace Shipping.Domain.Entities;

public class Shipment : AggregateRoot<Guid>
{
    private readonly List<ShipmentItem> _items = [];

    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string TrackingNumber { get; private set; } = string.Empty;
    public ShipmentStatus Status { get; private set; }
    public Address DeliveryAddress { get; private set; } = null!;
    public string Carrier { get; private set; } = "DHL Express";
    public DateTime? EstimatedDeliveryUtc { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }
    public string? FailureReason { get; private set; }
    public Guid CorrelationId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<ShipmentItem> Items => _items.AsReadOnly();

    private Shipment() { }

    public static Shipment Create(
        Guid orderId,
        Guid customerId,
        Address deliveryAddress,
        string carrier,
        Guid correlationId)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));
        ArgumentNullException.ThrowIfNull(deliveryAddress);

        return new Shipment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            Status = ShipmentStatus.Created,
            DeliveryAddress = deliveryAddress,
            Carrier = string.IsNullOrWhiteSpace(carrier) ? "DHL Express" : carrier.Trim(),
            CorrelationId = correlationId == Guid.Empty ? Guid.NewGuid() : correlationId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public ShipmentItem AddItem(Guid productId, string productName, int quantity)
    {
        if (Status != ShipmentStatus.Created)
        {
            throw new InvalidOperationException($"Cannot add items to a shipment with status '{Status}'.");
        }

        var item = ShipmentItem.Create(Id, productId, productName, quantity);
        _items.Add(item);
        UpdatedAtUtc = DateTime.UtcNow;
        return item;
    }

    public void FinalizeCreation()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Shipment must contain at least one item.");
        }

        TrackingNumber = $"TRK-{Guid.NewGuid():N}"[..16].ToUpperInvariant();
        EstimatedDeliveryUtc = DateTime.UtcNow.AddDays(3);
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new ShipmentCreatedDomainEvent(Id, OrderId, TrackingNumber, CorrelationId));
    }

    public void MarkAsDispatched()
    {
        if (Status != ShipmentStatus.Created)
        {
            throw new InvalidOperationException($"Cannot dispatch shipment from status '{Status}'.");
        }

        Status = ShipmentStatus.Dispatched;
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new ShipmentDispatchedDomainEvent(Id, OrderId, TrackingNumber, CorrelationId));
    }

    public void MarkAsInTransit()
    {
        if (Status != ShipmentStatus.Dispatched)
        {
            throw new InvalidOperationException($"Cannot mark shipment as in-transit from status '{Status}'.");
        }

        Status = ShipmentStatus.InTransit;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAsDelivered()
    {
        if (Status != ShipmentStatus.InTransit && Status != ShipmentStatus.Dispatched)
        {
            throw new InvalidOperationException($"Cannot mark shipment as delivered from status '{Status}'.");
        }

        Status = ShipmentStatus.Delivered;
        DeliveredAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new ShipmentDeliveredDomainEvent(Id, OrderId, CorrelationId));
    }

    public void MarkAsFailed(string reason)
    {
        if (Status == ShipmentStatus.Delivered)
        {
            throw new InvalidOperationException("Cannot mark a delivered shipment as failed.");
        }

        Status = ShipmentStatus.Failed;
        FailureReason = reason;
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new ShipmentFailedDomainEvent(Id, OrderId, reason, CorrelationId));
    }
}
