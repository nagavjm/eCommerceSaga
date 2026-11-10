using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Notification.Application.Common.Interfaces;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence;

public class NotificationDbContext : DbContext, INotificationDbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<NotificationLog> Notifications => Set<NotificationLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NotificationLog>(builder =>
        {
            builder.ToTable("Notifications");
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Id).ValueGeneratedNever();

            builder.Property(n => n.Recipient)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(n => n.Recipient);

            builder.Property(n => n.Subject)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Body)
                .IsRequired();

            builder.Property(n => n.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(n => n.Channel)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(n => n.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(n => n.FailureReason)
                .HasMaxLength(500);

            builder.Property(n => n.CorrelationId)
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
