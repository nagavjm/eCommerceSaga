using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using SagaOrchestrator.Domain.Entities;

namespace SagaOrchestrator.Application.Common.Interfaces;

public interface ISagaDbContext
{
    DbSet<OrderSagaState> SagaStates { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
