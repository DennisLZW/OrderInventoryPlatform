using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderInventoryPlatform.Application.Interfaces;
using OrderInventoryPlatform.Application.Queries;
using OrderInventoryPlatform.Infrastructure.Persistence;
using OrderInventoryPlatform.Infrastructure.Queries;
using OrderInventoryPlatform.Infrastructure.Persistence.Repositories;

namespace OrderInventoryPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<IOrderWriteRepository, OrderWriteRepository>();
        services.AddScoped<IInventoryWriteRepository, InventoryWriteRepository>();
        services.AddScoped<IProductQueryService, ProductQueryService>();
        services.AddScoped<IInventoryQueryService, InventoryQueryService>();
        services.AddScoped<IOrderQueryService, OrderQueryService>();

        return services;
    }
}
