using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shipping.Application.Common.Interfaces;
using Shipping.Domain.Enums;

namespace Shipping.Application.Commands;

public sealed record UpdateShipmentStatusCommand(
    Guid ShipmentId,
    ShipmentStatus NewStatus,
    string? FailureReason = null) : IRequest<Result>;

public class UpdateShipmentStatusCommandHandler : IRequestHandler<UpdateShipmentStatusCommand, Result>
{
    private readonly IShippingDbContext _dbContext;

    public UpdateShipmentStatusCommandHandler(IShippingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateShipmentStatusCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _dbContext.Shipments
            .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

        if (shipment is null)
        {
            return Error.NotFound("Shipment.NotFound", $"Shipment with ID '{request.ShipmentId}' was not found.");
        }

        switch (request.NewStatus)
        {
            case ShipmentStatus.Dispatched:
                shipment.MarkAsDispatched();
                break;
            case ShipmentStatus.InTransit:
                shipment.MarkAsInTransit();
                break;
            case ShipmentStatus.Delivered:
                shipment.MarkAsDelivered();
                break;
            case ShipmentStatus.Failed:
                shipment.MarkAsFailed(request.FailureReason ?? "Carrier delivery failed.");
                break;
            default:
                return Error.Validation("Shipment.InvalidStatus", $"Status '{request.NewStatus}' cannot be applied directly.");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
