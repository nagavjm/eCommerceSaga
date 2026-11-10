using Inventory.Application.Common.Interfaces;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("InventoryDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=InventoryDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName)));

        services.AddScoped<IInventoryDbContext>(provider => provider.GetRequiredService<InventoryDbContext>());

        return services;
    }
}
