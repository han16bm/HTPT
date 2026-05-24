using System.ComponentModel.DataAnnotations;

namespace API.User.Models.Commands;

/// <summary>
/// Gateway gọi endpoint này để kiểm tra quyền của user trước khi forward request.
/// </summary>
public class PermissionValidationRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Permission code cần kiểm tra, VD: "PRODUCT.EDIT". Null = chỉ cần xác thực token.
    /// </summary>
    public string? RequiredPermission { get; set; }

    public string? Path { get; set; }
    public string? Method { get; set; }
}
