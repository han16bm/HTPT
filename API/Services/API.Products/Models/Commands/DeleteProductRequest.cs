using System.ComponentModel.DataAnnotations;

namespace API.Products.Models.Commands;

public class DeleteProductRequest
{
    [Required]
    public long Id { get; set; }
}
