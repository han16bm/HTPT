using System.Security.Claims;
using netcore.Entities.Entities;

namespace API.Auth.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, string roleCode, decimal? customerId = null);
    string GenerateRefreshToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
    long? GetUserIdFromExpiredToken(string token);
}
