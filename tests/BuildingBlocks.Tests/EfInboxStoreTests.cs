using BuildingBlocks.Messaging.Inbox;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace BuildingBlocks.Tests;

public class EfInboxStoreTests
{
    private readonly TestMessagingDbContext _dbContext;
    private readonly EfInboxStore<TestMessagingDbContext> _inboxStore;

    public EfInboxStoreTests()
    {
        var options = new DbContextOptionsBuilder<TestMessagingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestMessagingDbContext(options);
        _inboxStore = new EfInboxStore<TestMessagingDbContext>(_dbContext, NullLogger<EfInboxStore<TestMessagingDbContext>>.Instance);
    }

    [Fact]
    public async Task HasBeenProcessedAsync_WhenMessageNotProcessed_ShouldReturnFalse()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var handlerName = "OrderCreatedHandler";

        // Act
        var result = await _inboxStore.HasBeenProcessedAsync(messageId, handlerName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAsProcessedAsync_ShouldPersistAndMakeHasBeenProcessedReturnTrue()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var handlerName = "StockReservedHandler";

        // Act
        await _inboxStore.MarkAsProcessedAsync(messageId, handlerName);

        // Assert
        var hasBeenProcessed = await _inboxStore.HasBeenProcessedAsync(messageId, handlerName);
        hasBeenProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task HasBeenProcessedAsync_WithDifferentHandler_ShouldDistinguishSeparately()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var handlerA = "HandlerA";
        var handlerB = "HandlerB";

        await _inboxStore.MarkAsProcessedAsync(messageId, handlerA);

        // Act & Assert
        (await _inboxStore.HasBeenProcessedAsync(messageId, handlerA)).Should().BeTrue();
        (await _inboxStore.HasBeenProcessedAsync(messageId, handlerB)).Should().BeFalse();
    }
}
