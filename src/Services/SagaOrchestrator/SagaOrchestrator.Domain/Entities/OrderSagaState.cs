using BuildingBlocks.Common.Primitives;
using SagaOrchestrator.Domain.Enums;
using SagaOrchestrator.Domain.Events;

namespace SagaOrchestrator.Domain.Entities;

public class OrderSagaState : AggregateRoot<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerEmail { get; private set; } = string.Empty;
    public string ShippingAddress { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public SagaStateEnum CurrentState { get; private set; }
    public string? FailedStep { get; private set; }
    public string? FailureReason { get; private set; }
    public int RetryCount { get; private set; }
    public CompensationStatus CompensationStatus { get; private set; }
    public Guid CorrelationId { get; private set; }
    public string? TrackingNumber { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public List<string> CompletedSteps { get; private set; } = [];

    private OrderSagaState() { }

    public static OrderSagaState Start(
        Guid orderId,
        Guid customerId,
        string customerEmail,
        string shippingAddress,
        decimal totalAmount,
        Guid correlationId)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));

        var saga = new OrderSagaState
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            CustomerEmail = customerEmail.Trim().ToLowerInvariant(),
            ShippingAddress = shippingAddress.Trim(),
            TotalAmount = totalAmount,
            CurrentState = SagaStateEnum.Started,
            CompensationStatus = CompensationStatus.None,
            RetryCount = 0,
            CorrelationId = correlationId == Guid.Empty ? Guid.NewGuid() : correlationId,
            CreatedAtUtc = DateTime.UtcNow
        };

        saga.CompletedSteps.Add("OrderPlaced");
        saga.AddDomainEvent(new SagaStartedDomainEvent(saga.Id, orderId, saga.CorrelationId));
        return saga;
    }

    public void TransitionTo(SagaStateEnum newState, string completedStep)
    {
        CurrentState = newState;
        if (!string.IsNullOrWhiteSpace(completedStep) && !CompletedSteps.Contains(completedStep))
        {
            CompletedSteps.Add(completedStep);
        }

        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new SagaStepCompletedDomainEvent(Id, OrderId, completedStep, CorrelationId));

        if (newState == SagaStateEnum.Completed)
        {
            AddDomainEvent(new SagaCompletedDomainEvent(Id, OrderId, CorrelationId));
        }
    }

    public void SetTrackingNumber(string trackingNumber)
    {
        TrackingNumber = trackingNumber;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void FailStep(string stepName, string reason)
    {
        FailedStep = stepName;
        FailureReason = reason;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void StartCompensation(string reason)
    {
        CompensationStatus = CompensationStatus.InProgress;
        FailureReason = reason;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void CompleteCompensation()
    {
        CompensationStatus = CompensationStatus.Completed;
        CurrentState = SagaStateEnum.Failed;
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new SagaFailedDomainEvent(Id, OrderId, FailureReason ?? "Saga compensated and marked as failed", CorrelationId));
    }

    public void Escalate(string reason)
    {
        CurrentState = SagaStateEnum.EscalatedForManualIntervention;
        FailureReason = reason;
        CompensationStatus = CompensationStatus.Failed;
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new SagaEscalatedDomainEvent(Id, OrderId, reason, CorrelationId));
    }

    public void IncrementRetry()
    {
        RetryCount++;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
