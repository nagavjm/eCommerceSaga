using FluentAssertions;
using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Domain.Events;
using Order.Domain.ValueObjects;

namespace Order.UnitTests;

public class OrderDomainTests
{
    private static Address CreateValidAddress() =>
        new("123 Market St", "Seattle", "WA", "USA", "98101");

    [Fact]
    public void Create_WithValidData_ShouldInitializePendingOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var address = CreateValidAddress();

        // Act
        var order = Order.Domain.Entities.Order.Create(
            customerId,
            "buyer@example.com",
            address,
            correlationId);

        // Assert
        order.Id.Should().NotBeEmpty();
        order.CustomerId.Should().Be(customerId);
        order.CustomerEmail.Should().Be("buyer@example.com");
        order.Status.Should().Be(OrderStatus.Pending);
        order.ShippingAddress.Should().Be(address);
        order.TotalAmount.Should().Be(0m);
        order.CorrelationId.Should().Be(correlationId);
        order.Items.Should().BeEmpty();
        order.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AddItem_WithValidItem_ShouldCalculateTotalAmount()
    {
        // Arrange
        var order = Order.Domain.Entities.Order.Create(
            Guid.NewGuid(),
            "buyer@example.com",
            CreateValidAddress(),
            Guid.NewGuid());

        var prodId1 = Guid.NewGuid();
        var prodId2 = Guid.NewGuid();

        // Act
        order.AddItem(prodId1, "Item A", 25.00m, 2); // 50.00
        order.AddItem(prodId2, "Item B", 10.50m, 3); // 31.50

        // Assert
        order.Items.Should().HaveCount(2);
        order.TotalAmount.Should().Be(81.50m);
    }

    [Fact]
    public void AddItem_DuplicateProduct_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Domain.Entities.Order.Create(
            Guid.NewGuid(),
            "buyer@example.com",
            CreateValidAddress(),
            Guid.NewGuid());

        var prodId = Guid.NewGuid();
        order.AddItem(prodId, "Item A", 25.00m, 1);

        // Act
        var act = () => order.AddItem(prodId, "Item A Duplicate", 25.00m, 1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already present in this order*");
    }

    [Fact]
    public void FinalizePlacement_WithNoItems_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Domain.Entities.Order.Create(
            Guid.NewGuid(),
            "buyer@example.com",
            CreateValidAddress(),
            Guid.NewGuid());

        // Act
        var act = () => order.FinalizePlacement();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must have at least one item*");
    }

    [Fact]
    public void FinalizePlacement_WithItems_ShouldRaiseOrderPlacedDomainEvent()
    {
        // Arrange
        var order = Order.Domain.Entities.Order.Create(
            Guid.NewGuid(),
            "buyer@example.com",
            CreateValidAddress(),
            Guid.NewGuid());

        order.AddItem(Guid.NewGuid(), "Laptop", 1200m, 1);

        // Act
        order.FinalizePlacement();

        // Assert
        order.DomainEvents.Should().ContainSingle(e => e is OrderPlacedDomainEvent);
    }

    [Fact]
    public void MarkAsStockReserved_FromPending_ShouldSucceed()
    {
        // Arrange
        var order = Order.Domain.Entities.Order.Create(
            Guid.NewGuid(),
            "buyer@example.com",
            CreateValidAddress(),
            Guid.NewGuid());

        // Act
        order.MarkAsStockReserved();

        // Assert
        order.Status.Should().Be(OrderStatus.StockReserved);
    }

    [Fact]
    public void MarkAsPaid_FromStockReserved_ShouldSucceed()
    {
        // Arrange
        var order = Order.Domain.Entities.Order.Create(
            Guid.NewGuid(),
            "buyer@example.com",
            CreateValidAddress(),
            Guid.NewGuid());
        order.MarkAsStockReserved();

        // Act
        order.MarkAsPaid();

        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void Confirm_FromValidStatus_ShouldConfirmAndRaiseDomainEvent()
    {
        // Arrange
        var order = Order.Domain.Entities.Order.Create(
            Guid.NewGuid(),
            "buyer@example.com",
            CreateValidAddress(),
            Guid.NewGuid());
        order.MarkAsStockReserved();
        order.MarkAsPaid();

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.DomainEvents.Should().ContainSingle(e => e is OrderConfirmedDomainEvent);
    }

    [Fact]
    public void Confirm_WhenCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Domain.Entities.Order.Create(
            Guid.NewGuid(),
            "buyer@example.com",
            CreateValidAddress(),
            Guid.NewGuid());
        order.Cancel("Out of stock");

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot confirm a cancelled order*");
    }

    [Fact]
    public void Cancel_WhenPending_ShouldCancelAndRecordReason()
    {
        // Arrange
        var order = Order.Domain.Entities.Order.Create(
            Guid.NewGuid(),
            "buyer@example.com",
            CreateValidAddress(),
            Guid.NewGuid());

        // Act
        order.Cancel("Payment failed");

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be("Payment failed");
        order.DomainEvents.Should().ContainSingle(e => e is OrderCancelledDomainEvent);
    }

    [Fact]
    public void Cancel_WhenAlreadyConfirmed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Domain.Entities.Order.Create(
            Guid.NewGuid(),
            "buyer@example.com",
            CreateValidAddress(),
            Guid.NewGuid());
        order.Confirm();

        // Act
        var act = () => order.Cancel("Buyer requested");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot cancel an order that is already*");
    }

    [Fact]
    public void Address_ValueObject_SameValues_ShouldBeEqual()
    {
        // Arrange
        var addr1 = new Address("123 Main St", "Springfield", "IL", "USA", "62701");
        var addr2 = new Address("123 Main St", "Springfield", "IL", "USA", "62701");

        // Assert
        addr1.Should().Be(addr2);
        (addr1 == addr2).Should().BeTrue();
    }

    [Fact]
    public void Address_ValueObject_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var addr1 = new Address("123 Main St", "Springfield", "IL", "USA", "62701");
        var addr2 = new Address("456 Elm St", "Springfield", "IL", "USA", "62701");

        // Assert
        addr1.Should().NotBe(addr2);
        (addr1 == addr2).Should().BeFalse();
    }
}
