namespace API.User.Models.DTOs;

public class AuthConfiguration
{
    public string JwtSecret { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
