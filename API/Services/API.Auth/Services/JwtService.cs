using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Auth.Constants;
using API.Auth.Interfaces;
using API.Auth.Models.DTOs;
using Microsoft.IdentityModel.Tokens;
using netcore.Entities.Entities;

namespace API.Auth.Services;

public class JwtService : IJwtService
{
    private readonly AuthConfiguration _config;

    public JwtService(AuthConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(User user, string roleCode, decimal? customerId = null)
    {
        var claims = new List<Claim>
        {
            new(AuthConstants.ClaimUserId, ((long)user.Id).ToString()),
            new(AuthConstants.ClaimUserName, user.Username),
            new(AuthConstants.ClaimRole, roleCode),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (customerId.HasValue)
            claims.Add(new Claim(AuthConstants.ClaimCustomerId, ((long)customerId.Value).ToString()));

        return BuildToken(claims, _config.AccessTokenExpiryMinutes);
    }

    public string GenerateRefreshToken(User user)
    {
        var claims = new List<Claim>
        {
            new(AuthConstants.ClaimUserId, ((long)user.Id).ToString()),
            new("token_type", "refresh"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        return BuildToken(claims, _config.RefreshTokenExpiryDays * 24 * 60);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config.JwtSecret);

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = AuthConstants.JwtIssuer,
                ValidAudience = AuthConstants.JwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public long? GetUserIdFromExpiredToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config.JwtSecret);

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,   // Không check lifetime khi refresh
                ValidateIssuerSigningKey = true,
                ValidIssuer = AuthConstants.JwtIssuer,
                ValidAudience = AuthConstants.JwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
            }, out _);

            var userIdClaim = principal.FindFirst(AuthConstants.ClaimUserId)?.Value;
            return long.TryParse(userIdClaim, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }

    private string BuildToken(List<Claim> claims, int expiryMinutes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: AuthConstants.JwtIssuer,
            audience: AuthConstants.JwtAudience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
