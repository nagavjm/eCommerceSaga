using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Payment.Application.Common.Interfaces;

public interface IPaymentDbContext
{
    DbSet<Domain.Entities.Payment> Payments { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
