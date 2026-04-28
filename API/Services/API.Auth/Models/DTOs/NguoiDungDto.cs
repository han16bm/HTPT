namespace API.Auth.Models.DTOs;

/// <summary>
/// Thông tin người dùng trả về sau khi đăng nhập.
/// </summary>
public class NguoiDungDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? CustomerCode { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? AvatarUrl { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public long? CustomerId { get; set; }
}
