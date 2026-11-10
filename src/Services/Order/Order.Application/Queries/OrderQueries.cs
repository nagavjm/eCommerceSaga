using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order.Application.Common.Interfaces;
using Order.Application.DTOs;

namespace Order.Application.Queries;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<OrderResponseDto>>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderResponseDto>>
{
    private readonly IOrderDbContext _dbContext;

    public GetOrderByIdQueryHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<OrderResponseDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Error.NotFound("Order.NotFound", $"Order with ID '{request.OrderId}' was not found.");
        }

        var addressDto = new AddressDto(
            order.ShippingAddress.Street,
            order.ShippingAddress.City,
            order.ShippingAddress.State,
            order.ShippingAddress.Country,
            order.ShippingAddress.ZipCode);

        var itemDtos = order.Items.Select(i => new OrderItemResponseDto(
            i.Id,
            i.ProductId,
            i.ProductName,
            i.UnitPrice,
            i.Quantity,
            i.TotalPrice)).ToList();

        return new OrderResponseDto(
            order.Id,
            order.CustomerId,
            order.CustomerEmail,
            order.Status.ToString(),
            addressDto,
            order.TotalAmount,
            order.CorrelationId,
            order.CancellationReason,
            itemDtos,
            order.CreatedAtUtc);
    }
}

public sealed record GetCustomerOrdersQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<OrderResponseDto>>>;

public class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, Result<IReadOnlyList<OrderResponseDto>>>
{
    private readonly IOrderDbContext _dbContext;

    public GetCustomerOrdersQueryHandler(IOrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<OrderResponseDto>>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        IReadOnlyList<OrderResponseDto> response = orders.Select(order =>
        {
            var addressDto = new AddressDto(
                order.ShippingAddress.Street,
                order.ShippingAddress.City,
                order.ShippingAddress.State,
                order.ShippingAddress.Country,
                order.ShippingAddress.ZipCode);

            var itemDtos = order.Items.Select(i => new OrderItemResponseDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Quantity,
                i.TotalPrice)).ToList();

            return new OrderResponseDto(
                order.Id,
                order.CustomerId,
                order.CustomerEmail,
                order.Status.ToString(),
                addressDto,
                order.TotalAmount,
                order.CorrelationId,
                order.CancellationReason,
                itemDtos,
                order.CreatedAtUtc);
        }).ToList();

        return Result<IReadOnlyList<OrderResponseDto>>.Success(response);
    }
}
