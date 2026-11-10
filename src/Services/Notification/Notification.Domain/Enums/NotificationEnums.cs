namespace Notification.Domain.Enums;

public enum NotificationType
{
    OrderConfirmation = 1,
    PaymentReceipt = 2,
    ShipmentDispatched = 3,
    OrderCancelled = 4
}

public enum NotificationChannel
{
    Email = 1,
    Sms = 2
}

public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3
}
