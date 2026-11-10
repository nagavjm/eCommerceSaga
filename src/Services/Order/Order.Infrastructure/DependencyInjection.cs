using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Common.Interfaces;
using Order.Infrastructure.Persistence;

namespace Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=OrderDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName)));

        services.AddScoped<IOrderDbContext>(sp => sp.GetRequiredService<OrderDbContext>());

        return services;
    }
}
