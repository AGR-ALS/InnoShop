namespace ProductService.Api.Contracts;

public class GetProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> ProductImages { get; set; } = null!;
    public decimal Price { get; set; }
    public DateOnly CreatedDate { get; set; }
    public bool IsAvailable { get; set; }
    public Guid UserId { get; set; }
}