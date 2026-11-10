using BuildingBlocks.Common.Results;
using Cart.Application.Common.Interfaces;
using Cart.Application.DTOs;
using Cart.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cart.Application.Queries;

public sealed record GetCartByCustomerIdQuery(Guid CustomerId) : IRequest<Result<CartDto>>;

public class GetCartByCustomerIdQueryHandler : IRequestHandler<GetCartByCustomerIdQuery, Result<CartDto>>
{
    private readonly ICartDbContext _dbContext;

    public GetCartByCustomerIdQueryHandler(ICartDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CartDto>> Handle(GetCartByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.ShoppingCarts
            .AsNoTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (cart is null)
        {
            return new CartDto(Guid.Empty, request.CustomerId, 0, []);
        }

        var items = cart.Items.Select(i => new CartItemDto(
            i.Id,
            i.ProductId,
            i.ProductName,
            i.UnitPrice,
            i.Quantity,
            i.TotalPrice,
            i.ImageUrl)).ToList();

        return new CartDto(cart.Id, cart.CustomerId, cart.TotalPrice, items);
    }
}
