using BuildingBlocks.Messaging.Outbox;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace BuildingBlocks.Tests;

public class TestMessagingDbContext : DbContext
{
    public TestMessagingDbContext(DbContextOptions<TestMessagingDbContext> options) : base(options)
    {
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<BuildingBlocks.Messaging.Inbox.InboxMessage> InboxMessages => Set<BuildingBlocks.Messaging.Inbox.InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.HasKey(m => m.Id);
            b.Property(m => m.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<BuildingBlocks.Messaging.Inbox.InboxMessage>(b =>
        {
            b.HasKey(m => new { m.Id, m.HandlerName });
            b.Property(m => m.Id).ValueGeneratedNever();
        });
    }
}

public class EfOutboxStoreTests
{
    private readonly TestMessagingDbContext _dbContext;
    private readonly EfOutboxStore<TestMessagingDbContext> _outboxStore;

    public EfOutboxStoreTests()
    {
        var options = new DbContextOptionsBuilder<TestMessagingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestMessagingDbContext(options);
        _outboxStore = new EfOutboxStore<TestMessagingDbContext>(_dbContext, NullLogger<EfOutboxStore<TestMessagingDbContext>>.Instance);
    }

    [Fact]
    public async Task GetUnprocessedMessagesAsync_ShouldReturnOnlyUnprocessedMessages()
    {
        // Arrange
        var msg1 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow.AddMinutes(-5),
            Type = "TestEvent",
            Payload = "{}",
            ProcessedOnUtc = null,
            RetryCount = 0
        };

        var msg2 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow.AddMinutes(-2),
            Type = "TestEvent2",
            Payload = "{}",
            ProcessedOnUtc = DateTime.UtcNow,
            RetryCount = 0
        };

        _dbContext.OutboxMessages.AddRange(msg1, msg2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _outboxStore.GetUnprocessedMessagesAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(msg1.Id);
    }

    [Fact]
    public async Task MarkAsProcessedAsync_ShouldUpdateProcessedTimestamp()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var msg = new OutboxMessage
        {
            Id = messageId,
            OccurredOnUtc = DateTime.UtcNow,
            Type = "TestEvent",
            Payload = "{}",
            ProcessedOnUtc = null,
            RetryCount = 0
        };

        _dbContext.OutboxMessages.Add(msg);
        await _dbContext.SaveChangesAsync();

        // Act
        await _outboxStore.MarkAsProcessedAsync(messageId);

        // Assert
        var updated = await _dbContext.OutboxMessages.FindAsync(messageId);
        updated.Should().NotBeNull();
        updated!.ProcessedOnUtc.Should().NotBeNull();
        updated.Error.Should().BeNull();
    }

    [Fact]
    public async Task RecordFailureAsync_ShouldIncrementRetryCountAndSetError()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var msg = new OutboxMessage
        {
            Id = messageId,
            OccurredOnUtc = DateTime.UtcNow,
            Type = "TestEvent",
            Payload = "{}",
            ProcessedOnUtc = null,
            RetryCount = 1
        };

        _dbContext.OutboxMessages.Add(msg);
        await _dbContext.SaveChangesAsync();

        // Act
        await _outboxStore.RecordFailureAsync(messageId, "Connection timeout");

        // Assert
        var updated = await _dbContext.OutboxMessages.FindAsync(messageId);
        updated.Should().NotBeNull();
        updated!.RetryCount.Should().Be(2);
        updated.Error.Should().Be("Connection timeout");
    }
}
