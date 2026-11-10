using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shipping.Application.Common.Interfaces;
using Shipping.Infrastructure.Persistence;

namespace Shipping.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ShippingDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=ShippingDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<ShippingDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(ShippingDbContext).Assembly.FullName)));

        services.AddScoped<IShippingDbContext>(sp => sp.GetRequiredService<ShippingDbContext>());

        return services;
    }
}
