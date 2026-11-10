using BuildingBlocks.Common.Results;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Product.Application.Common.Interfaces;
using Product.Application.DTOs;
using ProductDomain = Product.Domain.Entities;

namespace Product.Application.Commands;

public sealed record CreateProductCommand(
    string Name,
    string Description,
    string SKU,
    decimal Price,
    Guid CategoryId,
    string ImageUrl) : IRequest<Result<ProductDto>>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SKU).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IProductDbContext _dbContext;

    public CreateProductCommandHandler(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _dbContext.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Error.NotFound("Category.NotFound", "The specified category does not exist.");
        }

        var skuExists = await _dbContext.Products
            .AnyAsync(p => p.SKU == request.SKU.ToUpperInvariant(), cancellationToken);

        if (skuExists)
        {
            return Error.Conflict("Product.DuplicateSKU", "A product with this SKU already exists.");
        }

        var product = ProductDomain.Product.Create(
            request.Name,
            request.Description,
            request.SKU,
            request.Price,
            request.CategoryId,
            request.ImageUrl);

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var category = await _dbContext.Categories.FindAsync([product.CategoryId], cancellationToken);

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.SKU,
            product.Price,
            product.CategoryId,
            category?.Name,
            product.ImageUrl,
            product.IsActive);
    }
}

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    Guid CategoryId,
    string ImageUrl) : IRequest<Result<ProductDto>>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IProductDbContext _dbContext;

    public UpdateProductCommandHandler(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
        {
            return Error.NotFound("Product.NotFound", $"Product with ID '{request.Id}' was not found.");
        }

        product.UpdateDetails(
            request.Name,
            request.Description,
            request.Price,
            request.CategoryId,
            request.ImageUrl);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.SKU,
            product.Price,
            product.CategoryId,
            product.Category?.Name,
            product.ImageUrl,
            product.IsActive);
    }
}

public sealed record DeleteProductCommand(Guid Id) : IRequest<Result>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IProductDbContext _dbContext;

    public DeleteProductCommandHandler(IProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
        {
            return Error.NotFound("Product.NotFound", $"Product with ID '{request.Id}' was not found.");
        }

        product.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
