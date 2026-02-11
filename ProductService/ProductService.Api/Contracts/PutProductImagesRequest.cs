namespace ProductService.Api.Contracts;

public class PutProductImagesRequest
{
    public List<IFormFile> ProductImages { get; set; } = null!;
}