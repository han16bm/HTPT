using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace FishShop.Gateway.Services;

public class TokenValidator
{
    private readonly IConfiguration _config;
    private readonly ILogger<TokenValidator> _logger;

    public TokenValidator(IConfiguration config, ILogger<TokenValidator> logger)
    {
        _config = config;
        _logger = logger;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var secret = _config["Gateway:JwtSecret"];
            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("Gateway:JwtSecret not configured");
            }

            var issuer = _config["Gateway:JwtIssuer"];
            if (string.IsNullOrEmpty(issuer))
            {
                issuer = "FishShop";
            }

            var audience = _config["Gateway:JwtAudience"];
            if (string.IsNullOrEmpty(audience))
            {
                audience = "FishShop";
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
            };

            var handler = new JsonWebTokenHandler();
            handler.MapInboundClaims = false;

            var result = handler.ValidateTokenAsync(token, parameters).GetAwaiter().GetResult();
            if (!result.IsValid)
            {
                var exceptionType = result.Exception != null ? result.Exception.GetType().Name : "Unknown";
                var exceptionMessage = result.Exception != null ? result.Exception.Message : "Unknown error";
                _logger.LogWarning("Token validation failed: {Type} {Msg}", exceptionType, exceptionMessage);
                return null;
            }

            return new ClaimsPrincipal(result.ClaimsIdentity);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning("Token validation failed: {Type} {Msg}", ex.GetType().Name, ex.Message);
            return null;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Token argument error: {Msg}", ex.Message);
            return null;
        }
    }

    public string? GetClaim(ClaimsPrincipal principal, string claimType)
    {
        var claim = principal.FindFirst(claimType);
        if (claim == null)
        {
            return null;
        }
        return claim.Value;
    }
}
