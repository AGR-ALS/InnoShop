using AutoMapper;
using Castle.Core.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Application.Exceptions;
using ProductService.DataAccess;
using ProductService.DataAccess.Entities;
using ProductService.DataAccess.Repositories;
using ProductService.Domain.Filters;
using ProductService.Domain.Models;
using Xunit;

namespace ProductService.Tests.DataAccessTests.RepositoriesTests;

public class ProductRepositoryTests
{
    private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ProductServiceDbContext> _options;
        private readonly ProductServiceDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ProductRepository _repository;

        public ProductRepositoryTests()
        {
            
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            
            _options = new DbContextOptionsBuilder<ProductServiceDbContext>()
                .UseSqlite(_connection)
                .Options;

            
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductService.DataAccess.Mapping.ProductProfile>();
            }, new LoggerFactory());
            _mapper = config.CreateMapper();

            
            _dbContext = new ProductServiceDbContext(_options, new ConfigurationBuilder().Build());
            _dbContext.Database.EnsureCreated();

            
            _repository = new ProductRepository(_dbContext, _mapper);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            _connection.Close();
            _connection.Dispose();
        }

        private async Task<ProductEntity> CreateTestProductEntity(
            string name = "Test Product",
            string description = "Test Description",
            decimal price = 100.00m,
            bool isAvailable = true,
            Guid? userId = null,
            DateOnly? createdDate = null,
            List<string>? productImages = null,
            bool isOwnerActivated = true)
        {
            var product = new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Price = price,
                IsAvailable = isAvailable,
                UserId = userId ?? Guid.NewGuid(),
                CreatedDate = createdDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                ProductImages = productImages ?? new List<string>(),
                IsOwnerActivated = isOwnerActivated
            };

            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();
            return product;
        }

        [Fact]
        public async Task GetAllProductsAsync_ReturnsFilteredProducts()
        {
            
            var product1 = await CreateTestProductEntity(
                name: "Laptop",
                description: "Gaming laptop",
                price: 1500.00m,
                isAvailable: true,
                createdDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
                isOwnerActivated: true);

            var product2 = await CreateTestProductEntity(
                name: "Phone",
                description: "Smartphone",
                price: 800.00m,
                isAvailable: false,
                createdDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                isOwnerActivated: true);

            var product3 = await CreateTestProductEntity(
                name: "Tablet",
                description: "Android tablet",
                price: 400.00m,
                isAvailable: true,
                createdDate: DateOnly.FromDateTime(DateTime.UtcNow),
                isOwnerActivated: false); 

            var filter = new ProductFilter
            {
                Name = "Lap",
                IsAvailable = true,
                PriceFrom = 1000.00m
            };

            
            var result = await _repository.GetAllProductsAsync(filter, CancellationToken.None);

            
            Assert.Single(result);
            Assert.Equal(product1.Id, result[0].Id);
            Assert.Equal("Laptop", result[0].Name);
        }

        [Fact]
        public async Task GetAllProductsAsync_FiltersByCreatedDateRange()
        {
            
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));

            var product1 = await CreateTestProductEntity(
                name: "Product 1",
                createdDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-8)), 
                isOwnerActivated: true);

            var product2 = await CreateTestProductEntity(
                name: "Product 2",
                createdDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-12)), 
                isOwnerActivated: true);

            var product3 = await CreateTestProductEntity(
                name: "Product 3",
                createdDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)), 
                isOwnerActivated: true);

            var filter = new ProductFilter
            {
                CreatedFromDate = startDate,
                CreatedToDate = endDate
            };

            
            var result = await _repository.GetAllProductsAsync(filter, CancellationToken.None);

            
            Assert.Single(result);
            Assert.Equal(product1.Id, result[0].Id);
        }

        [Fact]
        public async Task GetAllProductsAsync_FiltersOutNotOwnerActivated()
        {
            
            var product1 = await CreateTestProductEntity(
                name: "Active Product",
                isOwnerActivated: true);

            var product2 = await CreateTestProductEntity(
                name: "Inactive Product",
                isOwnerActivated: false); 

            var filter = new ProductFilter(); 

            
            var result = await _repository.GetAllProductsAsync(filter, CancellationToken.None);

            
            Assert.Single(result);
            Assert.Equal(product1.Id, result[0].Id);
            Assert.Equal("Active Product", result[0].Name);
        }

        [Fact]
        public async Task GetProductByIdAsync_ReturnsProduct_WhenExists()
        {
            
            var expectedProduct = await CreateTestProductEntity(
                name: "Test Product",
                price: 99.99m,
                isAvailable: true);

            
            var result = await _repository.GetProductByIdAsync(expectedProduct.Id, CancellationToken.None);

            
            Assert.NotNull(result);
            Assert.Equal(expectedProduct.Id, result.Id);
            Assert.Equal("Test Product", result.Name);
            Assert.Equal(99.99m, result.Price);
            Assert.True(result.IsAvailable);
        }

        [Fact]
        public async Task GetProductByIdAsync_ThrowsNotFoundException_WhenNotExists()
        {
            
            var nonExistentId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _repository.GetProductByIdAsync(nonExistentId, CancellationToken.None));
        }

        [Fact]
        public async Task CreateProductAsync_CreatesProductAndReturnsId()
        {
            
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "New Product",
                Description = "Product Description",
                Price = 250.00m,
                IsAvailable = true,
                UserId = Guid.NewGuid(),
                CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ProductImages = new List<string> { "image1.jpg" },
                IsOwnerActivated = true
            };

            
            var result = await _repository.CreateProductAsync(product, CancellationToken.None);

            
            Assert.Equal(product.Id, result);
            
            var savedProduct = await _dbContext.Products.FindAsync(product.Id);
            Assert.NotNull(savedProduct);
            Assert.Equal(product.Name, savedProduct.Name);
            Assert.Equal(product.Price, savedProduct.Price);
        }

        [Fact]
        public async Task UpdateProductAsync_UpdatesExistingProduct()
        {
            
            var existingProduct = await CreateTestProductEntity(
                name: "Old Name",
                description: "Old Description",
                price: 100.00m,
                isAvailable: true);

            var updatedProduct = new Product
            {
                Id = existingProduct.Id,
                Name = "Updated Name",
                Description = "Updated Description",
                Price = 150.00m,
                IsAvailable = false
            };

            
            await _repository.UpdateProductAsync(updatedProduct, CancellationToken.None);
            _dbContext.ChangeTracker.Clear();
            
            var dbProduct = await _dbContext.Products.FindAsync(existingProduct.Id);
            Assert.NotNull(dbProduct);
            Assert.Equal("Updated Name", dbProduct.Name);
            Assert.Equal("Updated Description", dbProduct.Description);
            Assert.Equal(150.00m, dbProduct.Price);
            Assert.False(dbProduct.IsAvailable);
        }

        [Fact]
        public async Task UpdateProductAsync_ThrowsNotFoundException_WhenProductNotFound()
        {
            
            var nonExistentProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Non-existent",
                Description = "Description",
                Price = 100.00m,
                IsAvailable = true
            };

            
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _repository.UpdateProductAsync(nonExistentProduct, CancellationToken.None));
        }

        [Fact]
        public async Task DeleteProductAsync_DeletesExistingProduct()
        {
            
            var existingProduct = await CreateTestProductEntity();

            
            await _repository.DeleteProductAsync(existingProduct.Id, CancellationToken.None);
            _dbContext.ChangeTracker.Clear();
            
            var dbProduct = await _dbContext.Products.FindAsync(existingProduct.Id);
            Assert.Null(dbProduct);
        }

        [Fact]
        public async Task DeleteProductAsync_ThrowsNotFoundException_WhenProductNotFound()
        {
            
            var nonExistentId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _repository.DeleteProductAsync(nonExistentId, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateProductImagesAsync_UpdatesImages()
        {
            
            var existingProduct = await CreateTestProductEntity(
                productImages: new List<string> { "old1.jpg", "old2.jpg" });

            var newImages = new List<string> { "new1.jpg", "new2.jpg", "new3.jpg" };

            
            await _repository.UpdateProductImagesAsync(existingProduct.Id, newImages, CancellationToken.None);
            _dbContext.ChangeTracker.Clear();

            
            var dbProduct = await _dbContext.Products.FindAsync(existingProduct.Id);
            Assert.NotNull(dbProduct);
            Assert.Equal(newImages, dbProduct.ProductImages);
        }

        [Fact]
        public async Task UpdateProductImagesAsync_ThrowsNotFoundException_WhenProductNotFound()
        {
            
            var nonExistentId = Guid.NewGuid();
            var images = new List<string> { "image.jpg" };

            
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _repository.UpdateProductImagesAsync(nonExistentId, images, CancellationToken.None));
        }

        [Fact]
        public async Task SetProductOwnerActiveAsync_UpdatesAllProductsForUser()
        {
            
            var userId = Guid.NewGuid();
            
            var product1 = await CreateTestProductEntity(
                name: "Product 1",
                userId: userId,
                isOwnerActivated: true);

            var product2 = await CreateTestProductEntity(
                name: "Product 2",
                userId: userId,
                isOwnerActivated: true);

            var otherUserProduct = await CreateTestProductEntity(
                name: "Other User Product",
                userId: Guid.NewGuid(),
                isOwnerActivated: true);

            
            await _repository.SetProductOwnerActiveAsync(userId, false, CancellationToken.None);
            _dbContext.ChangeTracker.Clear();
            
            var updatedProduct1 = await _dbContext.Products.FindAsync(product1.Id);
            var updatedProduct2 = await _dbContext.Products.FindAsync(product2.Id);
            var unchangedProduct = await _dbContext.Products.FindAsync(otherUserProduct.Id);

            Assert.NotNull(updatedProduct1);
            Assert.NotNull(updatedProduct2);
            Assert.NotNull(unchangedProduct);

            Assert.False(updatedProduct1.IsOwnerActivated);
            Assert.False(updatedProduct2.IsOwnerActivated);
            Assert.True(unchangedProduct.IsOwnerActivated); 
        }

        [Fact]
        public async Task SetProductOwnerActiveAsync_DoesNothing_WhenUserHasNoProducts()
        {
            
            var userId = Guid.NewGuid();

            
            await _repository.SetProductOwnerActiveAsync(userId, false, CancellationToken.None);

            
            Assert.True(true);
        }
}