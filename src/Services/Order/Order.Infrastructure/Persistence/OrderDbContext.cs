using BuildingBlocks.Messaging.Inbox;
using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Order.Application.Common.Interfaces;
using Order.Domain.Entities;
using Order.Domain.Enums;

namespace Order.Infrastructure.Persistence;

public class OrderDbContext : DbContext, IOrderDbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Order> Orders => Set<Domain.Entities.Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Domain.Entities.Order>(builder =>
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ValueGeneratedNever();

            builder.Property(o => o.CustomerId)
                .IsRequired();

            builder.Property(o => o.CustomerEmail)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(o => o.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(o => o.CorrelationId)
                .IsRequired();

            builder.Property(o => o.CancellationReason)
                .HasMaxLength(500);

            builder.OwnsOne(o => o.ShippingAddress, address =>
            {
                address.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(200).IsRequired();
                address.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100).IsRequired();
                address.Property(a => a.State).HasColumnName("ShippingState").HasMaxLength(100).IsRequired();
                address.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(100).IsRequired();
                address.Property(a => a.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20).IsRequired();
            });

            builder.Navigation(o => o.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(builder =>
        {
            builder.ToTable("OrderItems");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).ValueGeneratedNever();

            builder.Property(i => i.OrderId)
                .IsRequired();

            builder.Property(i => i.ProductId)
                .IsRequired();

            builder.Property(i => i.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.Ignore(i => i.TotalPrice);
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
