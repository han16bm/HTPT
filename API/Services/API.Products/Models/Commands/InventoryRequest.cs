using System.ComponentModel.DataAnnotations;

namespace API.Products.Models.Commands;

public class NhapHangRequest
{
    [Required]
    public long ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng nhập phải lớn hơn 0")]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? UnitCost { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public long? CreatedBy { get; set; }
}
