namespace API.User.Models.DTOs;

public class PermissionValidationResponse
{
    public bool IsValid { get; set; }
    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];
    public string? Message { get; set; }
}
