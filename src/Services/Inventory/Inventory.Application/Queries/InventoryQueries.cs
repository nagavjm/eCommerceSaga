using BuildingBlocks.Common.Results;
using Inventory.Application.Common.Interfaces;
using Inventory.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Queries;

public sealed record GetStockByProductIdQuery(Guid ProductId) : IRequest<Result<InventoryDto>>;

public class GetStockByProductIdQueryHandler : IRequestHandler<GetStockByProductIdQuery, Result<InventoryDto>>
{
    private readonly IInventoryDbContext _dbContext;

    public GetStockByProductIdQueryHandler(IInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<InventoryDto>> Handle(GetStockByProductIdQuery request, CancellationToken cancellationToken)
    {
        var inventory = await _dbContext.InventoryItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId, cancellationToken);

        if (inventory is null)
        {
            return Error.NotFound("Inventory.NotFound", $"Stock record for product ID '{request.ProductId}' was not found.");
        }

        return new InventoryDto(
            inventory.Id,
            inventory.ProductId,
            inventory.AvailableStock,
            inventory.ReservedStock,
            inventory.TotalStock,
            inventory.LastUpdatedUtc);
    }
}

public sealed record GetAllStockQuery : IRequest<Result<IReadOnlyList<InventoryDto>>>;

public class GetAllStockQueryHandler : IRequestHandler<GetAllStockQuery, Result<IReadOnlyList<InventoryDto>>>
{
    private readonly IInventoryDbContext _dbContext;

    public GetAllStockQueryHandler(IInventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyList<InventoryDto>>> Handle(GetAllStockQuery request, CancellationToken cancellationToken)
    {
        var items = await _dbContext.InventoryItems
            .AsNoTracking()
            .Select(i => new InventoryDto(
                i.Id,
                i.ProductId,
                i.AvailableStock,
                i.ReservedStock,
                i.TotalStock,
                i.LastUpdatedUtc))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<InventoryDto>>(items);
    }
}
