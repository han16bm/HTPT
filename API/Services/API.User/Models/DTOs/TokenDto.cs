namespace API.User.Models.DTOs;

/// <summary>
/// Response chứa JWT tokens sau khi đăng nhập / refresh.
/// </summary>
public class TokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }  // Giây
    public UserDto User { get; set; } = null!;
}
