using Microsoft.EntityFrameworkCore;
using ProductDomain = Product.Domain.Entities;

namespace Product.Application.Common.Interfaces;

public interface IProductDbContext
{
    DbSet<ProductDomain.Product> Products { get; }
    DbSet<ProductDomain.Category> Categories { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
