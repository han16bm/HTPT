using System.Security.Claims;
using netcore.Entities.Entities;
using UserEntity = netcore.Entities.Entities.User;

namespace API.User.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(UserEntity user, string roleCode, decimal? customerId = null);
    string GenerateRefreshToken(UserEntity user);
    ClaimsPrincipal? ValidateToken(string token);
    long? GetUserIdFromExpiredToken(string token);
}
