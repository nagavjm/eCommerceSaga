using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SagaOrchestrator.Application.Common.Interfaces;
using SagaOrchestrator.Application.DTOs;
using SagaOrchestrator.Domain.Entities;
using SagaOrchestrator.Domain.Enums;

namespace SagaOrchestrator.Application.Queries;

public sealed record GetSagaByOrderIdQuery(Guid OrderId) : IRequest<Result<SagaStatusDto>>;

public class GetSagaByOrderIdQueryHandler : IRequestHandler<GetSagaByOrderIdQuery, Result<SagaStatusDto>>
{
    private readonly ISagaDbContext _dbContext;

    public GetSagaByOrderIdQueryHandler(ISagaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<SagaStatusDto>> Handle(GetSagaByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var saga = await _dbContext.SagaStates
            .FirstOrDefaultAsync(s => s.OrderId == request.OrderId, cancellationToken);

        if (saga is null)
        {
            return Error.NotFound("Saga.NotFound", $"No saga found for Order ID '{request.OrderId}'.");
        }

        return MapToDto(saga);
    }

    private static SagaStatusDto MapToDto(OrderSagaState saga)
    {
        return new SagaStatusDto(
            saga.Id,
            saga.OrderId,
            saga.CustomerId,
            saga.CustomerEmail,
            saga.CurrentState.ToString(),
            saga.CompletedSteps.ToList(),
            saga.FailedStep,
            saga.FailureReason,
            saga.RetryCount,
            saga.CompensationStatus.ToString(),
            saga.TrackingNumber,
            saga.CorrelationId,
            saga.CreatedAtUtc,
            saga.UpdatedAtUtc);
    }
}

public sealed record GetFailedOrEscalatedSagasQuery : IRequest<Result<IReadOnlyList<SagaStatusDto>>>;

public class GetFailedOrEscalatedSagasQueryHandler : IRequestHandler<GetFailedOrEscalatedSagasQuery, Result<IReadOnlyList<SagaStatusDto>>>
{
    private readonly ISagaDbContext _dbContext;

    public GetFailedOrEscalatedSagasQueryHandler(ISagaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<SagaStatusDto>>> Handle(GetFailedOrEscalatedSagasQuery request, CancellationToken cancellationToken)
    {
        var sagas = await _dbContext.SagaStates
            .Where(s => s.CurrentState == SagaStateEnum.Failed || s.CurrentState == SagaStateEnum.EscalatedForManualIntervention)
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        IReadOnlyList<SagaStatusDto> dtos = sagas.Select(s => new SagaStatusDto(
            s.Id,
            s.OrderId,
            s.CustomerId,
            s.CustomerEmail,
            s.CurrentState.ToString(),
            s.CompletedSteps.ToList(),
            s.FailedStep,
            s.FailureReason,
            s.RetryCount,
            s.CompensationStatus.ToString(),
            s.TrackingNumber,
            s.CorrelationId,
            s.CreatedAtUtc,
            s.UpdatedAtUtc)).ToList();

        return Result<IReadOnlyList<SagaStatusDto>>.Success(dtos);
    }
}
