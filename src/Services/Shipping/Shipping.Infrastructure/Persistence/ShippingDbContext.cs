using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Shipping.Application.Common.Interfaces;
using Shipping.Domain.Entities;

namespace Shipping.Infrastructure.Persistence;

public class ShippingDbContext : DbContext, IShippingDbContext
{
    public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options)
    {
    }

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shipment>(builder =>
        {
            builder.ToTable("Shipments");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).ValueGeneratedNever();

            builder.Property(s => s.OrderId)
                .IsRequired();

            builder.HasIndex(s => s.OrderId)
                .IsUnique();

            builder.Property(s => s.CustomerId)
                .IsRequired();

            builder.Property(s => s.TrackingNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(s => s.TrackingNumber)
                .IsUnique();

            builder.Property(s => s.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(s => s.Carrier)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.FailureReason)
                .HasMaxLength(500);

            builder.Property(s => s.CorrelationId)
                .IsRequired();

            builder.OwnsOne(s => s.DeliveryAddress, address =>
            {
                address.Property(a => a.Street).HasColumnName("DeliveryStreet").HasMaxLength(200).IsRequired();
                address.Property(a => a.City).HasColumnName("DeliveryCity").HasMaxLength(100).IsRequired();
                address.Property(a => a.State).HasColumnName("DeliveryState").HasMaxLength(100).IsRequired();
                address.Property(a => a.Country).HasColumnName("DeliveryCountry").HasMaxLength(100).IsRequired();
                address.Property(a => a.ZipCode).HasColumnName("DeliveryZipCode").HasMaxLength(20).IsRequired();
            });

            builder.Navigation(s => s.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(s => s.Items)
                .WithOne()
                .HasForeignKey(i => i.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShipmentItem>(builder =>
        {
            builder.ToTable("ShipmentItems");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).ValueGeneratedNever();

            builder.Property(i => i.ShipmentId)
                .IsRequired();

            builder.Property(i => i.ProductId)
                .IsRequired();

            builder.Property(i => i.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.Quantity)
                .IsRequired();
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
    }
}
