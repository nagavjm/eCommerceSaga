using FluentAssertions;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Domain.Events;

namespace Notification.UnitTests;

public class NotificationDomainTests
{
    [Fact]
    public void Create_WithValidData_ShouldInitializePendingNotification()
    {
        // Arrange
        var correlationId = Guid.NewGuid();

        // Act
        var notification = NotificationLog.Create(
            "customer@example.com",
            "Order Confirmed",
            "Thank you for your order!",
            NotificationType.OrderConfirmation,
            NotificationChannel.Email,
            correlationId);

        // Assert
        notification.Id.Should().NotBeEmpty();
        notification.Recipient.Should().Be("customer@example.com");
        notification.Subject.Should().Be("Order Confirmed");
        notification.Body.Should().Be("Thank you for your order!");
        notification.Type.Should().Be(NotificationType.OrderConfirmation);
        notification.Channel.Should().Be(NotificationChannel.Email);
        notification.Status.Should().Be(NotificationStatus.Pending);
        notification.CorrelationId.Should().Be(correlationId);
        notification.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        notification.SentAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData("", "Subject", "Body")]
    [InlineData("test@example.com", "", "Body")]
    [InlineData("test@example.com", "Subject", "")]
    public void Create_WithInvalidArguments_ShouldThrowArgumentException(string recipient, string subject, string body)
    {
        // Act
        var act = () => NotificationLog.Create(
            recipient,
            subject,
            body,
            NotificationType.PaymentReceipt,
            NotificationChannel.Email,
            Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkAsSent_ShouldUpdateStatusAndSentAtUtcAndRaiseDomainEvent()
    {
        // Arrange
        var notification = NotificationLog.Create(
            "customer@example.com",
            "Order Shipped",
            "Your package is on its way!",
            NotificationType.ShipmentDispatched,
            NotificationChannel.Email,
            Guid.NewGuid());

        // Act
        notification.MarkAsSent();

        // Assert
        notification.Status.Should().Be(NotificationStatus.Sent);
        notification.SentAtUtc.Should().NotBeNull();
        notification.DomainEvents.Should().ContainSingle(e => e is NotificationSentDomainEvent);
    }

    [Fact]
    public void MarkAsFailed_ShouldUpdateStatusAndFailureReasonAndRaiseDomainEvent()
    {
        // Arrange
        var notification = NotificationLog.Create(
            "customer@example.com",
            "Receipt",
            "Payment failed",
            NotificationType.PaymentReceipt,
            NotificationChannel.Email,
            Guid.NewGuid());

        // Act
        notification.MarkAsFailed("SMTP server connection timeout");

        // Assert
        notification.Status.Should().Be(NotificationStatus.Failed);
        notification.FailureReason.Should().Be("SMTP server connection timeout");
        notification.DomainEvents.Should().ContainSingle(e => e is NotificationFailedDomainEvent);
    }
}
