namespace ProductService.Api.Contracts;

public class GetProductsRequest
{ 
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateOnly? CreatedFromDate { get; set; }
    public DateOnly? CreatedToDate { get; set; }
    public bool? IsAvailable { get; set; }
    public decimal? PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }
        
}