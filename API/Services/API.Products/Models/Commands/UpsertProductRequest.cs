using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace API.Products.Models.Commands;

public class UpsertProductRequest
{
    public long? Id { get; set; } // null = tạo mới, có giá trị = cập nhật

    [Required(ErrorMessage = "Danh mục không được để trống")]
    public long CategoryId { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [StringLength(200, ErrorMessage = "Tên sản phẩm tối đa 200 ký tự")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Slug tối đa 250 ký tự")]
    public string? Slug { get; set; }

    [StringLength(50)]
    public string? Sku { get; set; }

    [StringLength(500)]
    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public bool RemoveImage { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá vốn không hợp lệ")]
    public decimal CostPrice { get; set; }

    [Required(ErrorMessage = "Giá bán không được để trống")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá bán không hợp lệ")]
    public decimal SalePrice { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public decimal? WeightGrams { get; set; }

    public bool Status { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    public List<UpsertProductImageRequest> Images { get; set; } = [];
}

public class UpsertProductImageRequest
{
    public long? Id { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public bool Remove { get; set; }
}
