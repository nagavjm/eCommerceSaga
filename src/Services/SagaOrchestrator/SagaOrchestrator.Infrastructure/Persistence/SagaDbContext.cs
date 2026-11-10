using BuildingBlocks.Messaging.Inbox;
using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using SagaOrchestrator.Application.Common.Interfaces;
using SagaOrchestrator.Domain.Entities;

namespace SagaOrchestrator.Infrastructure.Persistence;

public class SagaDbContext : DbContext, ISagaDbContext
{
    public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options)
    {
    }

    public DbSet<OrderSagaState> SagaStates => Set<OrderSagaState>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderSagaState>(builder =>
        {
            builder.ToTable("SagaStates");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).ValueGeneratedNever();

            builder.Property(s => s.OrderId)
                .IsRequired();

            builder.HasIndex(s => s.OrderId)
                .IsUnique();

            builder.Property(s => s.CustomerId)
                .IsRequired();

            builder.Property(s => s.CustomerEmail)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(s => s.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(s => s.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(s => s.CurrentState)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(s => s.CompensationStatus)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(s => s.FailedStep)
                .HasMaxLength(100);

            builder.Property(s => s.FailureReason)
                .HasMaxLength(500);

            builder.Property(s => s.TrackingNumber)
                .HasMaxLength(50);

            builder.Property(s => s.CorrelationId)
                .IsRequired();

            // EF Core 9 natively supports primitive collections: List<string>
            builder.PrimitiveCollection(s => s.CompletedSteps);
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
