using BuildingBlocks.Messaging.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Inbox;

public class EfInboxStore<TDbContext> : IInboxStore where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly ILogger<EfInboxStore<TDbContext>> _logger;

    public EfInboxStore(TDbContext dbContext, ILogger<EfInboxStore<TDbContext>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> HasBeenProcessedAsync(Guid messageId, string handlerName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<InboxMessage>()
            .AnyAsync(m => m.Id == messageId && m.HandlerName == handlerName && m.ProcessedOnUtc != null, cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid messageId, string handlerName, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Set<InboxMessage>()
            .FirstOrDefaultAsync(m => m.Id == messageId && m.HandlerName == handlerName, cancellationToken);

        if (existing is null)
        {
            var inboxMessage = new InboxMessage
            {
                Id = messageId,
                HandlerName = handlerName,
                ReceivedOnUtc = DateTime.UtcNow,
                ProcessedOnUtc = DateTime.UtcNow
            };

            _dbContext.Set<InboxMessage>().Add(inboxMessage);
        }
        else
        {
            existing.ProcessedOnUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
