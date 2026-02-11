using AutoMapper;
using ProductService.DataAccess.Entities;
using ProductService.Domain;
using ProductService.Domain.Models;

namespace ProductService.DataAccess.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<ProductEntity, Product>();
        CreateMap<Product, ProductEntity>();
    }
}