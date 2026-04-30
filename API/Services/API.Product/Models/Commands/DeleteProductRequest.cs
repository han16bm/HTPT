using System.ComponentModel.DataAnnotations;

namespace API.Product.Models.Commands;

public class DeleteProductRequest
{
    [Required]
    public long Id { get; set; }
}
