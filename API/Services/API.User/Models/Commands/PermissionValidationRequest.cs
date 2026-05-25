using System.ComponentModel.DataAnnotations;

namespace API.User.Models.Commands;

public class PermissionValidationRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    public string? RequiredPermission { get; set; }
    public string? Path { get; set; }
    public string? Method { get; set; }
}
