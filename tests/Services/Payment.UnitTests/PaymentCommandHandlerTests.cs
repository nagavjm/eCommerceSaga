using System.Text.Json;
using BuildingBlocks.Contracts.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Payment.Application.Commands;
using Payment.Application.Common.Interfaces;
using Payment.Application.Queries;
using Payment.Domain.Enums;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Services;

namespace Payment.UnitTests;

public static class PaymentDbContextFactory
{
    public static PaymentDbContext Create()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PaymentDbContext(options);
    }
}

public class PaymentCommandHandlerTests
{
    [Fact]
    public async Task ProcessPayment_SuccessfulGateway_ShouldCompletePaymentAndSaveOutboxMessage()
    {
        // Arrange
        await using var ctx = PaymentDbContextFactory.Create();
        var mockGateway = new Mock<IPaymentGateway>();
        mockGateway
            .Setup(g => g.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResult(true, "TXN-APPROVED-123", null));

        var handler = new ProcessPaymentCommandHandler(ctx, mockGateway.Object);
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var command = new ProcessPaymentCommand(orderId, customerId, 199.99m, "USD", correlationId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Completed");
        result.Value.TransactionReference.Should().Be("TXN-APPROVED-123");

        // Verify DB persistence
        var persisted = await ctx.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(PaymentStatus.Completed);

        // Verify Outbox message contains PaymentCompletedIntegrationEvent
        var outboxMessage = await ctx.OutboxMessages.FirstOrDefaultAsync();
        outboxMessage.Should().NotBeNull();
        outboxMessage!.Type.Should().Contain(nameof(PaymentCompletedIntegrationEvent));

        var evt = JsonSerializer.Deserialize<PaymentCompletedIntegrationEvent>(outboxMessage.Payload);
        evt.Should().NotBeNull();
        evt!.OrderId.Should().Be(orderId);
        evt.Amount.Should().Be(199.99m);
        evt.TransactionReference.Should().Be("TXN-APPROVED-123");
    }

    [Fact]
    public async Task ProcessPayment_DeclinedGateway_ShouldMarkAsFailedAndSaveOutboxMessage()
    {
        // Arrange
        await using var ctx = PaymentDbContextFactory.Create();
        var mockGateway = new Mock<IPaymentGateway>();
        mockGateway
            .Setup(g => g.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResult(false, null, "Insufficient funds"));

        var handler = new ProcessPaymentCommandHandler(ctx, mockGateway.Object);
        var orderId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(orderId, Guid.NewGuid(), 500m, "USD", Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Failed");
        result.Value.FailureReason.Should().Be("Insufficient funds");

        var persisted = await ctx.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
        persisted!.Status.Should().Be(PaymentStatus.Failed);

        // Verify Outbox message contains PaymentFailedIntegrationEvent
        var outboxMessage = await ctx.OutboxMessages.FirstOrDefaultAsync();
        outboxMessage.Should().NotBeNull();
        outboxMessage!.Type.Should().Contain(nameof(PaymentFailedIntegrationEvent));
    }

    [Fact]
    public async Task RefundPayment_CompletedPayment_ShouldUpdateStatusAndSaveOutboxMessage()
    {
        // Arrange
        await using var ctx = PaymentDbContextFactory.Create();
        var payment = Payment.Domain.Entities.Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            75m,
            "USD",
            Guid.NewGuid());
        payment.MarkAsCompleted("TXN-REF-123");
        ctx.Payments.Add(payment);
        await ctx.SaveChangesAsync();

        var mockGateway = new Mock<IPaymentGateway>();
        mockGateway
            .Setup(g => g.RefundPaymentAsync("TXN-REF-123", 75m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayResult(true, "REFUND-123", null));

        var handler = new RefundPaymentCommandHandler(ctx, mockGateway.Object);

        // Act
        var result = await handler.Handle(new RefundPaymentCommand(payment.OrderId, 75m, Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await ctx.Payments.FindAsync(payment.Id);
        updated!.Status.Should().Be(PaymentStatus.Refunded);

        var outboxMessage = await ctx.OutboxMessages.FirstOrDefaultAsync();
        outboxMessage.Should().NotBeNull();
        outboxMessage!.Type.Should().Contain(nameof(PaymentRefundedIntegrationEvent));
    }

    [Fact]
    public async Task SandboxPaymentGateway_NormalAmount_ShouldSucceed()
    {
        // Arrange
        var gateway = new SandboxPaymentGateway();

        // Act
        var result = await gateway.ProcessPaymentAsync(99.99m, "USD", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.TransactionReference.Should().StartWith("SANDBOX-");
    }

    [Fact]
    public async Task SandboxPaymentGateway_AmountOver10000_ShouldDecline()
    {
        // Arrange
        var gateway = new SandboxPaymentGateway();

        // Act
        var result = await gateway.ProcessPaymentAsync(10000.00m, "USD", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.FailureReason.Should().Contain("declined");
    }
}
