using BuildingBlocks.Common.Primitives;

namespace Notification.Domain.Events;

public sealed record NotificationSentDomainEvent(
    Guid NotificationId,
    string Recipient,
    string NotificationType,
    Guid CorrelationId) : IDomainEvent;

public sealed record NotificationFailedDomainEvent(
    Guid NotificationId,
    string Recipient,
    string Reason,
    Guid CorrelationId) : IDomainEvent;
