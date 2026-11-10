using FluentAssertions;
using SagaOrchestrator.Domain.Entities;
using SagaOrchestrator.Domain.Enums;
using SagaOrchestrator.Domain.Events;

namespace SagaOrchestrator.UnitTests;

public class SagaDomainTests
{
    [Fact]
    public void Start_WithValidData_ShouldInitializeSagaInStartedState()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        // Act
        var saga = OrderSagaState.Start(
            orderId,
            customerId,
            "buyer@example.com",
            "123 Main St",
            250.00m,
            correlationId);

        // Assert
        saga.Id.Should().NotBeEmpty();
        saga.OrderId.Should().Be(orderId);
        saga.CustomerId.Should().Be(customerId);
        saga.CustomerEmail.Should().Be("buyer@example.com");
        saga.ShippingAddress.Should().Be("123 Main St");
        saga.TotalAmount.Should().Be(250.00m);
        saga.CurrentState.Should().Be(SagaStateEnum.Started);
        saga.CompensationStatus.Should().Be(CompensationStatus.None);
        saga.RetryCount.Should().Be(0);
        saga.CorrelationId.Should().Be(correlationId);
        saga.CompletedSteps.Should().ContainSingle(s => s == "OrderPlaced");
        saga.DomainEvents.Should().ContainSingle(e => e is SagaStartedDomainEvent);
    }

    [Fact]
    public void TransitionTo_ShouldAdvanceStateAndRecordCompletedStep()
    {
        // Arrange
        var saga = OrderSagaState.Start(Guid.NewGuid(), Guid.NewGuid(), "a@b.com", "Addr", 100m, Guid.NewGuid());

        // Act
        saga.TransitionTo(SagaStateEnum.ReservingStock, "OrderPlaced");
        saga.TransitionTo(SagaStateEnum.StockReserved, "StockReserved");

        // Assert
        saga.CurrentState.Should().Be(SagaStateEnum.StockReserved);
        saga.CompletedSteps.Should().Contain("StockReserved");
        saga.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void TransitionTo_CompletedState_ShouldRaiseSagaCompletedDomainEvent()
    {
        // Arrange
        var saga = OrderSagaState.Start(Guid.NewGuid(), Guid.NewGuid(), "a@b.com", "Addr", 100m, Guid.NewGuid());

        // Act
        saga.TransitionTo(SagaStateEnum.Completed, "NotificationSent");

        // Assert
        saga.CurrentState.Should().Be(SagaStateEnum.Completed);
        saga.DomainEvents.Should().Contain(e => e is SagaCompletedDomainEvent);
    }

    [Fact]
    public void FailStep_ShouldRecordFailedStepAndReason()
    {
        // Arrange
        var saga = OrderSagaState.Start(Guid.NewGuid(), Guid.NewGuid(), "a@b.com", "Addr", 100m, Guid.NewGuid());

        // Act
        saga.FailStep("ReserveStock", "Insufficient stock for Product 123");

        // Assert
        saga.FailedStep.Should().Be("ReserveStock");
        saga.FailureReason.Should().Be("Insufficient stock for Product 123");
    }

    [Fact]
    public void StartCompensation_ShouldSetCompensationStatusInProgress()
    {
        // Arrange
        var saga = OrderSagaState.Start(Guid.NewGuid(), Guid.NewGuid(), "a@b.com", "Addr", 100m, Guid.NewGuid());

        // Act
        saga.StartCompensation("Payment was declined");

        // Assert
        saga.CompensationStatus.Should().Be(CompensationStatus.InProgress);
        saga.FailureReason.Should().Be("Payment was declined");
    }

    [Fact]
    public void CompleteCompensation_ShouldSetCompensationStatusCompletedAndStateFailed()
    {
        // Arrange
        var saga = OrderSagaState.Start(Guid.NewGuid(), Guid.NewGuid(), "a@b.com", "Addr", 100m, Guid.NewGuid());
        saga.StartCompensation("Payment failed");

        // Act
        saga.CompleteCompensation();

        // Assert
        saga.CompensationStatus.Should().Be(CompensationStatus.Completed);
        saga.CurrentState.Should().Be(SagaStateEnum.Failed);
        saga.DomainEvents.Should().Contain(e => e is SagaFailedDomainEvent);
    }

    [Fact]
    public void Escalate_ShouldSetStateToEscalatedForManualIntervention()
    {
        // Arrange
        var saga = OrderSagaState.Start(Guid.NewGuid(), Guid.NewGuid(), "a@b.com", "Addr", 100m, Guid.NewGuid());

        // Act
        saga.Escalate("Carrier warehouse unavailable");

        // Assert
        saga.CurrentState.Should().Be(SagaStateEnum.EscalatedForManualIntervention);
        saga.CompensationStatus.Should().Be(CompensationStatus.Failed);
        saga.DomainEvents.Should().Contain(e => e is SagaEscalatedDomainEvent);
    }

    [Fact]
    public void IncrementRetry_ShouldIncreaseRetryCount()
    {
        // Arrange
        var saga = OrderSagaState.Start(Guid.NewGuid(), Guid.NewGuid(), "a@b.com", "Addr", 100m, Guid.NewGuid());

        // Act
        saga.IncrementRetry();
        saga.IncrementRetry();

        // Assert
        saga.RetryCount.Should().Be(2);
    }
}
