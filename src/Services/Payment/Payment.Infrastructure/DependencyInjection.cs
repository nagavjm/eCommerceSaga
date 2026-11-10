using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Common.Interfaces;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Services;

namespace Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PaymentDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=PaymentDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(PaymentDbContext).Assembly.FullName)));

        services.AddScoped<IPaymentDbContext>(sp => sp.GetRequiredService<PaymentDbContext>());
        services.AddScoped<IPaymentGateway, SandboxPaymentGateway>();

        return services;
    }
}
