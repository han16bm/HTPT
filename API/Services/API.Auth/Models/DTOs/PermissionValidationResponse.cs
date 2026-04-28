namespace API.Auth.Models.DTOs;

/// <summary>
/// Response của /permissions/check — Gateway gọi để xác thực quyền.
/// </summary>
public class PermissionValidationResponse
{
    public bool IsValid { get; set; }
    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
    public string? Message { get; set; }
}
