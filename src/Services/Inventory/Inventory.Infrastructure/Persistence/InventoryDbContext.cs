using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public class InventoryDbContext : DbContext, IInventoryDbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InventoryItem>(builder =>
        {
            builder.ToTable("InventoryItems");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).ValueGeneratedNever();

            builder.Property(i => i.ProductId)
                .IsRequired();

            builder.HasIndex(i => i.ProductId)
                .IsUnique();

            builder.Property(i => i.AvailableStock)
                .IsRequired();

            builder.Property(i => i.ReservedStock)
                .IsRequired();

            builder.Ignore(i => i.TotalStock);
        });
    }
}
