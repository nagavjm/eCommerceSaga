using FluentAssertions;
using Product.Domain.Entities;

namespace Product.UnitTests;

public class ProductDomainTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnProduct()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var product = Product.Domain.Entities.Product.Create(
            "Test Product",
            "A great test product",
            "TEST-001",
            99.99m,
            categoryId,
            "https://example.com/image.jpg");

        // Assert
        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be("Test Product");
        product.SKU.Should().Be("TEST-001"); // uppercase
        product.Price.Should().Be(99.99m);
        product.CategoryId.Should().Be(categoryId);
        product.IsActive.Should().BeTrue();
        product.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("  test-sku  ")]
    [InlineData("test-sku")]
    public void Create_ShouldNormalizeSKUToUpperCase(string sku)
    {
        // Act
        var product = Product.Domain.Entities.Product.Create(
            "Product", "Desc", sku, 10m, Guid.NewGuid(), "url");

        // Assert
        product.SKU.Should().Be("TEST-SKU");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Create_WithInvalidPrice_ShouldThrowArgumentException(decimal price)
    {
        // Act
        var act = () => Product.Domain.Entities.Product.Create(
            "Product", "Desc", "SKU-001", price, Guid.NewGuid(), "url");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Price must be greater than zero*");
    }

    [Fact]
    public void Create_ShouldTrimWhitespaceFromStringFields()
    {
        // Act
        var product = Product.Domain.Entities.Product.Create(
            "  My Product  ",
            "  My Description  ",
            "  SKU-123  ",
            49.99m,
            Guid.NewGuid(),
            "  https://img.url  ");

        // Assert
        product.Name.Should().Be("My Product");
        product.Description.Should().Be("My Description");
        product.ImageUrl.Should().Be("https://img.url");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var product = Product.Domain.Entities.Product.Create(
            "Product", "Desc", "SKU-001", 10m, Guid.NewGuid(), "url");

        // Act
        product.Deactivate();

        // Assert
        product.IsActive.Should().BeFalse();
        product.UpdatedAtUtc.Should().NotBeNull();
        product.UpdatedAtUtc!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        // Arrange
        var product = Product.Domain.Entities.Product.Create(
            "Product", "Desc", "SKU-001", 10m, Guid.NewGuid(), "url");
        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        var product = Product.Domain.Entities.Product.Create(
            "Old Name", "Old Desc", "SKU-001", 10m, Guid.NewGuid(), "old-url");
        var newCategoryId = Guid.NewGuid();

        // Act
        product.UpdateDetails("New Name", "New Description", 25m, newCategoryId, "new-url");

        // Assert
        product.Name.Should().Be("New Name");
        product.Description.Should().Be("New Description");
        product.Price.Should().Be(25m);
        product.CategoryId.Should().Be(newCategoryId);
        product.ImageUrl.Should().Be("new-url");
        product.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void UpdateDetails_WithInvalidPrice_ShouldThrowArgumentException()
    {
        // Arrange
        var product = Product.Domain.Entities.Product.Create(
            "Product", "Desc", "SKU-001", 10m, Guid.NewGuid(), "url");

        // Act
        var act = () => product.UpdateDetails("Product", "Desc", 0m, Guid.NewGuid(), "url");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Price must be greater than zero*");
    }

    [Fact]
    public void Category_Create_ShouldReturnCategoryWithTrimmedName()
    {
        // Act
        var category = Category.Create("  Electronics  ", "  Electronic devices  ");

        // Assert
        category.Id.Should().NotBeEmpty();
        category.Name.Should().Be("Electronics");
        category.Description.Should().Be("Electronic devices");
    }
}
