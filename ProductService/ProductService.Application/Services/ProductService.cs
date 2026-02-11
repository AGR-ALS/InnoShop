using ProductService.Application.Abstractions.Files;
using ProductService.Application.Abstractions.Files.ImageUploading;
using ProductService.Application.Abstractions.Repositories;
using ProductService.Application.Abstractions.Services;
using ProductService.Domain;
using ProductService.Domain.Filters;
using ProductService.Domain.Models;

namespace ProductService.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IFileDeletingService _fileDeletingService;

    public ProductService(IProductRepository repository, IImageUploadingService imageUploadingService, IFileDeletingService fileDeletingService)
    {
        _repository = repository;
        _fileDeletingService = fileDeletingService;
    }
    public async Task<List<Product>> GetAllProductsAsync(ProductFilter productFilter, CancellationToken cancellationToken)
    {
        return await _repository.GetAllProductsAsync(productFilter, cancellationToken);
    }

    public async Task<Product> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _repository.GetProductByIdAsync(id, cancellationToken);
    }

    public async Task<Guid> CreateProductAsync(Product product, CancellationToken cancellationToken)
    {
        var productId = await _repository.CreateProductAsync(product, cancellationToken);
        return productId;
    }

    public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken)
    {
        await _repository.UpdateProductAsync(product, cancellationToken);
    }

    public async Task DeleteProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var imagesToDelete = (await GetProductByIdAsync(id, cancellationToken)).ProductImages;
        await _repository.DeleteProductAsync(id, cancellationToken);
        if(imagesToDelete.Any())
            await _fileDeletingService.DeleteFilesAsync(imagesToDelete,  cancellationToken);
    }
    

    public async Task UpdateProductImagesAsync(Guid id, List<string> productImages, CancellationToken cancellationToken)
    {
        var imagesToDelete = (await GetProductByIdAsync(id, cancellationToken)).ProductImages;
        await _repository.UpdateProductImagesAsync(id, productImages, cancellationToken);
        if(imagesToDelete.Any())
            await _fileDeletingService.DeleteFilesAsync(imagesToDelete, cancellationToken);
    }

    public async Task SetProductOwnerActiveAsync(Guid userId, bool isOwnerActive, CancellationToken cancellationToken)
    {
        await _repository.SetProductOwnerActiveAsync(userId, isOwnerActive, cancellationToken);
    }
}