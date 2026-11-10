using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Application.Common.Interfaces;
using SagaOrchestrator.Infrastructure.Persistence;

namespace SagaOrchestrator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SagaDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=SagaDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<SagaDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(SagaDbContext).Assembly.FullName)));

        services.AddScoped<ISagaDbContext>(sp => sp.GetRequiredService<SagaDbContext>());

        return services;
    }
}
