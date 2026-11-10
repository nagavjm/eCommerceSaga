using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Common.Interfaces;

public interface IInventoryDbContext
{
    DbSet<InventoryItem> InventoryItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
