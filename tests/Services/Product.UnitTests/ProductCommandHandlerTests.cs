using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Product.Application.Commands;
using Product.Application.Common.Interfaces;
using Product.Domain.Entities;
using Product.Infrastructure.Persistence;

namespace Product.UnitTests;

/// <summary>
/// Creates an isolated in-memory ProductDbContext per test.
/// </summary>
public static class ProductDbContextFactory
{
    public static ProductDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique per test
            .Options;
        return new ProductDbContext(options);
    }
}

public class CreateProductCommandHandlerTests
{
    private static readonly Guid _categoryId = Guid.NewGuid();

    private static ProductDbContext SeedWithCategory()
    {
        var ctx = ProductDbContextFactory.Create();
        var category = Category.Create("Electronics", "Electronic devices");

        // override Id via reflection trick: use the EF Add + set Id manually
        // Category.Create gives a new GUID; set a known one for tests
        // Instead, capture the Id from Create()
        ctx.Categories.Add(category);
        ctx.SaveChanges();
        return ctx;
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProduct()
    {
        // Arrange
        await using var ctx = ProductDbContextFactory.Create();
        var category = Category.Create("Electronics", "Electronic devices");
        ctx.Categories.Add(category);
        await ctx.SaveChangesAsync();

        var handler = new CreateProductCommandHandler(ctx);
        var command = new CreateProductCommand(
            "Laptop Pro",
            "A powerful laptop",
            "LAP-PRO-001",
            1299.99m,
            category.Id,
            "https://img.url/laptop.jpg");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Laptop Pro");
        result.Value.SKU.Should().Be("LAP-PRO-001");
        result.Value.Price.Should().Be(1299.99m);
        result.Value.CategoryId.Should().Be(category.Id);

        // Verify persisted in DB
        var persisted = await ctx.Products.FirstOrDefaultAsync();
        persisted.Should().NotBeNull();
        persisted!.SKU.Should().Be("LAP-PRO-001");
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ShouldReturnNotFoundError()
    {
        // Arrange
        await using var ctx = ProductDbContextFactory.Create();
        var handler = new CreateProductCommandHandler(ctx);
        var command = new CreateProductCommand(
            "Laptop",
            "A laptop",
            "LAP-001",
            999m,
            Guid.NewGuid(), // category doesn't exist
            "url");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Category.NotFound");
    }

    [Fact]
    public async Task Handle_DuplicateSKU_ShouldReturnConflictError()
    {
        // Arrange
        await using var ctx = ProductDbContextFactory.Create();
        var category = Category.Create("Electronics", "Electronic devices");
        ctx.Categories.Add(category);
        var existingProduct = Product.Domain.Entities.Product.Create(
            "Old Laptop", "Old Desc", "LAP-DUP-001", 500m, category.Id, "url");
        ctx.Products.Add(existingProduct);
        await ctx.SaveChangesAsync();

        var handler = new CreateProductCommandHandler(ctx);
        var command = new CreateProductCommand(
            "New Laptop",
            "New Desc",
            "LAP-DUP-001", // same SKU (case-insensitive)
            800m,
            category.Id,
            "url");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Product.DuplicateSKU");
    }

    [Fact]
    public async Task Handle_SKUIsCaseInsensitive_DuplicateDetected()
    {
        // Arrange
        await using var ctx = ProductDbContextFactory.Create();
        var category = Category.Create("Electronics", "Electronic devices");
        ctx.Categories.Add(category);
        var existingProduct = Product.Domain.Entities.Product.Create(
            "Laptop", "Desc", "laptop-sku", 500m, category.Id, "url");
        ctx.Products.Add(existingProduct);
        await ctx.SaveChangesAsync();

        var handler = new CreateProductCommandHandler(ctx);
        var command = new CreateProductCommand(
            "New Laptop", "New Desc",
            "LAPTOP-SKU", // uppercase version of same SKU
            800m, category.Id, "url");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Product.DuplicateSKU");
    }
}

public class DeleteProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProduct_ShouldDeactivateProduct()
    {
        // Arrange
        await using var ctx = ProductDbContextFactory.Create();
        var category = Category.Create("Electronics", "Electronic devices");
        ctx.Categories.Add(category);
        var product = Product.Domain.Entities.Product.Create(
            "Laptop", "Desc", "SKU-DEL-001", 999m, category.Id, "url");
        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();

        var handler = new DeleteProductCommandHandler(ctx);

        // Act
        var result = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await ctx.Products.FindAsync(product.Id);
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NonExistentProduct_ShouldReturnNotFoundError()
    {
        // Arrange
        await using var ctx = ProductDbContextFactory.Create();
        var handler = new DeleteProductCommandHandler(ctx);

        // Act
        var result = await handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Product.NotFound");
    }
}
