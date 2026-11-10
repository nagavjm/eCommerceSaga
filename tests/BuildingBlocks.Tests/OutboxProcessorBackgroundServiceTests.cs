using System.Reflection;
using System.Text.Json;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging.Abstractions;
using BuildingBlocks.Messaging.Outbox;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BuildingBlocks.Tests;

public class OutboxProcessorBackgroundServiceTests
{
    [Fact]
    public async Task ProcessOutboxMessagesAsync_WhenUnprocessedMessagesExist_ShouldPublishAndMarkProcessed()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockOutboxStore = new Mock<IOutboxStore>();
        var mockPublisher = new Mock<IMessagePublisher>();

        var testEvent = new StockReservedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid());
        var messageId = Guid.NewGuid();
        var outboxMessage = new OutboxMessage
        {
            Id = messageId,
            Type = typeof(StockReservedIntegrationEvent).AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(testEvent),
            OccurredOnUtc = DateTime.UtcNow,
            ProcessedOnUtc = null,
            RetryCount = 0
        };

        mockOutboxStore
            .Setup(x => x.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { outboxMessage });

        services.AddScoped(_ => mockOutboxStore.Object);
        services.AddScoped(_ => mockPublisher.Object);

        var serviceProvider = services.BuildServiceProvider();
        var processor = new OutboxProcessorBackgroundService(
            serviceProvider,
            NullLogger<OutboxProcessorBackgroundService>.Instance);

        var method = typeof(OutboxProcessorBackgroundService).GetMethod(
            "ProcessOutboxMessagesAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);

        method.Should().NotBeNull();

        // Act
        var task = (Task)method!.Invoke(processor, new object[] { CancellationToken.None })!;
        await task;

        // Assert
        mockPublisher.Verify(
            x => x.PublishAsync(
                It.Is<object>(o => o is StockReservedIntegrationEvent && ((StockReservedIntegrationEvent)o).OrderId == testEvent.OrderId),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        mockOutboxStore.Verify(
            x => x.MarkAsProcessedAsync(messageId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessOutboxMessagesAsync_WhenTypeCannotBeResolved_ShouldRecordFailure()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockOutboxStore = new Mock<IOutboxStore>();
        var mockPublisher = new Mock<IMessagePublisher>();

        var messageId = Guid.NewGuid();
        var outboxMessage = new OutboxMessage
        {
            Id = messageId,
            Type = "NonExistentNamespace.NonExistentType, SomeAssembly",
            Payload = "{}",
            OccurredOnUtc = DateTime.UtcNow,
            ProcessedOnUtc = null,
            RetryCount = 0
        };

        mockOutboxStore
            .Setup(x => x.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { outboxMessage });

        services.AddScoped(_ => mockOutboxStore.Object);
        services.AddScoped(_ => mockPublisher.Object);

        var serviceProvider = services.BuildServiceProvider();
        var processor = new OutboxProcessorBackgroundService(
            serviceProvider,
            NullLogger<OutboxProcessorBackgroundService>.Instance);

        var method = typeof(OutboxProcessorBackgroundService).GetMethod(
            "ProcessOutboxMessagesAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var task = (Task)method!.Invoke(processor, new object[] { CancellationToken.None })!;
        await task;

        // Assert
        mockPublisher.Verify(
            x => x.PublishAsync(
                It.IsAny<object>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        mockOutboxStore.Verify(
            x => x.RecordFailureAsync(
                messageId,
                It.Is<string>(s => s.Contains("Unknown type")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
