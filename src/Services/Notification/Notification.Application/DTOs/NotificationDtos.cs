namespace Notification.Application.DTOs;

public sealed record NotificationDto(
    Guid Id,
    string Recipient,
    string Subject,
    string Body,
    string Type,
    string Channel,
    string Status,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime? SentAtUtc);

public sealed record SendNotificationRequest(
    string RecipientEmail,
    string Subject,
    string Body,
    string NotificationType = "OrderConfirmation",
    string Channel = "Email");
