using FluentAssertions;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Domain.Events;

namespace Payment.UnitTests;

public class PaymentDomainTests
{
    [Fact]
    public void Create_WithValidData_ShouldInitializePendingPayment()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        // Act
        var payment = Payment.Domain.Entities.Payment.Create(
            orderId,
            customerId,
            150.00m,
            "USD",
            correlationId);

        // Assert
        payment.Id.Should().NotBeEmpty();
        payment.OrderId.Should().Be(orderId);
        payment.CustomerId.Should().Be(customerId);
        payment.Amount.Should().Be(150.00m);
        payment.Currency.Should().Be("USD");
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.CorrelationId.Should().Be(correlationId);
        payment.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WithInvalidAmount_ShouldThrowArgumentException(decimal amount)
    {
        // Act
        var act = () => Payment.Domain.Entities.Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            amount,
            "USD",
            Guid.NewGuid());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Amount must be greater than zero*");
    }

    [Fact]
    public void MarkAsCompleted_FromPending_ShouldSucceedAndRaiseDomainEvent()
    {
        // Arrange
        var payment = Payment.Domain.Entities.Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            200m,
            "USD",
            Guid.NewGuid());

        // Act
        payment.MarkAsCompleted("TXN-123456");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.TransactionReference.Should().Be("TXN-123456");
        payment.CompletedAtUtc.Should().NotBeNull();
        payment.DomainEvents.Should().ContainSingle(e => e is PaymentCompletedDomainEvent);
    }

    [Fact]
    public void MarkAsFailed_FromPending_ShouldSucceedAndRaiseDomainEvent()
    {
        // Arrange
        var payment = Payment.Domain.Entities.Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            200m,
            "USD",
            Guid.NewGuid());

        // Act
        payment.MarkAsFailed("Card declined: Insufficient funds");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be("Card declined: Insufficient funds");
        payment.DomainEvents.Should().ContainSingle(e => e is PaymentFailedDomainEvent);
    }

    [Fact]
    public void MarkAsRefunded_WhenCompleted_ShouldSucceedAndRaiseDomainEvent()
    {
        // Arrange
        var payment = Payment.Domain.Entities.Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            200m,
            "USD",
            Guid.NewGuid());
        payment.MarkAsCompleted("TXN-123456");

        // Act
        payment.MarkAsRefunded();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.DomainEvents.Should().Contain(e => e is PaymentRefundedDomainEvent);
    }

    [Fact]
    public void MarkAsRefunded_WhenPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = Payment.Domain.Entities.Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            200m,
            "USD",
            Guid.NewGuid());

        // Act
        var act = () => payment.MarkAsRefunded();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot refund payment with status 'Pending'*");
    }
}
