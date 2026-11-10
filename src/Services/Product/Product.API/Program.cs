using BuildingBlocks.Observability;
using BuildingBlocks.Observability.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Product.Application;
using Product.Infrastructure;
using Product.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Product Service API", Version = "v1" });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddObservability(builder.Configuration, "Product.API");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1"));
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    try
    {
        await dbContext.Database.EnsureCreatedAsync();
        await ProductDataSeeder.SeedAsync(dbContext);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not initialize or seed ProductDb on startup: {Message}", ex.Message);
    }
}

app.UseObservability();
app.MapControllers();
app.MapCustomHealthChecks();

app.Run();
