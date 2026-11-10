using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Outbox;

public class EfOutboxStore<TDbContext> : IOutboxStore where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly ILogger<EfOutboxStore<TDbContext>> _logger;

    public EfOutboxStore(TDbContext dbContext, ILogger<EfOutboxStore<TDbContext>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 20, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < 5)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _dbContext.Set<OutboxMessage>().FindAsync([messageId], cancellationToken);
        if (message is not null)
        {
            message.ProcessedOnUtc = DateTime.UtcNow;
            message.Error = null;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RecordFailureAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
    {
        var message = await _dbContext.Set<OutboxMessage>().FindAsync([messageId], cancellationToken);
        if (message is not null)
        {
            message.RetryCount++;
            message.Error = error;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
