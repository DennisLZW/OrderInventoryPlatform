using OrderInventoryPlatform.Application.DTOs;
using OrderInventoryPlatform.Application.Interfaces;

namespace OrderInventoryPlatform.Application.Services;

public interface IProductService
{
    Task<IReadOnlyCollection<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

public class ProductService(IProductRepository productRepository) : IProductService
{
    public async Task<IReadOnlyCollection<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);

        return products
            .Select(p => new ProductDto(p.Id, p.Sku, p.Name, p.Price, p.ReorderThreshold))
            .ToArray();
    }
}
