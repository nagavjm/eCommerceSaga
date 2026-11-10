using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Notification.Application.Commands;
using Notification.Application.Common.Interfaces;
using Notification.Application.Queries;
using Notification.Domain.Enums;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Services;

namespace Notification.UnitTests;

public static class NotificationDbContextFactory
{
    public static NotificationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NotificationDbContext(options);
    }
}

public class NotificationCommandHandlerTests
{
    [Fact]
    public async Task SendNotification_SuccessfulEmail_ShouldLogSentNotification()
    {
        // Arrange
        await using var ctx = NotificationDbContextFactory.Create();
        var mockSender = new Mock<IEmailSender>();
        mockSender
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new SendNotificationCommandHandler(ctx, mockSender.Object);
        var correlationId = Guid.NewGuid();

        var command = new SendNotificationCommand(
            "alice@example.com",
            "Order Placed",
            "Your order has been placed successfully.",
            NotificationType.OrderConfirmation,
            NotificationChannel.Email,
            correlationId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Sent");
        result.Value.Recipient.Should().Be("alice@example.com");

        var persisted = await ctx.Notifications.FirstOrDefaultAsync();
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(NotificationStatus.Sent);
        persisted.SentAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task SendNotification_FailingEmail_ShouldLogFailedNotification()
    {
        // Arrange
        await using var ctx = NotificationDbContextFactory.Create();
        var mockSender = new Mock<IEmailSender>();
        mockSender
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new SendNotificationCommandHandler(ctx, mockSender.Object);

        var command = new SendNotificationCommand(
            "bob@example.com",
            "Order Confirmation",
            "Order body",
            NotificationType.OrderConfirmation,
            NotificationChannel.Email,
            Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Failed");

        var persisted = await ctx.Notifications.FirstOrDefaultAsync();
        persisted!.Status.Should().Be(NotificationStatus.Failed);
        persisted.FailureReason.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNotificationsByRecipient_ShouldReturnAllMatching()
    {
        // Arrange
        await using var ctx = NotificationDbContextFactory.Create();
        var email = "charlie@example.com";

        var notif1 = Notification.Domain.Entities.NotificationLog.Create(
            email, "Sub 1", "Body 1", NotificationType.OrderConfirmation, NotificationChannel.Email, Guid.NewGuid());
        var notif2 = Notification.Domain.Entities.NotificationLog.Create(
            email, "Sub 2", "Body 2", NotificationType.PaymentReceipt, NotificationChannel.Email, Guid.NewGuid());
        var notifOther = Notification.Domain.Entities.NotificationLog.Create(
            "other@example.com", "Sub 3", "Body 3", NotificationType.OrderConfirmation, NotificationChannel.Email, Guid.NewGuid());

        ctx.Notifications.AddRange(notif1, notif2, notifOther);
        await ctx.SaveChangesAsync();

        var handler = new GetNotificationsByRecipientQueryHandler(ctx);

        // Act
        var result = await handler.Handle(new GetNotificationsByRecipientQuery(email), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task EmailSender_Simulation_DeliveryFailureKeyword_ShouldFail()
    {
        // Arrange
        var sender = new EmailSender(NullLogger<EmailSender>.Instance);

        // Act
        var successResult = await sender.SendEmailAsync("user@test.com", "Test", "Body", CancellationToken.None);
        var failResult = await sender.SendEmailAsync("fail-delivery@test.com", "Test", "Body", CancellationToken.None);

        // Assert
        successResult.Should().BeTrue();
        failResult.Should().BeFalse();
    }
}
