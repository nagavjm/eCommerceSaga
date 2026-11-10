using BuildingBlocks.Common.Primitives;

namespace SagaOrchestrator.Domain.Events;

public sealed record SagaStartedDomainEvent(Guid SagaId, Guid OrderId, Guid CorrelationId) : IDomainEvent;
public sealed record SagaStepCompletedDomainEvent(Guid SagaId, Guid OrderId, string StepName, Guid CorrelationId) : IDomainEvent;
public sealed record SagaCompletedDomainEvent(Guid SagaId, Guid OrderId, Guid CorrelationId) : IDomainEvent;
public sealed record SagaFailedDomainEvent(Guid SagaId, Guid OrderId, string Reason, Guid CorrelationId) : IDomainEvent;
public sealed record SagaEscalatedDomainEvent(Guid SagaId, Guid OrderId, string Reason, Guid CorrelationId) : IDomainEvent;
