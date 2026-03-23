using Microsoft.Extensions.DependencyInjection;
using OrderInventoryPlatform.Application.Interfaces;
using OrderInventoryPlatform.Application.Services;

namespace OrderInventoryPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IPlaceOrderHandler, PlaceOrderHandler>();
        services.AddScoped<IAdjustInventoryHandler, AdjustInventoryHandler>();
        return services;
    }
}
