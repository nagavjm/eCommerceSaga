using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shipping.Application.Common.Interfaces;
using Shipping.Application.DTOs;
using Shipping.Domain.Entities;

namespace Shipping.Application.Queries;

public sealed record GetShipmentByOrderIdQuery(Guid OrderId) : IRequest<Result<ShipmentDto>>;

public class GetShipmentByOrderIdQueryHandler : IRequestHandler<GetShipmentByOrderIdQuery, Result<ShipmentDto>>
{
    private readonly IShippingDbContext _dbContext;

    public GetShipmentByOrderIdQueryHandler(IShippingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ShipmentDto>> Handle(GetShipmentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var shipment = await _dbContext.Shipments
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.OrderId == request.OrderId, cancellationToken);

        if (shipment is null)
        {
            return Error.NotFound("Shipment.NotFound", $"Shipment for Order ID '{request.OrderId}' was not found.");
        }

        return MapToDto(shipment);
    }

    private static ShipmentDto MapToDto(Shipment shipment)
    {
        var addressDto = new AddressDto(
            shipment.DeliveryAddress.Street,
            shipment.DeliveryAddress.City,
            shipment.DeliveryAddress.State,
            shipment.DeliveryAddress.Country,
            shipment.DeliveryAddress.ZipCode);

        var itemDtos = shipment.Items.Select(i => new ShipmentItemDto(
            i.ProductId,
            i.ProductName,
            i.Quantity)).ToList();

        return new ShipmentDto(
            shipment.Id,
            shipment.OrderId,
            shipment.CustomerId,
            shipment.TrackingNumber,
            shipment.Status.ToString(),
            addressDto,
            shipment.Carrier,
            shipment.EstimatedDeliveryUtc,
            shipment.DeliveredAtUtc,
            shipment.FailureReason,
            itemDtos,
            shipment.CreatedAtUtc);
    }
}

public sealed record GetShipmentByTrackingNumberQuery(string TrackingNumber) : IRequest<Result<ShipmentDto>>;

public class GetShipmentByTrackingNumberQueryHandler : IRequestHandler<GetShipmentByTrackingNumberQuery, Result<ShipmentDto>>
{
    private readonly IShippingDbContext _dbContext;

    public GetShipmentByTrackingNumberQueryHandler(IShippingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ShipmentDto>> Handle(GetShipmentByTrackingNumberQuery request, CancellationToken cancellationToken)
    {
        var shipment = await _dbContext.Shipments
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.TrackingNumber == request.TrackingNumber.Trim().ToUpperInvariant(), cancellationToken);

        if (shipment is null)
        {
            return Error.NotFound("Shipment.NotFound", $"Shipment with Tracking Number '{request.TrackingNumber}' was not found.");
        }

        var addressDto = new AddressDto(
            shipment.DeliveryAddress.Street,
            shipment.DeliveryAddress.City,
            shipment.DeliveryAddress.State,
            shipment.DeliveryAddress.Country,
            shipment.DeliveryAddress.ZipCode);

        var itemDtos = shipment.Items.Select(i => new ShipmentItemDto(
            i.ProductId,
            i.ProductName,
            i.Quantity)).ToList();

        return new ShipmentDto(
            shipment.Id,
            shipment.OrderId,
            shipment.CustomerId,
            shipment.TrackingNumber,
            shipment.Status.ToString(),
            addressDto,
            shipment.Carrier,
            shipment.EstimatedDeliveryUtc,
            shipment.DeliveredAtUtc,
            shipment.FailureReason,
            itemDtos,
            shipment.CreatedAtUtc);
    }
}
