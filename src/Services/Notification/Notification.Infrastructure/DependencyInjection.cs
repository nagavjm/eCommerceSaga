using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Common.Interfaces;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Services;

namespace Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NotificationDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=NotificationDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName)));

        services.AddScoped<INotificationDbContext>(sp => sp.GetRequiredService<NotificationDbContext>());
        services.AddScoped<IEmailSender, EmailSender>();

        return services;
    }
}
