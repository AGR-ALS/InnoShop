using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Abstractions.Repositories;
using ProductService.Application.Exceptions;
using ProductService.DataAccess.Entities;
using ProductService.Domain.Filters;
using ProductService.Domain.Models;

namespace ProductService.DataAccess.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductServiceDbContext _dbServiceDbContext;
    private readonly IMapper _mapper;

    public ProductRepository(ProductServiceDbContext dbServiceDbContext, IMapper mapper)
    {
        _dbServiceDbContext = dbServiceDbContext;
        _mapper = mapper;
    }
    public async Task<List<Product>> GetAllProductsAsync(ProductFilter productFilter, CancellationToken cancellationToken)
    {
        var productEntitiesQuery = _dbServiceDbContext.Products.AsNoTracking().AsQueryable();
        
        if (productFilter.Name != null)
            productEntitiesQuery = productEntitiesQuery.Where(p=>p.Name.Contains(productFilter.Name));
        if (productFilter.Description != null)
            productEntitiesQuery = productEntitiesQuery.Where(p=>p.Description.Contains(productFilter.Description));
        if (productFilter.IsAvailable != null)
            productEntitiesQuery = productEntitiesQuery.Where(p=>p.IsAvailable == productFilter.IsAvailable);
        if (productFilter.CreatedFromDate != null)
            productEntitiesQuery = productEntitiesQuery.Where(p=>p.CreatedDate >= productFilter.CreatedFromDate);
        if (productFilter.CreatedToDate != null)
            productEntitiesQuery = productEntitiesQuery.Where(p=>p.CreatedDate <= productFilter.CreatedToDate);
        if (productFilter.PriceFrom != null)
            productEntitiesQuery = productEntitiesQuery.Where(p=>p.Price >= productFilter.PriceFrom);
        if (productFilter.PriceTo != null)
            productEntitiesQuery = productEntitiesQuery.Where(p=>p.Price <= productFilter.PriceTo);
        productEntitiesQuery = productEntitiesQuery.Where(p=>p.IsOwnerActivated == true);
        return _mapper.Map<List<Product>>(await productEntitiesQuery.ToListAsync(cancellationToken: cancellationToken));
    }

    public async Task<Product> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var productEntity = await _dbServiceDbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (productEntity == null)
            throw new NotFoundException("Product was not found");
        return _mapper.Map<Product>(productEntity);
    }

    public async Task<Guid> CreateProductAsync(Product product, CancellationToken cancellationToken)
    {
        var productEntity = _mapper.Map<ProductEntity>(product);
        await _dbServiceDbContext.Products.AddAsync(productEntity, cancellationToken);
        await _dbServiceDbContext.SaveChangesAsync(cancellationToken);
        return productEntity.Id;
    }

    public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken)
    {
        var rowsUpdated = await _dbServiceDbContext.Products.Where(p=>p.Id == product.Id)
            .ExecuteUpdateAsync(s=>s
                .SetProperty(r=>r.Name, r=> product.Name)
                .SetProperty(r=>r.Description, r=> product.Description)
                .SetProperty(r=>r.Price, r=> product.Price)
                .SetProperty(r=>r.IsAvailable, r=> product.IsAvailable),
                cancellationToken);
        if (rowsUpdated == 0)
        {
            throw new NotFoundException("Product was not found");
        }
    }

    public async Task DeleteProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var rowsDeleted = await _dbServiceDbContext.Products.Where(p => p.Id == id).ExecuteDeleteAsync(cancellationToken);
        if (rowsDeleted == 0)
        {
            throw new NotFoundException("Product was not found");
        }
    }

    public async Task UpdateProductImagesAsync(Guid id, List<string> productImages, CancellationToken cancellationToken)
    {
        var rowsUpdated = await _dbServiceDbContext.Products.Where(p=>p.Id == id)
            .ExecuteUpdateAsync(s=>s
                    .SetProperty(r=>r.ProductImages, r=> productImages),
                cancellationToken);
        if (rowsUpdated == 0)
        {
            throw new NotFoundException("Product was not found");
        }
    }

    public async Task SetProductOwnerActiveAsync(Guid userId, bool isOwnerActive, CancellationToken cancellationToken)
    {
        await _dbServiceDbContext.Products.Where(p=>p.UserId == userId)
            .ExecuteUpdateAsync(s=>s
                    .SetProperty(r=>r.IsOwnerActivated, r=> isOwnerActive),
                cancellationToken);
    }
}