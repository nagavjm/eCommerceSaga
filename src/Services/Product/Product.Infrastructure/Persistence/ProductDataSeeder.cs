using Microsoft.EntityFrameworkCore;
using ProductDomain = Product.Domain.Entities;

namespace Product.Infrastructure.Persistence;

public static class ProductDataSeeder
{
    public static readonly Guid ElectronicsCategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AccessoriesCategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid Product1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid Product2Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid Product3Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    public static async Task SeedAsync(ProductDbContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var electronics = ProductDomain.Category.Create("Electronics", "Gadgets, smartphones, and laptops");
        var accessories = ProductDomain.Category.Create("Accessories", "Cables, chargers, and cases");

        typeof(ProductDomain.Category).GetProperty("Id")!.SetValue(electronics, ElectronicsCategoryId);
        typeof(ProductDomain.Category).GetProperty("Id")!.SetValue(accessories, AccessoriesCategoryId);

        await context.Categories.AddRangeAsync(electronics, accessories);

        var product1 = ProductDomain.Product.Create(
            "UltraBook Pro 15",
            "High performance 15-inch laptop with 32GB RAM and 1TB NVMe SSD",
            "UB-PRO-15",
            1499.99m,
            ElectronicsCategoryId,
            "https://images.unsplash.com/photo-1517336714731-489689fd1ca8");

        var product2 = ProductDomain.Product.Create(
            "Noise Cancelling Headphones",
            "Wireless over-ear headphones with 40-hour battery life",
            "NC-HEAD-01",
            249.99m,
            AccessoriesCategoryId,
            "https://images.unsplash.com/photo-1505740420928-5e560c06d30e");

        var product3 = ProductDomain.Product.Create(
            "Smartwatch Series X",
            "Fitness and health tracking smartwatch with AMOLED display",
            "SW-SERIES-X",
            399.99m,
            ElectronicsCategoryId,
            "https://images.unsplash.com/photo-1523275335684-37898b6baf30");

        typeof(ProductDomain.Product).GetProperty("Id")!.SetValue(product1, Product1Id);
        typeof(ProductDomain.Product).GetProperty("Id")!.SetValue(product2, Product2Id);
        typeof(ProductDomain.Product).GetProperty("Id")!.SetValue(product3, Product3Id);

        await context.Products.AddRangeAsync(product1, product2, product3);
        await context.SaveChangesAsync();
    }
}
