using FluentAssertions;
using Inventory.Application.Commands;
using Inventory.Application.DTOs;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.UnitTests;

public static class InventoryDbContextFactory
{
    public static InventoryDbContext Create()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InventoryDbContext(options);
    }
}

public class ReserveStockCommandHandlerTests
{
    [Fact]
    public async Task Handle_SufficientStockForAllItems_ShouldReturnSuccessResult()
    {
        // Arrange
        await using var ctx = InventoryDbContextFactory.Create();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        ctx.InventoryItems.Add(InventoryItem.Create(productId1, 100));
        ctx.InventoryItems.Add(InventoryItem.Create(productId2, 50));
        await ctx.SaveChangesAsync();

        var handler = new ReserveStockCommandHandler(ctx);
        var command = new ReserveStockCommand(
            Guid.NewGuid(),
            new List<StockReservationItemDto>
            {
                new(productId1, 10),
                new(productId2, 5)
            });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsSuccessful.Should().BeTrue();

        var item1 = await ctx.InventoryItems.FirstAsync(i => i.ProductId == productId1);
        item1.AvailableStock.Should().Be(90);
        item1.ReservedStock.Should().Be(10);

        var item2 = await ctx.InventoryItems.FirstAsync(i => i.ProductId == productId2);
        item2.AvailableStock.Should().Be(45);
        item2.ReservedStock.Should().Be(5);
    }

    [Fact]
    public async Task Handle_InsufficientStockForOneItem_ShouldReturnFailureAndNotReserveAny()
    {
        // Arrange
        await using var ctx = InventoryDbContextFactory.Create();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        ctx.InventoryItems.Add(InventoryItem.Create(productId1, 100));
        ctx.InventoryItems.Add(InventoryItem.Create(productId2, 2)); // only 2 available
        await ctx.SaveChangesAsync();

        var handler = new ReserveStockCommandHandler(ctx);
        var command = new ReserveStockCommand(
            Guid.NewGuid(),
            new List<StockReservationItemDto>
            {
                new(productId1, 10),
                new(productId2, 10) // requesting 10 but only 2 available
            });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // handler returns Result.Success wrapping a failed dto
        result.Value.IsSuccessful.Should().BeFalse();
        result.Value.FailureReason.Should().Contain("Insufficient stock");

        // Verify no stock was reserved (pre-validation prevents partial reservation)
        var item1 = await ctx.InventoryItems.FirstAsync(i => i.ProductId == productId1);
        item1.ReservedStock.Should().Be(0); // not reserved since pre-check failed

        var item2 = await ctx.InventoryItems.FirstAsync(i => i.ProductId == productId2);
        item2.ReservedStock.Should().Be(0); // not reserved
    }

    [Fact]
    public async Task Handle_ProductNotInInventory_ShouldReturnFailure()
    {
        // Arrange
        await using var ctx = InventoryDbContextFactory.Create();
        var handler = new ReserveStockCommandHandler(ctx);
        var command = new ReserveStockCommand(
            Guid.NewGuid(),
            new List<StockReservationItemDto>
            {
                new(Guid.NewGuid(), 1) // product not in inventory
            });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsSuccessful.Should().BeFalse();
        result.Value.FailureReason.Should().Contain("does not exist in inventory");
    }
}

public class ReleaseStockCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReleaseReservedStock()
    {
        // Arrange
        await using var ctx = InventoryDbContextFactory.Create();
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(productId, 100);
        item.Reserve(20);
        ctx.InventoryItems.Add(item);
        await ctx.SaveChangesAsync();

        var handler = new ReleaseStockCommandHandler(ctx);
        var command = new ReleaseStockCommand(
            Guid.NewGuid(),
            new List<StockReservationItemDto> { new(productId, 20) });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updated = await ctx.InventoryItems.FirstAsync(i => i.ProductId == productId);
        updated.AvailableStock.Should().Be(100); // back to original
        updated.ReservedStock.Should().Be(0);
    }
}

public class RestockCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProduct_ShouldIncreaseAvailableStock()
    {
        // Arrange
        await using var ctx = InventoryDbContextFactory.Create();
        var productId = Guid.NewGuid();
        ctx.InventoryItems.Add(InventoryItem.Create(productId, 10));
        await ctx.SaveChangesAsync();

        var handler = new RestockCommandHandler(ctx);
        var command = new RestockCommand(productId, 50);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AvailableStock.Should().Be(60);
        result.Value.TotalStock.Should().Be(60);
    }

    [Fact]
    public async Task Handle_NewProduct_ShouldCreateInventoryItemWithGivenStock()
    {
        // Arrange
        await using var ctx = InventoryDbContextFactory.Create();
        var handler = new RestockCommandHandler(ctx);
        var newProductId = Guid.NewGuid();
        var command = new RestockCommand(newProductId, 25);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ProductId.Should().Be(newProductId);
        result.Value.AvailableStock.Should().Be(25);

        var created = await ctx.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == newProductId);
        created.Should().NotBeNull();
    }
}
