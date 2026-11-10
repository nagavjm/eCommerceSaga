using BuildingBlocks.Common.Results;
using FluentValidation;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs;
using Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Commands;

public sealed record ReserveStockCommand(
    Guid OrderId,
    IReadOnlyList<StockReservationItemDto> Items) : IRequest<Result<StockReservationResultDto>>;

public class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
{
    public ReserveStockCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Reservation items cannot be empty.");
    }
}

public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, Result<StockReservationResultDto>>
{
    private readonly IInventoryDbContext _dbContext;

    public ReserveStockCommandHandler(IInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<StockReservationResultDto>> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();

        var inventoryItems = await _dbContext.InventoryItems
            .Where(i => productIds.Contains(i.ProductId))
            .ToListAsync(cancellationToken);

        // Pre-validate all items have sufficient stock
        foreach (var requestedItem in request.Items)
        {
            var inventory = inventoryItems.FirstOrDefault(i => i.ProductId == requestedItem.ProductId);
            if (inventory is null)
            {
                return new StockReservationResultDto(
                    false,
                    $"Product ID '{requestedItem.ProductId}' does not exist in inventory.");
            }

            if (inventory.AvailableStock < requestedItem.Quantity)
            {
                return new StockReservationResultDto(
                    false,
                    $"Insufficient stock for Product ID '{requestedItem.ProductId}'. Available: {inventory.AvailableStock}, Requested: {requestedItem.Quantity}");
            }
        }

        // Perform atomic reservations
        foreach (var requestedItem in request.Items)
        {
            var inventory = inventoryItems.First(i => i.ProductId == requestedItem.ProductId);
            inventory.Reserve(requestedItem.Quantity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new StockReservationResultDto(true);
    }
}

public sealed record ReleaseStockCommand(
    Guid OrderId,
    IReadOnlyList<StockReservationItemDto> Items) : IRequest<Result>;

public class ReleaseStockCommandHandler : IRequestHandler<ReleaseStockCommand, Result>
{
    private readonly IInventoryDbContext _dbContext;

    public ReleaseStockCommandHandler(IInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();

        var inventoryItems = await _dbContext.InventoryItems
            .Where(i => productIds.Contains(i.ProductId))
            .ToListAsync(cancellationToken);

        foreach (var requestedItem in request.Items)
        {
            var inventory = inventoryItems.FirstOrDefault(i => i.ProductId == requestedItem.ProductId);
            inventory?.Release(requestedItem.Quantity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed record CommitStockDeductionCommand(
    Guid OrderId,
    IReadOnlyList<StockReservationItemDto> Items) : IRequest<Result>;

public class CommitStockDeductionCommandHandler : IRequestHandler<CommitStockDeductionCommand, Result>
{
    private readonly IInventoryDbContext _dbContext;

    public CommitStockDeductionCommandHandler(IInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(CommitStockDeductionCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();

        var inventoryItems = await _dbContext.InventoryItems
            .Where(i => productIds.Contains(i.ProductId))
            .ToListAsync(cancellationToken);

        foreach (var requestedItem in request.Items)
        {
            var inventory = inventoryItems.FirstOrDefault(i => i.ProductId == requestedItem.ProductId);
            inventory?.CommitDeduction(requestedItem.Quantity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed record RestockCommand(
    Guid ProductId,
    int Quantity) : IRequest<Result<InventoryDto>>;

public class RestockCommandHandler : IRequestHandler<RestockCommand, Result<InventoryDto>>
{
    private readonly IInventoryDbContext _dbContext;

    public RestockCommandHandler(IInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<InventoryDto>> Handle(RestockCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _dbContext.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId, cancellationToken);

        if (inventory is null)
        {
            inventory = InventoryItem.Create(request.ProductId, request.Quantity);
            _dbContext.InventoryItems.Add(inventory);
        }
        else
        {
            inventory.Restock(request.Quantity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new InventoryDto(
            inventory.Id,
            inventory.ProductId,
            inventory.AvailableStock,
            inventory.ReservedStock,
            inventory.TotalStock,
            inventory.LastUpdatedUtc);
    }
}
