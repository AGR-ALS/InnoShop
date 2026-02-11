namespace ProductService.Domain.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public Guid UserId { get; set; }
    public DateOnly CreatedDate { get; set; }
    public List<string> ProductImages { get; set; } = null!;
    public bool IsOwnerActivated { get; set; } = true;
}