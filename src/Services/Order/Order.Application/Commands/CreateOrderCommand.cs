using System.Text.Json;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging.Outbox;
using FluentValidation;
using MediatR;
using Order.Application.Common.Interfaces;
using Order.Application.DTOs;
using Order.Domain.ValueObjects;
using ContractOrderItemDto = BuildingBlocks.Contracts.Events.OrderItemDto;

namespace Order.Application.Commands;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    string CustomerEmail,
    AddressDto ShippingAddress,
    IReadOnlyList<CreateOrderItemDto> Items,
    Guid CorrelationId) : IRequest<Result<OrderCreatedResponseDto>>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("CustomerId is required.");
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().WithMessage("A valid CustomerEmail is required.");
        RuleFor(x => x.ShippingAddress).NotNull().WithMessage("ShippingAddress is required.");
        RuleFor(x => x.ShippingAddress.Street).NotEmpty().WithMessage("Street is required.");
        RuleFor(x => x.ShippingAddress.City).NotEmpty().WithMessage("City is required.");
        RuleFor(x => x.ShippingAddress.State).NotEmpty().WithMessage("State is required.");
        RuleFor(x => x.ShippingAddress.Country).NotEmpty().WithMessage("Country is required.");
        RuleFor(x => x.ShippingAddress.ZipCode).NotEmpty().WithMessage("ZipCode is required.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one order item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("ProductId is required.");
            item.RuleFor(i => i.ProductName).NotEmpty().WithMessage("ProductName is required.");
            item.RuleFor(i => i.UnitPrice).GreaterThan(0).WithMessage("UnitPrice must be greater than zero.");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderCreatedResponseDto>>
{
    private readonly IOrderDbContext _dbContext;

    public CreateOrderCommandHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<OrderCreatedResponseDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var address = new Address(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.Country,
            request.ShippingAddress.ZipCode);

        var correlationId = request.CorrelationId == Guid.Empty ? Guid.NewGuid() : request.CorrelationId;

        var order = Domain.Entities.Order.Create(
            request.CustomerId,
            request.CustomerEmail,
            address,
            correlationId);

        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);
        }

        order.FinalizePlacement();

        _dbContext.Orders.Add(order);

        // Transactional Outbox Pattern: Save integration event in the same transaction
        var contractItems = order.Items.Select(i => new ContractOrderItemDto(
            i.ProductId,
            i.ProductName,
            i.Quantity,
            i.UnitPrice)).ToList();

        var integrationEvent = new OrderPlacedIntegrationEvent(
            order.Id,
            order.CustomerId,
            order.CustomerEmail,
            order.ShippingAddress.ToString(),
            order.TotalAmount,
            contractItems,
            order.CorrelationId);

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(OrderPlacedIntegrationEvent).AssemblyQualifiedName ?? typeof(OrderPlacedIntegrationEvent).Name,
            Payload = JsonSerializer.Serialize(integrationEvent),
            ProcessedOnUtc = null,
            RetryCount = 0
        };

        _dbContext.OutboxMessages.Add(outboxMessage);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OrderCreatedResponseDto(
            order.Id,
            order.CorrelationId,
            order.Status.ToString(),
            order.TotalAmount);
    }
}
