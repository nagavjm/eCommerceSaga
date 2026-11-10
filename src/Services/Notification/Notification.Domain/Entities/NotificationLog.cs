using BuildingBlocks.Common.Primitives;
using Notification.Domain.Enums;
using Notification.Domain.Events;

namespace Notification.Domain.Entities;

public class NotificationLog : AggregateRoot<Guid>
{
    public string Recipient { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public Guid CorrelationId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? SentAtUtc { get; private set; }

    private NotificationLog() { }

    public static NotificationLog Create(
        string recipient,
        string subject,
        string body,
        NotificationType type,
        NotificationChannel channel,
        Guid correlationId)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Recipient cannot be empty.", nameof(recipient));
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be empty.", nameof(subject));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be empty.", nameof(body));

        return new NotificationLog
        {
            Id = Guid.NewGuid(),
            Recipient = recipient.Trim().ToLowerInvariant(),
            Subject = subject.Trim(),
            Body = body.Trim(),
            Type = type,
            Channel = channel,
            Status = NotificationStatus.Pending,
            CorrelationId = correlationId == Guid.Empty ? Guid.NewGuid() : correlationId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkAsSent()
    {
        if (Status == NotificationStatus.Sent)
        {
            return;
        }

        Status = NotificationStatus.Sent;
        SentAtUtc = DateTime.UtcNow;
        AddDomainEvent(new NotificationSentDomainEvent(Id, Recipient, Type.ToString(), CorrelationId));
    }

    public void MarkAsFailed(string reason)
    {
        Status = NotificationStatus.Failed;
        FailureReason = reason;
        AddDomainEvent(new NotificationFailedDomainEvent(Id, Recipient, reason, CorrelationId));
    }
}
