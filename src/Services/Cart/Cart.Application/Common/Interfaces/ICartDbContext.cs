using Cart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cart.Application.Common.Interfaces;

public interface ICartDbContext
{
    DbSet<ShoppingCart> ShoppingCarts { get; }
    DbSet<CartItem> CartItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
