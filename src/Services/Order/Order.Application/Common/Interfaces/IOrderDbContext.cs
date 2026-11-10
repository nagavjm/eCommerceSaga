using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;

namespace Order.Application.Common.Interfaces;

public interface IOrderDbContext
{
    DbSet<Domain.Entities.Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
