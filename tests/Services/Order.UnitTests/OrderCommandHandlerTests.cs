using System.Text.Json;
using BuildingBlocks.Contracts.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Application.Queries;
using Order.Domain.Enums;
using Order.Domain.ValueObjects;
using Order.Infrastructure.Persistence;

namespace Order.UnitTests;

public static class OrderDbContextFactory
{
    public static OrderDbContext Create()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new OrderDbContext(options);
    }
}

public class OrderCommandHandlerTests
{
    [Fact]
    public async Task CreateOrder_ValidCommand_ShouldSaveOrderAndOutboxMessageAtomically()
    {
        // Arrange
        await using var ctx = OrderDbContextFactory.Create();
        var handler = new CreateOrderCommandHandler(ctx);

        var customerId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var command = new CreateOrderCommand(
            customerId,
            "john.doe@example.com",
            new AddressDto("100 Pine St", "Seattle", "WA", "USA", "98101"),
            new List<CreateOrderItemDto>
            {
                new(Guid.NewGuid(), "Laptop", 1000m, 1),
                new(Guid.NewGuid(), "Mouse", 50m, 2)
            },
            correlationId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().NotBeEmpty();
        result.Value.Status.Should().Be("Pending");
        result.Value.TotalAmount.Should().Be(1100m);
        result.Value.CorrelationId.Should().Be(correlationId);

        // Verify Order persisted in DB
        var persistedOrder = await ctx.Orders.Include(o => o.Items).FirstOrDefaultAsync();
        persistedOrder.Should().NotBeNull();
        persistedOrder!.CustomerId.Should().Be(customerId);
        persistedOrder.TotalAmount.Should().Be(1100m);
        persistedOrder.Items.Should().HaveCount(2);

        // Verify Transactional Outbox: Event was created and saved in same transaction
        var outboxMessage = await ctx.OutboxMessages.FirstOrDefaultAsync();
        outboxMessage.Should().NotBeNull();
        outboxMessage!.Type.Should().Contain(nameof(OrderPlacedIntegrationEvent));

        var deserializedEvent = JsonSerializer.Deserialize<OrderPlacedIntegrationEvent>(outboxMessage.Payload);
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.OrderId.Should().Be(persistedOrder.Id);
        deserializedEvent.TotalAmount.Should().Be(1100m);
        deserializedEvent.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task ConfirmOrder_ExistingOrder_ShouldUpdateStatusToConfirmed()
    {
        // Arrange
        await using var ctx = OrderDbContextFactory.Create();
        var order = Order.Domain.Entities.Order.Create(
            Guid.NewGuid(),
            "user@test.com",
            new Address("1 St", "City", "ST", "US", "12345"),
            Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), "Item", 10m, 1);
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();

        var handler = new ConfirmOrderCommandHandler(ctx);

        // Act
        var result = await handler.Handle(new ConfirmOrderCommand(order.Id, Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await ctx.Orders.FindAsync(order.Id);
        updated!.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task CancelOrder_ExistingOrder_ShouldUpdateStatusToCancelledWithReason()
    {
        // Arrange
        await using var ctx = OrderDbContextFactory.Create();
        var order = Order.Domain.Entities.Order.Create(
            Guid.NewGuid(),
            "user@test.com",
            new Address("1 St", "City", "ST", "US", "12345"),
            Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), "Item", 10m, 1);
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();

        var handler = new CancelOrderCommandHandler(ctx);

        // Act
        var result = await handler.Handle(new CancelOrderCommand(order.Id, "Payment declined", Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await ctx.Orders.FindAsync(order.Id);
        updated!.Status.Should().Be(OrderStatus.Cancelled);
        updated.CancellationReason.Should().Be("Payment declined");
    }

    [Fact]
    public async Task GetOrderById_ExistingOrder_ShouldReturnCompleteDto()
    {
        // Arrange
        await using var ctx = OrderDbContextFactory.Create();
        var customerId = Guid.NewGuid();
        var order = Order.Domain.Entities.Order.Create(
            customerId,
            "user@test.com",
            new Address("1 St", "City", "ST", "US", "12345"),
            Guid.NewGuid());
        var prodId = Guid.NewGuid();
        order.AddItem(prodId, "Phone", 599.99m, 1);
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();

        var handler = new GetOrderByIdQueryHandler(ctx);

        // Act
        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(order.Id);
        result.Value.CustomerId.Should().Be(customerId);
        result.Value.TotalAmount.Should().Be(599.99m);
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].ProductId.Should().Be(prodId);
    }
}
