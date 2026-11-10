using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace BuildingBlocks.Tests;

public class InMemoryMessageBusTests
{
    private readonly InMemoryMessageBus _bus;

    public InMemoryMessageBusTests()
    {
        _bus = new InMemoryMessageBus(NullLogger<InMemoryMessageBus>.Instance);
    }

    [Fact]
    public async Task PublishAsync_WhenSubscribed_ShouldDeliverMessageToSubscriber()
    {
        // Arrange
        OrderPlacedIntegrationEvent? receivedEvent = null;
        var correlationId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var items = new List<OrderItemDto> { new(Guid.NewGuid(), "Laptop", 1, 999.99m) };

        _bus.Subscribe<OrderPlacedIntegrationEvent>((evt, _) =>
        {
            receivedEvent = evt;
            return Task.CompletedTask;
        });

        var orderPlacedEvent = new OrderPlacedIntegrationEvent(
            orderId,
            Guid.NewGuid(),
            "customer@example.com",
            "123 Main St",
            999.99m,
            items,
            correlationId);

        // Act
        await _bus.PublishAsync(orderPlacedEvent);

        // Assert
        receivedEvent.Should().NotBeNull();
        receivedEvent!.OrderId.Should().Be(orderId);
        receivedEvent.CorrelationId.Should().Be(correlationId);
        receivedEvent.TotalAmount.Should().Be(999.99m);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleSubscribers_ShouldDeliverToAllSubscribers()
    {
        // Arrange
        var subscriber1Received = false;
        var subscriber2Received = false;

        _bus.Subscribe<StockReservedIntegrationEvent>((_, _) =>
        {
            subscriber1Received = true;
            return Task.CompletedTask;
        });

        _bus.Subscribe<StockReservedIntegrationEvent>((_, _) =>
        {
            subscriber2Received = true;
            return Task.CompletedTask;
        });

        var stockReservedEvent = new StockReservedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid());

        // Act
        await _bus.PublishAsync(stockReservedEvent);

        // Assert
        subscriber1Received.Should().BeTrue();
        subscriber2Received.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_WhenNoSubscribers_ShouldNotThrowException()
    {
        // Arrange
        var stockReservedEvent = new StockReservedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        var act = async () => await _bus.PublishAsync(stockReservedEvent);
        await act.Should().NotThrowAsync();
    }
}
