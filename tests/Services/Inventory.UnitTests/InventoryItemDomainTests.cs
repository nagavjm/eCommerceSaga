using FluentAssertions;
using Inventory.Domain.Entities;

namespace Inventory.UnitTests;

public class InventoryItemDomainTests
{
    [Fact]
    public void Create_WithValidData_ShouldInitializeInventoryItem()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var item = InventoryItem.Create(productId, 100);

        // Assert
        item.Id.Should().NotBeEmpty();
        item.ProductId.Should().Be(productId);
        item.AvailableStock.Should().Be(100);
        item.ReservedStock.Should().Be(0);
        item.TotalStock.Should().Be(100);
        item.LastUpdatedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithNegativeInitialStock_ShouldThrowArgumentException()
    {
        // Act
        var act = () => InventoryItem.Create(Guid.NewGuid(), -1);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Initial stock cannot be negative*");
    }

    [Fact]
    public void Create_WithZeroInitialStock_ShouldSucceed()
    {
        // Act
        var item = InventoryItem.Create(Guid.NewGuid(), 0);

        // Assert
        item.AvailableStock.Should().Be(0);
        item.ReservedStock.Should().Be(0);
        item.TotalStock.Should().Be(0);
    }

    [Fact]
    public void Reserve_WithSufficientStock_ShouldReturnTrueAndUpdateStock()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), 10);

        // Act
        var result = item.Reserve(3);

        // Assert
        result.Should().BeTrue();
        item.AvailableStock.Should().Be(7);
        item.ReservedStock.Should().Be(3);
        item.TotalStock.Should().Be(10); // unchanged
    }

    [Fact]
    public void Reserve_WithExactAvailableStock_ShouldReturnTrue()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), 5);

        // Act
        var result = item.Reserve(5);

        // Assert
        result.Should().BeTrue();
        item.AvailableStock.Should().Be(0);
        item.ReservedStock.Should().Be(5);
    }

    [Fact]
    public void Reserve_WithInsufficientStock_ShouldReturnFalseAndNotChangeStock()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), 2);

        // Act
        var result = item.Reserve(5);

        // Assert
        result.Should().BeFalse();
        item.AvailableStock.Should().Be(2); // unchanged
        item.ReservedStock.Should().Be(0); // unchanged
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Reserve_WithInvalidQuantity_ShouldThrowArgumentException(int quantity)
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), 10);

        // Act
        var act = () => item.Reserve(quantity);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Reserved quantity must be greater than zero*");
    }

    [Fact]
    public void Release_ShouldMoveStockFromReservedToAvailable()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), 10);
        item.Reserve(4);

        // Act
        item.Release(2);

        // Assert
        item.AvailableStock.Should().Be(8); // 6 + 2 released
        item.ReservedStock.Should().Be(2);  // 4 - 2 released
        item.TotalStock.Should().Be(10);    // unchanged
    }

    [Fact]
    public void Release_MoreThanReserved_ShouldReleaseOnlyReservedAmount()
    {
        // Arrange — only 3 reserved
        var item = InventoryItem.Create(Guid.NewGuid(), 10);
        item.Reserve(3);

        // Act — try to release 10 (more than reserved)
        item.Release(10);

        // Assert — releases only 3
        item.ReservedStock.Should().Be(0);
        item.AvailableStock.Should().Be(10);
    }

    [Fact]
    public void CommitDeduction_ShouldReduceReservedStockPermanently()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), 10);
        item.Reserve(4);

        // Act
        item.CommitDeduction(4);

        // Assert
        item.ReservedStock.Should().Be(0);
        item.AvailableStock.Should().Be(6); // 10 - 4 reserved, available unchanged
        item.TotalStock.Should().Be(6);     // TotalStock decreases after commit
    }

    [Fact]
    public void CommitDeduction_MoreThanReserved_ShouldDeductOnlyReservedAmount()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), 10);
        item.Reserve(2);

        // Act — try to commit more than reserved
        item.CommitDeduction(5);

        // Assert — only 2 (reserved amount) is deducted
        item.ReservedStock.Should().Be(0);
        item.AvailableStock.Should().Be(8);
        item.TotalStock.Should().Be(8);
    }

    [Fact]
    public void Restock_ShouldIncreaseAvailableStock()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), 5);

        // Act
        item.Restock(10);

        // Assert
        item.AvailableStock.Should().Be(15);
        item.TotalStock.Should().Be(15);
        item.LastUpdatedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Restock_WithInvalidQuantity_ShouldThrowArgumentException(int quantity)
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), 5);

        // Act
        var act = () => item.Restock(quantity);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Restock quantity must be greater than zero*");
    }

    [Fact]
    public void FullOrderWorkflow_ReserveCommitDeduct_ShouldDecreaseTotalStock()
    {
        // Arrange — represents the full Saga flow for one order item
        var item = InventoryItem.Create(Guid.NewGuid(), 100);

        // Act — Step 2: Reserve for order
        var reserved = item.Reserve(10);

        // Step 5: Commit deduction after payment succeeds
        item.CommitDeduction(10);

        // Assert
        reserved.Should().BeTrue();
        item.AvailableStock.Should().Be(90);
        item.ReservedStock.Should().Be(0);
        item.TotalStock.Should().Be(90);
    }

    [Fact]
    public void CompensationWorkflow_ReserveThenRelease_ShouldRestoreAvailableStock()
    {
        // Arrange — represents the compensation flow when payment fails
        var item = InventoryItem.Create(Guid.NewGuid(), 100);

        // Act — Step 2: Reserve for order
        item.Reserve(10);

        // Step compensation: Release after payment failure
        item.Release(10);

        // Assert — stock fully restored
        item.AvailableStock.Should().Be(100);
        item.ReservedStock.Should().Be(0);
        item.TotalStock.Should().Be(100);
    }
}
