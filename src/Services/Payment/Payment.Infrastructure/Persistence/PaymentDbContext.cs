using BuildingBlocks.Messaging.Inbox;
using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Common.Interfaces;

namespace Payment.Infrastructure.Persistence;

public class PaymentDbContext : DbContext, IPaymentDbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Payment> Payments => Set<Domain.Entities.Payment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Domain.Entities.Payment>(builder =>
        {
            builder.ToTable("Payments");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(p => p.OrderId)
                .IsRequired();

            builder.HasIndex(p => p.OrderId)
                .IsUnique();

            builder.Property(p => p.CustomerId)
                .IsRequired();

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(p => p.TransactionReference)
                .HasMaxLength(100);

            builder.Property(p => p.FailureReason)
                .HasMaxLength(500);

            builder.Property(p => p.CorrelationId)
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
