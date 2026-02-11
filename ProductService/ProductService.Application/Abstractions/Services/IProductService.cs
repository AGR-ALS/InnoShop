using ProductService.Domain.Filters;
using ProductService.Domain.Models;

namespace ProductService.Application.Abstractions.Services;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync(ProductFilter productFilter, CancellationToken cancellationToken);
    Task<Product> GetProductByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> CreateProductAsync(Product product, CancellationToken cancellationToken);
    Task UpdateProductAsync(Product product, CancellationToken cancellationToken);
    Task DeleteProductAsync(Guid id, CancellationToken cancellationToken);
    Task UpdateProductImagesAsync(Guid id, List<string> productImages, CancellationToken cancellationToken);
    Task SetProductOwnerActiveAsync(Guid userId, bool isOwnerActive, CancellationToken cancellationToken);
}