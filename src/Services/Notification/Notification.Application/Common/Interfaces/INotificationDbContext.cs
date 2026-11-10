using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;

namespace Notification.Application.Common.Interfaces;

public interface INotificationDbContext
{
    DbSet<NotificationLog> Notifications { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
