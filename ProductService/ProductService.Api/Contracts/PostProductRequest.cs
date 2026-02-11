namespace ProductService.Api.Contracts;

public class PostProductRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal? Price { get; set; }
    public bool IsAvailable { get; set; }
}