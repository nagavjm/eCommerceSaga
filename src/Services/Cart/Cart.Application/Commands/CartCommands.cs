using BuildingBlocks.Common.Results;
using Cart.Application.Common.Interfaces;
using Cart.Application.DTOs;
using Cart.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cart.Application.Commands;

public sealed record AddItemToCartCommand(
    Guid CustomerId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    string ImageUrl) : IRequest<Result<CartDto>>;

public class AddItemToCartCommandValidator : AbstractValidator<AddItemToCartCommand>
{
    public AddItemToCartCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty();
        RuleFor(x => x.UnitPrice).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class AddItemToCartCommandHandler : IRequestHandler<AddItemToCartCommand, Result<CartDto>>
{
    private readonly ICartDbContext _dbContext;

    public AddItemToCartCommandHandler(ICartDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CartDto>> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (cart is null)
        {
            cart = ShoppingCart.Create(request.CustomerId);
            _dbContext.ShoppingCarts.Add(cart);
        }

        var item = cart.AddItem(
            request.ProductId,
            request.ProductName,
            request.UnitPrice,
            request.Quantity,
            request.ImageUrl);

        if (!_dbContext.CartItems.Local.Contains(item))
        {
            _dbContext.CartItems.Add(item);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(cart);
    }

    private static CartDto MapToDto(ShoppingCart cart)
    {
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

public sealed record UpdateCartItemQuantityCommand(
    Guid CustomerId,
    Guid ProductId,
    int Quantity) : IRequest<Result<CartDto>>;

public class UpdateCartItemQuantityCommandHandler : IRequestHandler<UpdateCartItemQuantityCommand, Result<CartDto>>
{
    private readonly ICartDbContext _dbContext;

    public UpdateCartItemQuantityCommandHandler(ICartDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CartDto>> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (cart is null)
        {
            return Error.NotFound("Cart.NotFound", "Shopping cart not found.");
        }

        cart.UpdateItemQuantity(request.ProductId, request.Quantity);
        await _dbContext.SaveChangesAsync(cancellationToken);

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

public sealed record RemoveCartItemCommand(Guid CustomerId, Guid ProductId) : IRequest<Result<CartDto>>;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, Result<CartDto>>
{
    private readonly ICartDbContext _dbContext;

    public RemoveCartItemCommandHandler(ICartDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CartDto>> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (cart is null)
        {
            return Error.NotFound("Cart.NotFound", "Shopping cart not found.");
        }

        cart.RemoveItem(request.ProductId);
        await _dbContext.SaveChangesAsync(cancellationToken);

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

public sealed record ClearCartCommand(Guid CustomerId) : IRequest<Result>;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Result>
{
    private readonly ICartDbContext _dbContext;

    public ClearCartCommandHandler(ICartDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (cart is not null)
        {
            cart.Clear();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
