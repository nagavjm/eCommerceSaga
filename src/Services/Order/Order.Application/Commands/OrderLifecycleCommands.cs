using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order.Application.Common.Interfaces;

namespace Order.Application.Commands;

public sealed record ConfirmOrderCommand(Guid OrderId, Guid CorrelationId) : IRequest<Result>;

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, Result>
{
    private readonly IOrderDbContext _dbContext;

    public ConfirmOrderCommandHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Error.NotFound("Order.NotFound", $"Order with ID '{request.OrderId}' was not found.");
        }

        order.Confirm();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed record CancelOrderCommand(Guid OrderId, string Reason, Guid CorrelationId) : IRequest<Result>;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly IOrderDbContext _dbContext;

    public CancelOrderCommandHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Error.NotFound("Order.NotFound", $"Order with ID '{request.OrderId}' was not found.");
        }

        order.Cancel(request.Reason);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
