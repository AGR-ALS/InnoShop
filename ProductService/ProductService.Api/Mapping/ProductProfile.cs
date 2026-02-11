using AutoMapper;
using ProductService.Api.Contracts;
using ProductService.Domain.Filters;
using ProductService.Domain.Models;

namespace ProductService.Api.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<GetProductsRequest, ProductFilter>();
        CreateMap<PostProductRequest, Product>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateOnly.FromDateTime(DateTime.UtcNow)));
        CreateMap<PutProductRequest, Product>();
        CreateMap<Product, GetProductResponse>()
            .ForMember(dest => dest.ProductImages,
            opt =>
                opt.MapFrom((src, dest, destMember, ctx) =>
                    src.ProductImages.Select(x => new string($"{ctx.Items["BaseUrl"]}/{x.ToString()}"))));
    }
}