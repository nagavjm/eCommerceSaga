using BuildingBlocks.Messaging.Inbox;
using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Shipping.Domain.Entities;

namespace Shipping.Application.Common.Interfaces;

public interface IShippingDbContext
{
    DbSet<Shipment> Shipments { get; }
    DbSet<ShipmentItem> ShipmentItems { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<InboxMessage> InboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
