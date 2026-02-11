using System.Security.Claims;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using ProductService.Api.Contracts;
using ProductService.Api.Controllers;
using ProductService.Api.Settings.Application;
using ProductService.Application.Abstractions.Files.ImageUploading;
using ProductService.Application.Abstractions.Services;
using ProductService.Domain.Filters;
using ProductService.Domain.Models;
using ProductService.Infrastructure.Files;
using Xunit;

namespace ProductService.Tests.ApiTests;

public class ProductControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock;
        private readonly Mock<IImageUploadingService> _imageUploadingServiceMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IHostEnvironment> _hostEnvironmentMock;
        private readonly Mock<IValidator<GetProductsRequest>> _getProductsValidatorMock;
        private readonly Mock<IValidator<PostProductRequest>> _postProductValidatorMock;
        private readonly Mock<IValidator<PutProductRequest>> _putProductValidatorMock;
        private readonly Mock<IValidator<PutProductImagesRequest>> _putProductImagesValidatorMock;
        private readonly Mock<IOptions<ApplicationConfiguration>> _applicationConfigurationOptionsMock;
        
        private readonly ProductController _controller;
        private readonly DefaultHttpContext _httpContext;

        public ProductControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _mapperMock = new Mock<IMapper>();
            _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            _imageUploadingServiceMock = new Mock<IImageUploadingService>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _hostEnvironmentMock = new Mock<IHostEnvironment>();
            _getProductsValidatorMock = new Mock<IValidator<GetProductsRequest>>();
            _postProductValidatorMock = new Mock<IValidator<PostProductRequest>>();
            _putProductValidatorMock = new Mock<IValidator<PutProductRequest>>();
            _putProductImagesValidatorMock = new Mock<IValidator<PutProductImagesRequest>>();
            _applicationConfigurationOptionsMock = new Mock<IOptions<ApplicationConfiguration>>();
            

            _applicationConfigurationOptionsMock.Setup(o => o.Value)
                .Returns(new ApplicationConfiguration { ApplicationUrl = "http://localhost:5186" });

            _controller = new ProductController(
                _productServiceMock.Object,
                _mapperMock.Object,
                _webHostEnvironmentMock.Object,
                _imageUploadingServiceMock.Object,
                _currentUserServiceMock.Object,
                _hostEnvironmentMock.Object,
                _applicationConfigurationOptionsMock.Object
            );

            _httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        private void SetupRegularUser(string userId = "regular-user-id", string role = "Regular")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, "user@example.com"),
                new Claim(ClaimTypes.Name, "Regular User"),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            
            _httpContext.User = principal;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        [Fact]
        public async Task GetProducts_CallsServiceAndReturnsOk()
        {
            
            var request = new GetProductsRequest
            {
                Name = "Laptop",
                PriceFrom = 500
            };

            var filter = new ProductFilter
            {
                Name = "Laptop",
                PriceFrom = 500
            };

            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Laptop",
                    Price = 1000,
                    IsAvailable = true,
                    ProductImages = new List<string>()
                }
            };

            var expectedResponse = new List<GetProductResponse>
            {
                new GetProductResponse
                {
                    Id = products[0].Id,
                    Name = "Laptop",
                    Price = 1000,
                    IsAvailable = true,
                    ProductImages = new List<string>()
                }
            };
            

            
            
            _getProductsValidatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mapperMock.Setup(m => m.Map<ProductFilter>(request)).Returns(filter);
    
            _productServiceMock
                .Setup(s => s.GetAllProductsAsync(filter, It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            
            _mapperMock
                .Setup(m => m.Map<List<GetProductResponse>>(products, It.IsAny<Action<IMappingOperationOptions<object, List<GetProductResponse>>>>()))
                .Returns(expectedResponse);

            
            var result = await _controller.GetProducts(request, CancellationToken.None, _getProductsValidatorMock.Object);

            
            var okResult = Assert.IsType<ActionResult<List<Product>>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            Assert.Equal(expectedResponse, objectResult.Value);
    
            _productServiceMock.Verify(s => 
                    s.GetAllProductsAsync(filter, It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task GetProductById_CallsServiceAndReturnsOk()
        {
            
            var productId = Guid.NewGuid();
            var baseUrl = "http://localhost:5000";
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                Price = 150.50m,
                IsAvailable = true
            };

            var expectedResponse = new GetProductResponse
            {
                Id = productId,
                Name = "Test Product",
                Price = 150.50m,
                IsAvailable = true
            };

            var applicationConfig = new ApplicationConfiguration { ApplicationUrl = baseUrl };
            _applicationConfigurationOptionsMock
                .Setup(x => x.Value)
                .Returns(applicationConfig);

            _productServiceMock
                .Setup(s => s.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _mapperMock
                .Setup(m => m.Map<GetProductResponse>(
                    It.IsAny<Product>(), 
                    It.IsAny<Action<IMappingOperationOptions<object, GetProductResponse>>>()))
                .Returns(expectedResponse);

            
            var result = await _controller.GetProductById(productId, CancellationToken.None);

            
            var okResult = Assert.IsType<ActionResult<Product>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            Assert.Equal(expectedResponse, objectResult.Value);
    
            _productServiceMock.Verify(s => 
                    s.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()), 
                Times.Once);
    
            _mapperMock.Verify(m => m.Map<GetProductResponse>(
                    It.Is<Product>(p => p.Id == productId),
                    It.IsAny<Action<IMappingOperationOptions<object, GetProductResponse>>>()), 
                Times.Once);
        }
        [Fact]
        public async Task CreateProduct_CallsServiceAndReturnsProductId()
        {
            
            SetupRegularUser("697f05b7-a353-4c64-b199-3aeca7dc9d13");
            var request = new PostProductRequest
            {
                Name = "New Product",
                Description = "Product Description",
                Price = 99.99m,
                IsAvailable = true
            };

            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "New Product",
                Description = "Product Description",
                Price = 99.99m,
                IsAvailable = true,
                UserId = new Guid("697f05b7-a353-4c64-b199-3aeca7dc9d13")
            };

            _postProductValidatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mapperMock.Setup(m => m.Map<Product>(request)).Returns(product);
            _currentUserServiceMock.Setup(s => s.Id).Returns("697f05b7-a353-4c64-b199-3aeca7dc9d13");
            
            _productServiceMock
                .Setup(s => s.CreateProductAsync(product, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productId);

            
            var result = await _controller.CreateProduct(request, CancellationToken.None, _postProductValidatorMock.Object);

            
            var okResult = Assert.IsType<ActionResult<Guid>>(result);
            var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            Assert.Equal(productId, objectResult.Value);
            
            _productServiceMock.Verify(s => 
                s.CreateProductAsync(It.Is<Product>(p => p.UserId == new Guid("697f05b7-a353-4c64-b199-3aeca7dc9d13")), It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_CallsService_WhenUserIsOwner()
        {
            
            SetupRegularUser("owner-id");
            var productId = Guid.NewGuid();
            var request = new PutProductRequest
            {
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 199.99m,
                IsAvailable = false
            };

            var product = new Product
            {
                Id = productId,
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 199.99m,
                IsAvailable = false,
                UserId = new Guid("0eefcb77-9fb9-4fbd-92a3-75b335ee0d13")
            };

            _putProductValidatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mapperMock.Setup(m => m.Map<Product>(request)).Returns(product);
            _currentUserServiceMock.Setup(s => s.Id).Returns("0eefcb77-9fb9-4fbd-92a3-75b335ee0d13");
            _currentUserServiceMock.Setup(s => s.Role).Returns("Regular");
            
            _productServiceMock
                .Setup(s => s.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product { UserId = new Guid("0eefcb77-9fb9-4fbd-92a3-75b335ee0d13") });

            
            var result = await _controller.UpdateProduct(productId, request, _putProductValidatorMock.Object, CancellationToken.None);

            
            Assert.IsType<OkResult>(result);
            _productServiceMock.Verify(s => 
                s.UpdateProductAsync(product, It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_ReturnsForbid_WhenUserIsNotOwnerAndNotAdmin()
        {
            
            SetupRegularUser("other-user-id");
            var productId = Guid.NewGuid();
            var request = new PutProductRequest();

            _putProductValidatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _currentUserServiceMock.Setup(s => s.Id).Returns("other-user-id");
            _currentUserServiceMock.Setup(s => s.Role).Returns("Regular");
            
            _productServiceMock
                .Setup(s => s.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product { UserId = Guid.NewGuid() }); 

            
            var result = await _controller.UpdateProduct(productId, request, _putProductValidatorMock.Object, CancellationToken.None);

            
            Assert.IsType<ForbidResult>(result);
            _productServiceMock.Verify(s => 
                s.UpdateProductAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), 
                Times.Never);
        }

        [Fact]
        public async Task DeleteProduct_CallsService_WhenUserIsAdmin()
        {
            
            SetupRegularUser("admin-id", "Admin");
            var productId = Guid.NewGuid();

            _currentUserServiceMock.Setup(s => s.Id).Returns("admin-id");
            _currentUserServiceMock.Setup(s => s.Role).Returns("Admin");
            
            _productServiceMock
                .Setup(s => s.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product { UserId = Guid.NewGuid() }); 

            
            var result = await _controller.DeleteProduct(productId, CancellationToken.None);

            
            Assert.IsType<OkResult>(result);
            _productServiceMock.Verify(s => 
                s.DeleteProductAsync(productId, It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task UpdateProductImages_CallsService_WhenUserIsOwner()
        {
            
            SetupRegularUser("owner-id");
            var productId = Guid.NewGuid();
            var mockFormFile = new Mock<IFormFile>();
            var request = new PutProductImagesRequest
            {
                ProductImages = new List<IFormFile> { mockFormFile.Object }
            };

            var uploadedImages = new List<string> { "uploads/image1.jpg" };

            _putProductImagesValidatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _currentUserServiceMock.Setup(s => s.Id).Returns("0eefcb77-9fb9-4fbd-92a3-75b335ee0d13");
            _currentUserServiceMock.Setup(s => s.Role).Returns("Regular");
            
            _productServiceMock
                .Setup(s => s.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product { UserId = new Guid("0eefcb77-9fb9-4fbd-92a3-75b335ee0d13") });

            _webHostEnvironmentMock.Setup(e => e.WebRootPath).Returns("/wwwroot");
            _imageUploadingServiceMock
                .Setup(s => s.UploadImageAsync(It.IsAny<IEnumerable<FormFileAdapter>>(), "/wwwroot", It.IsAny<CancellationToken>(), It.IsAny<string>()))
                .ReturnsAsync(uploadedImages);

            
            var result = await _controller.UpdateProductImages(productId, request, _putProductImagesValidatorMock.Object, CancellationToken.None);

            
            Assert.IsType<OkResult>(result);
            _imageUploadingServiceMock.Verify(s => 
                s.UploadImageAsync(It.IsAny<IEnumerable<FormFileAdapter>>(), "/wwwroot", It.IsAny<CancellationToken>(),It.IsAny<string>()), 
                Times.Once);
            _productServiceMock.Verify(s => 
                s.UpdateProductImagesAsync(productId, uploadedImages, It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Fact]
        public async Task UpdateProductImages_ReturnsForbid_WhenUserIsNotOwner()
        {
            
            SetupRegularUser("other-user-id");
            var productId = Guid.NewGuid();
            var request = new PutProductImagesRequest();

            _putProductImagesValidatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _currentUserServiceMock.Setup(s => s.Id).Returns("other-user-id");
            _currentUserServiceMock.Setup(s => s.Role).Returns("Regular");
            
            _productServiceMock
                .Setup(s => s.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Product { UserId = Guid.NewGuid() }); 

            
            var result = await _controller.UpdateProductImages(productId, request, _putProductImagesValidatorMock.Object, CancellationToken.None);

            
            Assert.IsType<ForbidResult>(result);
            _imageUploadingServiceMock.Verify(s => 
                s.UploadImageAsync(It.IsAny<IEnumerable<FormFileAdapter>>(), It.IsAny<string>(), It.IsAny<CancellationToken>(),  It.IsAny<string>()), 
                Times.Never);
        }
}