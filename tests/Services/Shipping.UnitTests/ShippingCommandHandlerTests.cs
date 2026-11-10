using System.Text.Json;
using BuildingBlocks.Contracts.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shipping.Application.Commands;
using Shipping.Application.DTOs;
using Shipping.Application.Queries;
using Shipping.Domain.Enums;
using Shipping.Infrastructure.Persistence;

namespace Shipping.UnitTests;

public static class ShippingDbContextFactory
{
    public static ShippingDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ShippingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ShippingDbContext(options);
    }
}

public class ShippingCommandHandlerTests
{
    [Fact]
    public async Task CreateShipment_ValidCommand_ShouldSaveShipmentAndOutboxMessage()
    {
        // Arrange
        await using var ctx = ShippingDbContextFactory.Create();
        var handler = new CreateShipmentCommandHandler(ctx);

        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var command = new CreateShipmentCommand(
            orderId,
            customerId,
            new AddressDto("456 Ocean Ave", "Miami", "FL", "USA", "33101"),
            "FedEx",
            new List<ShipmentItemDto>
            {
                new(Guid.NewGuid(), "Headphones", 1)
            },
            correlationId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.OrderId.Should().Be(orderId);
        result.Value.Status.Should().Be("Created");
        result.Value.TrackingNumber.Should().StartWith("TRK-");

        // Verify DB persistence
        var persisted = await ctx.Shipments.Include(s => s.Items).FirstOrDefaultAsync();
        persisted.Should().NotBeNull();
        persisted!.OrderId.Should().Be(orderId);
        persisted.Items.Should().HaveCount(1);

        // Verify Transactional Outbox
        var outboxMessage = await ctx.OutboxMessages.FirstOrDefaultAsync();
        outboxMessage.Should().NotBeNull();
        outboxMessage!.Type.Should().Contain(nameof(ShipmentCreatedIntegrationEvent));

        var evt = JsonSerializer.Deserialize<ShipmentCreatedIntegrationEvent>(outboxMessage.Payload);
        evt.Should().NotBeNull();
        evt!.OrderId.Should().Be(orderId);
        evt.TrackingNumber.Should().Be(persisted.TrackingNumber);
    }

    [Fact]
    public async Task UpdateShipmentStatus_Dispatched_ShouldUpdateStatus()
    {
        // Arrange
        await using var ctx = ShippingDbContextFactory.Create();
        var shipment = Shipping.Domain.Entities.Shipment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Shipping.Domain.ValueObjects.Address("1 St", "City", "ST", "US", "10001"),
            "DHL",
            Guid.NewGuid());
        shipment.AddItem(Guid.NewGuid(), "Keyboard", 1);
        shipment.FinalizeCreation();
        ctx.Shipments.Add(shipment);
        await ctx.SaveChangesAsync();

        var handler = new UpdateShipmentStatusCommandHandler(ctx);

        // Act
        var result = await handler.Handle(new UpdateShipmentStatusCommand(shipment.Id, ShipmentStatus.Dispatched), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await ctx.Shipments.FindAsync(shipment.Id);
        updated!.Status.Should().Be(ShipmentStatus.Dispatched);
    }

    [Fact]
    public async Task GetShipmentByOrderId_ExistingShipment_ShouldReturnDto()
    {
        // Arrange
        await using var ctx = ShippingDbContextFactory.Create();
        var orderId = Guid.NewGuid();
        var shipment = Shipping.Domain.Entities.Shipment.Create(
            orderId,
            Guid.NewGuid(),
            new Shipping.Domain.ValueObjects.Address("1 St", "City", "ST", "US", "10001"),
            "DHL",
            Guid.NewGuid());
        shipment.AddItem(Guid.NewGuid(), "Mousepad", 2);
        shipment.FinalizeCreation();
        ctx.Shipments.Add(shipment);
        await ctx.SaveChangesAsync();

        var handler = new GetShipmentByOrderIdQueryHandler(ctx);

        // Act
        var result = await handler.Handle(new GetShipmentByOrderIdQuery(orderId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().Be(orderId);
        result.Value.TrackingNumber.Should().Be(shipment.TrackingNumber);
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetShipmentByTrackingNumber_ExistingShipment_ShouldReturnDto()
    {
        // Arrange
        await using var ctx = ShippingDbContextFactory.Create();
        var shipment = Shipping.Domain.Entities.Shipment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Shipping.Domain.ValueObjects.Address("1 St", "City", "ST", "US", "10001"),
            "DHL",
            Guid.NewGuid());
        shipment.AddItem(Guid.NewGuid(), "Tablet", 1);
        shipment.FinalizeCreation();
        ctx.Shipments.Add(shipment);
        await ctx.SaveChangesAsync();

        var handler = new GetShipmentByTrackingNumberQueryHandler(ctx);

        // Act
        var result = await handler.Handle(new GetShipmentByTrackingNumberQuery(shipment.TrackingNumber), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TrackingNumber.Should().Be(shipment.TrackingNumber);
    }
}
