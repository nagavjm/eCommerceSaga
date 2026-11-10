using Cart.Domain.Entities;
using FluentAssertions;

namespace Cart.UnitTests;

public class ShoppingCartDomainTests
{
    private static ShoppingCart CreateCart() =>
        ShoppingCart.Create(Guid.NewGuid());

    [Fact]
    public void Create_ShouldInitializeWithEmptyItems()
    {
        // Act
        var cart = CreateCart();

        // Assert
        cart.Id.Should().NotBeEmpty();
        cart.CustomerId.Should().NotBeEmpty();
        cart.Items.Should().BeEmpty();
        cart.TotalPrice.Should().Be(0m);
    }

    [Fact]
    public void AddItem_NewProduct_ShouldAddItemToCart()
    {
        // Arrange
        var cart = CreateCart();
        var productId = Guid.NewGuid();

        // Act
        cart.AddItem(productId, "Laptop", 999.99m, 1, "laptop.jpg");

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items.First().ProductId.Should().Be(productId);
        cart.Items.First().Quantity.Should().Be(1);
        cart.Items.First().UnitPrice.Should().Be(999.99m);
        cart.TotalPrice.Should().Be(999.99m);
    }

    [Fact]
    public void AddItem_SameProductTwice_ShouldMergeQuantities()
    {
        // Arrange
        var cart = CreateCart();
        var productId = Guid.NewGuid();

        // Act
        cart.AddItem(productId, "Laptop", 999.99m, 1, "laptop.jpg");
        cart.AddItem(productId, "Laptop", 999.99m, 2, "laptop.jpg");

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items.First().Quantity.Should().Be(3);
        cart.TotalPrice.Should().Be(2999.97m);
    }

    [Fact]
    public void AddItem_DifferentProducts_ShouldAddSeparateItems()
    {
        // Arrange
        var cart = CreateCart();

        // Act
        cart.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1, "laptop.jpg");
        cart.AddItem(Guid.NewGuid(), "Mouse", 29.99m, 2, "mouse.jpg");

        // Assert
        cart.Items.Should().HaveCount(2);
        cart.TotalPrice.Should().BeApproximately(1059.97m, 0.01m);
    }

    [Fact]
    public void TotalPrice_ShouldCalculateCorrectly()
    {
        // Arrange
        var cart = CreateCart();

        // Act
        cart.AddItem(Guid.NewGuid(), "Item A", 10.00m, 3, "a.jpg"); // 30.00
        cart.AddItem(Guid.NewGuid(), "Item B", 5.50m, 2, "b.jpg");  // 11.00

        // Assert
        cart.TotalPrice.Should().Be(41.00m);
    }

    [Fact]
    public void UpdateItemQuantity_ExistingProduct_ShouldUpdateQuantity()
    {
        // Arrange
        var cart = CreateCart();
        var productId = Guid.NewGuid();
        cart.AddItem(productId, "Laptop", 999.99m, 1, "laptop.jpg");

        // Act
        cart.UpdateItemQuantity(productId, 5);

        // Assert
        cart.Items.First().Quantity.Should().Be(5);
        cart.TotalPrice.Should().BeApproximately(4999.95m, 0.01m);
    }

    [Fact]
    public void UpdateItemQuantity_ToZeroOrLess_ShouldRemoveItem()
    {
        // Arrange
        var cart = CreateCart();
        var productId = Guid.NewGuid();
        cart.AddItem(productId, "Laptop", 999.99m, 1, "laptop.jpg");

        // Act
        cart.UpdateItemQuantity(productId, 0);

        // Assert
        cart.Items.Should().BeEmpty();
        cart.TotalPrice.Should().Be(0m);
    }

    [Fact]
    public void UpdateItemQuantity_NonExistentProduct_ShouldDoNothing()
    {
        // Arrange
        var cart = CreateCart();
        cart.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1, "laptop.jpg");

        // Act — update a product not in cart
        cart.UpdateItemQuantity(Guid.NewGuid(), 5);

        // Assert — original item unchanged
        cart.Items.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveItem_ExistingProduct_ShouldRemoveFromCart()
    {
        // Arrange
        var cart = CreateCart();
        var productId = Guid.NewGuid();
        cart.AddItem(productId, "Laptop", 999.99m, 1, "laptop.jpg");

        // Act
        cart.RemoveItem(productId);

        // Assert
        cart.Items.Should().BeEmpty();
        cart.TotalPrice.Should().Be(0m);
    }

    [Fact]
    public void RemoveItem_NonExistentProduct_ShouldDoNothing()
    {
        // Arrange
        var cart = CreateCart();
        cart.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1, "laptop.jpg");

        // Act
        cart.RemoveItem(Guid.NewGuid());

        // Assert — original item still there
        cart.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        // Arrange
        var cart = CreateCart();
        cart.AddItem(Guid.NewGuid(), "Laptop", 999.99m, 1, "laptop.jpg");
        cart.AddItem(Guid.NewGuid(), "Mouse", 29.99m, 2, "mouse.jpg");

        // Act
        cart.Clear();

        // Assert
        cart.Items.Should().BeEmpty();
        cart.TotalPrice.Should().Be(0m);
        cart.UpdatedAtUtc.Should().NotBeNull();
    }
}

public class CartItemDomainTests
{
    [Fact]
    public void Create_ShouldSetCorrectTotalPrice()
    {
        // Arrange
        var cartId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        // Act
        var item = CartItem.Create(cartId, productId, "Laptop", 999.99m, 3, "laptop.jpg");

        // Assert
        item.Id.Should().NotBeEmpty();
        item.ShoppingCartId.Should().Be(cartId);  // CartItem.ShoppingCartId
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be("Laptop");
        item.UnitPrice.Should().Be(999.99m);
        item.Quantity.Should().Be(3);
        item.TotalPrice.Should().BeApproximately(2999.97m, 0.01m);
    }

    [Fact]
    public void UpdateQuantity_ShouldRecalculateTotalPrice()
    {
        // Arrange
        var item = CartItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Laptop", 100m, 1, "img.jpg");

        // Act
        item.UpdateQuantity(5);

        // Assert
        item.Quantity.Should().Be(5);
        item.TotalPrice.Should().Be(500m);
    }
}
