using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public static class InventoryDataSeeder
{
    public static readonly Guid Product1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid Product2Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid Product3Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public static async Task SeedAsync(InventoryDbContext context)
    {
        if (await context.InventoryItems.AnyAsync())
        {
            return;
        }

        var item1 = InventoryItem.Create(Product1Id, 50);
        var item2 = InventoryItem.Create(Product2Id, 100);
        var item3 = InventoryItem.Create(Product3Id, 75);

        await context.InventoryItems.AddRangeAsync(item1, item2, item3);
        await context.SaveChangesAsync();
    }
}
