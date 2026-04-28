namespace API.Auth.Models.DTOs;

/// <summary>
/// Config đọc từ appsettings.json — section "AuthService"
/// </summary>
public class AuthConfiguration
{
    public string JwtSecret { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
