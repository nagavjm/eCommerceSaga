using System.Text.Json;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging.Outbox;
using FluentValidation;
using MediatR;
using Shipping.Application.Common.Interfaces;
using Shipping.Application.DTOs;
using Shipping.Domain.Entities;
using Shipping.Domain.ValueObjects;

namespace Shipping.Application.Commands;

public sealed record CreateShipmentCommand(
    Guid OrderId,
    Guid CustomerId,
    AddressDto DeliveryAddress,
    string? Carrier,
    IReadOnlyList<ShipmentItemDto> Items,
    Guid CorrelationId) : IRequest<Result<ShipmentDto>>;

public class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required.");
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("CustomerId is required.");
        RuleFor(x => x.DeliveryAddress).NotNull().WithMessage("DeliveryAddress is required.");
        RuleFor(x => x.DeliveryAddress.Street).NotEmpty().WithMessage("Street is required.");
        RuleFor(x => x.DeliveryAddress.City).NotEmpty().WithMessage("City is required.");
        RuleFor(x => x.DeliveryAddress.State).NotEmpty().WithMessage("State is required.");
        RuleFor(x => x.DeliveryAddress.Country).NotEmpty().WithMessage("Country is required.");
        RuleFor(x => x.DeliveryAddress.ZipCode).NotEmpty().WithMessage("ZipCode is required.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required for shipment.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.ProductName).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, Result<ShipmentDto>>
{
    private readonly IShippingDbContext _dbContext;

    public CreateShipmentCommandHandler(IShippingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ShipmentDto>> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
    {
        var address = new Address(
            request.DeliveryAddress.Street,
            request.DeliveryAddress.City,
            request.DeliveryAddress.State,
            request.DeliveryAddress.Country,
            request.DeliveryAddress.ZipCode);

        var correlationId = request.CorrelationId == Guid.Empty ? Guid.NewGuid() : request.CorrelationId;
        var carrier = string.IsNullOrWhiteSpace(request.Carrier) ? "DHL Express" : request.Carrier;

        var shipment = Shipment.Create(
            request.OrderId,
            request.CustomerId,
            address,
            carrier,
            correlationId);

        foreach (var item in request.Items)
        {
            shipment.AddItem(item.ProductId, item.ProductName, item.Quantity);
        }

        shipment.FinalizeCreation();

        _dbContext.Shipments.Add(shipment);

        // Transactional Outbox Pattern
        var integrationEvent = new ShipmentCreatedIntegrationEvent(
            shipment.OrderId,
            shipment.Id,
            shipment.TrackingNumber,
            shipment.Carrier,
            shipment.CorrelationId);

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(ShipmentCreatedIntegrationEvent).AssemblyQualifiedName ?? typeof(ShipmentCreatedIntegrationEvent).Name,
            Payload = JsonSerializer.Serialize(integrationEvent),
            ProcessedOnUtc = null,
            RetryCount = 0
        };

        _dbContext.OutboxMessages.Add(outboxMessage);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var itemDtos = shipment.Items.Select(i => new ShipmentItemDto(
            i.ProductId,
            i.ProductName,
            i.Quantity)).ToList();

        var dto = new ShipmentDto(
            shipment.Id,
            shipment.OrderId,
            shipment.CustomerId,
            shipment.TrackingNumber,
            shipment.Status.ToString(),
            request.DeliveryAddress,
            shipment.Carrier,
            shipment.EstimatedDeliveryUtc,
            shipment.DeliveredAtUtc,
            shipment.FailureReason,
            itemDtos,
            shipment.CreatedAtUtc);

        return dto;
    }
}
