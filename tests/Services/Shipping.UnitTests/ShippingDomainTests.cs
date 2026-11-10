using FluentAssertions;
using Shipping.Domain.Entities;
using Shipping.Domain.Enums;
using Shipping.Domain.Events;
using Shipping.Domain.ValueObjects;

namespace Shipping.UnitTests;

public class ShippingDomainTests
{
    private static Address CreateValidAddress() =>
        new("789 Industrial Pkwy", "Dallas", "TX", "USA", "75001");

    [Fact]
    public void Create_WithValidData_ShouldInitializeCreatedShipment()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var address = CreateValidAddress();

        // Act
        var shipment = Shipment.Create(orderId, customerId, address, "FedEx Priority", correlationId);

        // Assert
        shipment.Id.Should().NotBeEmpty();
        shipment.OrderId.Should().Be(orderId);
        shipment.CustomerId.Should().Be(customerId);
        shipment.Status.Should().Be(ShipmentStatus.Created);
        shipment.Carrier.Should().Be("FedEx Priority");
        shipment.DeliveryAddress.Should().Be(address);
        shipment.CorrelationId.Should().Be(correlationId);
        shipment.Items.Should().BeEmpty();
        shipment.TrackingNumber.Should().BeEmpty();
    }

    [Fact]
    public void AddItem_WhenCreated_ShouldAddItems()
    {
        // Arrange
        var shipment = Shipment.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidAddress(), "DHL", Guid.NewGuid());
        var prodId = Guid.NewGuid();

        // Act
        shipment.AddItem(prodId, "Monitor", 2);

        // Assert
        shipment.Items.Should().HaveCount(1);
        shipment.Items.First().ProductId.Should().Be(prodId);
        shipment.Items.First().Quantity.Should().Be(2);
    }

    [Fact]
    public void FinalizeCreation_ShouldGenerateTrackingNumberAndSetEstimatedDelivery()
    {
        // Arrange
        var shipment = Shipment.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidAddress(), "DHL", Guid.NewGuid());
        shipment.AddItem(Guid.NewGuid(), "Desk", 1);

        // Act
        shipment.FinalizeCreation();

        // Assert
        shipment.TrackingNumber.Should().StartWith("TRK-");
        shipment.EstimatedDeliveryUtc.Should().NotBeNull();
        shipment.DomainEvents.Should().ContainSingle(e => e is ShipmentCreatedDomainEvent);
    }

    [Fact]
    public void FinalizeCreation_WithNoItems_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = Shipment.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidAddress(), "DHL", Guid.NewGuid());

        // Act
        var act = () => shipment.FinalizeCreation();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must contain at least one item*");
    }

    [Fact]
    public void MarkAsDispatched_FromCreated_ShouldTransitionStatusAndRaiseDomainEvent()
    {
        // Arrange
        var shipment = Shipment.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidAddress(), "DHL", Guid.NewGuid());
        shipment.AddItem(Guid.NewGuid(), "Desk", 1);
        shipment.FinalizeCreation();

        // Act
        shipment.MarkAsDispatched();

        // Assert
        shipment.Status.Should().Be(ShipmentStatus.Dispatched);
        shipment.DomainEvents.Should().Contain(e => e is ShipmentDispatchedDomainEvent);
    }

    [Fact]
    public void MarkAsInTransit_FromDispatched_ShouldTransitionStatus()
    {
        // Arrange
        var shipment = Shipment.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidAddress(), "DHL", Guid.NewGuid());
        shipment.AddItem(Guid.NewGuid(), "Desk", 1);
        shipment.FinalizeCreation();
        shipment.MarkAsDispatched();

        // Act
        shipment.MarkAsInTransit();

        // Assert
        shipment.Status.Should().Be(ShipmentStatus.InTransit);
    }

    [Fact]
    public void MarkAsDelivered_FromInTransit_ShouldSetDeliveredAtUtcAndRaiseDomainEvent()
    {
        // Arrange
        var shipment = Shipment.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidAddress(), "DHL", Guid.NewGuid());
        shipment.AddItem(Guid.NewGuid(), "Desk", 1);
        shipment.FinalizeCreation();
        shipment.MarkAsDispatched();
        shipment.MarkAsInTransit();

        // Act
        shipment.MarkAsDelivered();

        // Assert
        shipment.Status.Should().Be(ShipmentStatus.Delivered);
        shipment.DeliveredAtUtc.Should().NotBeNull();
        shipment.DomainEvents.Should().Contain(e => e is ShipmentDeliveredDomainEvent);
    }

    [Fact]
    public void MarkAsFailed_ShouldSetStatusFailedAndReason()
    {
        // Arrange
        var shipment = Shipment.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidAddress(), "DHL", Guid.NewGuid());
        shipment.AddItem(Guid.NewGuid(), "Desk", 1);
        shipment.FinalizeCreation();

        // Act
        shipment.MarkAsFailed("Address not found");

        // Assert
        shipment.Status.Should().Be(ShipmentStatus.Failed);
        shipment.FailureReason.Should().Be("Address not found");
        shipment.DomainEvents.Should().Contain(e => e is ShipmentFailedDomainEvent);
    }

    [Fact]
    public void MarkAsFailed_WhenDelivered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shipment = Shipment.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidAddress(), "DHL", Guid.NewGuid());
        shipment.AddItem(Guid.NewGuid(), "Desk", 1);
        shipment.FinalizeCreation();
        shipment.MarkAsDispatched();
        shipment.MarkAsDelivered();

        // Act
        var act = () => shipment.MarkAsFailed("Lost in transit");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot mark a delivered shipment as failed*");
    }

    [Fact]
    public void Address_ValueObject_Equality_ShouldWorkProperly()
    {
        // Arrange
        var addr1 = new Address("1 St", "City", "ST", "US", "10001");
        var addr2 = new Address("1 St", "City", "ST", "US", "10001");
        var addr3 = new Address("2 St", "City", "ST", "US", "10001");

        // Assert
        addr1.Should().Be(addr2);
        addr1.Should().NotBe(addr3);
    }
}
