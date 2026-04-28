namespace API.Products.Models.DTOs;

public class CategoryDto
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CategoryDto> Children { get; set; } = [];
}
