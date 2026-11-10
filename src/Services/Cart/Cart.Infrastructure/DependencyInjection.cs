using Cart.Application.Common.Interfaces;
using Cart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CartDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=CartDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<CartDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(CartDbContext).Assembly.FullName)));

        services.AddScoped<ICartDbContext>(provider => provider.GetRequiredService<CartDbContext>());

        return services;
    }
}
