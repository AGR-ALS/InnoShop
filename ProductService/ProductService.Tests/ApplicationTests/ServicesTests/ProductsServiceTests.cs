using Moq;
using ProductService.Application.Abstractions.Files;
using ProductService.Application.Abstractions.Files.ImageUploading;
using ProductService.Application.Abstractions.Repositories;
using ProductService.Domain.Filters;
using ProductService.Domain.Models;
using Xunit;

namespace ProductService.Tests.ApplicationTests.ServicesTests;

public class ProductsServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IImageUploadingService> _imageUploadingServiceMock;
    private readonly Mock<IFileDeletingService> _fileDeletingServiceMock;
    private readonly Application.Services.ProductService _service;

    public ProductsServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _imageUploadingServiceMock = new Mock<IImageUploadingService>();
        _fileDeletingServiceMock = new Mock<IFileDeletingService>();

        _service = new Application.Services.ProductService(
            _repositoryMock.Object,
            _imageUploadingServiceMock.Object,
            _fileDeletingServiceMock.Object
        );
    }

    [Fact]
    public async Task GetAllProductsAsync_CallsRepositoryAndReturnsProducts()
    {
        
        var filter = new ProductFilter
        {
            Name = "Test",
            IsAvailable = true
        };

        var expectedProducts = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                Price = 100,
                IsAvailable = true
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 2",
                Price = 200,
                IsAvailable = true
            }
        };

        _repositoryMock
            .Setup(r => r.GetAllProductsAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);

        
        var result = await _service.GetAllProductsAsync(filter, CancellationToken.None);

        
        Assert.Equal(expectedProducts, result);
        _repositoryMock.Verify(r =>
                r.GetAllProductsAsync(filter, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_CallsRepositoryAndReturnsProduct()
    {
        
        var productId = Guid.NewGuid();
        var expectedProduct = new Product
        {
            Id = productId,
            Name = "Test Product",
            Price = 150.50m,
            IsAvailable = true,
            ProductImages = new List<string>()
        };

        _repositoryMock
            .Setup(r => r.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);

        
        var result = await _service.GetProductByIdAsync(productId, CancellationToken.None);

        
        Assert.Equal(expectedProduct, result);
        _repositoryMock.Verify(r =>
                r.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_CallsRepositoryAndReturnsProductId()
    {
        
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "New Product",
            Price = 99.99m,
            IsAvailable = true
        };

        var expectedProductId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.CreateProductAsync(product, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProductId);

        
        var result = await _service.CreateProductAsync(product, CancellationToken.None);

        
        Assert.Equal(expectedProductId, result);
        _repositoryMock.Verify(r =>
                r.CreateProductAsync(product, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_CallsRepository()
    {
        
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Updated Product",
            Price = 199.99m,
            IsAvailable = false
        };

        
        await _service.UpdateProductAsync(product, CancellationToken.None);

        
        _repositoryMock.Verify(r =>
                r.UpdateProductAsync(product, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_DeletesProductAndImages()
    {
        
        var productId = Guid.NewGuid();
        var imagesToDelete = new List<string> { "image1.jpg", "image2.jpg" };

        var product = new Product
        {
            Id = productId,
            Name = "Product to Delete",
            Price = 50.00m,
            IsAvailable = true,
            ProductImages = imagesToDelete
        };

        _repositoryMock
            .Setup(r => r.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        
        await _service.DeleteProductAsync(productId, CancellationToken.None);

        
        _repositoryMock.Verify(r =>
                r.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(r =>
                r.DeleteProductAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);

        _fileDeletingServiceMock.Verify(f =>
                f.DeleteFilesAsync(imagesToDelete, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProductImagesAsync_UpdatesImagesAndDeletesOldOnes()
    {
        
        var productId = Guid.NewGuid();
        var oldImages = new List<string> { "old1.jpg", "old2.jpg" };
        var newImages = new List<string> { "new1.jpg", "new2.jpg" };

        var product = new Product
        {
            Id = productId,
            Name = "Product",
            Price = 100.00m,
            IsAvailable = true,
            ProductImages = oldImages
        };

        _repositoryMock
            .Setup(r => r.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        
        await _service.UpdateProductImagesAsync(productId, newImages, CancellationToken.None);

        
        _repositoryMock.Verify(r =>
                r.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(r =>
                r.UpdateProductImagesAsync(productId, newImages, It.IsAny<CancellationToken>()),
            Times.Once);

        _fileDeletingServiceMock.Verify(f =>
                f.DeleteFilesAsync(oldImages, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetProductOwnerActiveAsync_CallsRepository()
    {
        
        var userId = Guid.NewGuid();
        var isOwnerActive = false;

        
        await _service.SetProductOwnerActiveAsync(userId, isOwnerActive, CancellationToken.None);

        
        _repositoryMock.Verify(r =>
                r.SetProductOwnerActiveAsync(userId, isOwnerActive, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProductImagesAsync_WhenNoOldImages_DoesNotCallFileDeletingService()
    {
        
        var productId = Guid.NewGuid();
        var newImages = new List<string> { "new1.jpg" };

        var product = new Product
        {
            Id = productId,
            Name = "Product",
            Price = 100.00m,
            IsAvailable = true,
            ProductImages = new List<string>() 
        };

        _repositoryMock
            .Setup(r => r.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        
        await _service.UpdateProductImagesAsync(productId, newImages, CancellationToken.None);

        
        _fileDeletingServiceMock.Verify(f =>
                f.DeleteFilesAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()),
            Times.Never); 
    }
}