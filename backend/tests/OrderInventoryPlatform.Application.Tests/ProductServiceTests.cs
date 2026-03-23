using OrderInventoryPlatform.Application.Interfaces;
using OrderInventoryPlatform.Application.Services;
using OrderInventoryPlatform.Domain.Catalog;

namespace OrderInventoryPlatform.Application.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task GetAllAsync_MapsProductsToDtos()
    {
        var repository = new InMemoryProductRepository
        {
            Products =
            [
                Product.Create("KB-001", "Keyboard", 49.99m, 5),
                Product.Create("MS-001", "Mouse", 19.99m, 10)
            ]
        };

        var service = new ProductService(repository);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p is { Name: "Keyboard", Sku: "KB-001", Price: 49.99m, ReorderThreshold: 5 });
        Assert.Contains(result, p => p is { Name: "Mouse", Sku: "MS-001", Price: 19.99m, ReorderThreshold: 10 });
    }

    private sealed class InMemoryProductRepository : IProductRepository
    {
        public IReadOnlyCollection<Product> Products { get; init; } = [];

        public Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Products);

        public Task<IReadOnlyDictionary<Guid, Product>> GetByIdsAsync(
            IReadOnlyCollection<Guid> productIds,
            CancellationToken cancellationToken = default)
        {
            var map = Products.Where(p => productIds.Contains(p.Id)).ToDictionary(p => p.Id);
            return Task.FromResult<IReadOnlyDictionary<Guid, Product>>(map);
        }
    }
}
