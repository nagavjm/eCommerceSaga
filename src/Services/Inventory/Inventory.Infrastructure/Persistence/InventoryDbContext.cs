using BuildingBlocks.Messaging.Inbox;
using BuildingBlocks.Messaging.Outbox;
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
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

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

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("OutboxMessages");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).ValueGeneratedNever();

            builder.Property(m => m.Type)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(m => m.Payload)
                .IsRequired();

            builder.Property(m => m.OccurredOnUtc)
                .IsRequired();
        });

        modelBuilder.Entity<InboxMessage>(builder =>
        {
            builder.ToTable("InboxMessages");
            builder.HasKey(m => new { m.Id, m.HandlerName });
            builder.Property(m => m.Id).ValueGeneratedNever();
            builder.Property(m => m.HandlerName).HasMaxLength(200);
            builder.Property(m => m.ReceivedOnUtc).IsRequired();
        });
    }
}
