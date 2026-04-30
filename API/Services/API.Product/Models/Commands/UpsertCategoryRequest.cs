using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace API.Product.Models.Commands;

public class UpsertCategoryRequest
{
    public long? Id { get; set; }

    public long? ParentId { get; set; }

    [StringLength(30)]
    public string? CategoryCode { get; set; }

    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Slug { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public bool RemoveImage { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public bool Status { get; set; } = true;
}

public class DeleteCategoryRequest
{
    [Required]
    public long Id { get; set; }
}
