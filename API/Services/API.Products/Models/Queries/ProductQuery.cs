namespace API.Products.Models.Queries;

public class ProductQuery
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public long? CategoryId { get; set; }
    public bool? Status { get; set; }
    public bool? IsFeatured { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; } = "created_at";
    public string? SortDir { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
