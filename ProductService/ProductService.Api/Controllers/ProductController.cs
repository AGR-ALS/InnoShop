using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProductService.Api.Contracts;
using ProductService.Api.Settings.Application;
using ProductService.Application.Abstractions.Files.ImageUploading;
using ProductService.Application.Abstractions.Services;
using ProductService.Domain.Filters;
using ProductService.Domain.Models;
using ProductService.Infrastructure.Files;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IImageUploadingService _imageUploadingService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ApplicationConfiguration _applicationConfiguration;

    public ProductController(IProductService productService, IMapper mapper, IWebHostEnvironment webHostEnvironment,
        IImageUploadingService imageUploadingService, ICurrentUserService currentUserService, IHostEnvironment hostEnvironment, IOptions<ApplicationConfiguration> applicationConfiguration )
    {
        _productService = productService;
        _mapper = mapper;
        _webHostEnvironment = webHostEnvironment;
        _imageUploadingService = imageUploadingService;
        _currentUserService = currentUserService;
        _hostEnvironment = hostEnvironment;
        _applicationConfiguration = applicationConfiguration.Value;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts([FromQuery] GetProductsRequest request,
        CancellationToken cancellationToken, IValidator<GetProductsRequest> validator)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var productFilter = _mapper.Map<ProductFilter>(request);
        var products = await _productService.GetAllProductsAsync(productFilter, cancellationToken);
        var baseUrl = _applicationConfiguration.ApplicationUrl;
        var productsResponse = _mapper.Map<List<GetProductResponse>>(products, opt => opt.Items["BaseUrl"] = baseUrl);
        return Ok(productsResponse);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Product>> GetProductById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        var baseUrl = _applicationConfiguration.ApplicationUrl;
        var productResponse = _mapper.Map<GetProductResponse>(product, opt => opt.Items["BaseUrl"] = baseUrl);
        return Ok(productResponse);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateProduct([FromBody] PostProductRequest request,
        CancellationToken cancellationToken, IValidator<PostProductRequest> validator)
    {
        Console.WriteLine(User.Identity?.AuthenticationType);
        Console.WriteLine(User.Identity?.Name);

        
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var product = _mapper.Map<Product>(request);
        if (_currentUserService.Id != null) product.UserId = new Guid(_currentUserService.Id);
        var productId = await _productService.CreateProductAsync(product, cancellationToken);
        return Ok(productId);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateProduct([FromRoute] Guid id, [FromBody] PutProductRequest request,
        IValidator<PutProductRequest> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        if (!await CheckOnEditingRights(id, cancellationToken))
            return Forbid();


        var product = _mapper.Map<Product>(request);
        product.Id = id;
        await _productService.UpdateProductAsync(product, cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteProduct([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        if (!await CheckOnEditingRights(id, cancellationToken))
            return Forbid();
        await _productService.DeleteProductAsync(id, cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpPut("{id:guid}/images")]
    public async Task<ActionResult> UpdateProductImages([FromRoute] Guid id, [FromForm] PutProductImagesRequest request,
        IValidator<PutProductImagesRequest> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        if (!await CheckOnEditingRights(id, cancellationToken))
            return Forbid();

        var productImages =
            await _imageUploadingService.UploadImageAsync(request.ProductImages.Select(p => new FormFileAdapter(p)), _webHostEnvironment.WebRootPath,
                cancellationToken);
        await _productService.UpdateProductImagesAsync(id, productImages, cancellationToken);
        return Ok();
    }

    private async Task<bool> CheckOnEditingRights(Guid id, CancellationToken cancellationToken)
    {
        var productOwnerId = (await _productService.GetProductByIdAsync(id, cancellationToken)).UserId;
        var isAdmin = _currentUserService.Role == "Admin";
        if (productOwnerId.ToString() != _currentUserService.Id && !isAdmin)
            return false;
        return true;
    }
}