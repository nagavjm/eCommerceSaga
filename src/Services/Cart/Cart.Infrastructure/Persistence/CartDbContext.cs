using Cart.Application.Common.Interfaces;
using Cart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cart.Infrastructure.Persistence;

public class CartDbContext : DbContext, ICartDbContext
{
    public CartDbContext(DbContextOptions<CartDbContext> options) : base(options)
    {
    }

    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ShoppingCart>(builder =>
        {
            builder.ToTable("ShoppingCarts");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedNever();

            builder.Property(c => c.CustomerId)
                .IsRequired();

            builder.HasIndex(c => c.CustomerId)
                .IsUnique();

            builder.HasMany(c => c.Items)
                .WithOne()
                .HasForeignKey(i => i.ShoppingCartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(c => c.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<CartItem>(builder =>
        {
            builder.ToTable("CartItems");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).ValueGeneratedNever();

            builder.Property(i => i.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.Property(i => i.ImageUrl)
                .HasMaxLength(500);

            builder.Ignore(i => i.TotalPrice);
        });
    }
}
