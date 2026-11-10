namespace SagaOrchestrator.Application.DTOs;

public sealed record SagaStatusDto(
    Guid SagaId,
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    string CurrentState,
    IReadOnlyList<string> CompletedSteps,
    string? FailedStep,
    string? FailureReason,
    int RetryCount,
    string CompensationStatus,
    string? TrackingNumber,
    Guid CorrelationId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
