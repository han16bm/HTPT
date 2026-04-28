namespace API.Products.Models.DTOs;

public class ProductDto
{
    public long Id { get; set; }
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public int SoldQuantity { get; set; }
    public decimal? WeightGrams { get; set; }
    public bool Status { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProductImageDto> Images { get; set; } = [];
}

public class ProductListDto
{
    public long Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public int SoldQuantity { get; set; }
    public bool Status { get; set; }
    public bool IsFeatured { get; set; }
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductImageDto
{
    public long Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}
