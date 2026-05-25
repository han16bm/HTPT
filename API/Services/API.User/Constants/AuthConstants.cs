namespace API.User.Constants;

public static class AuthConstants
{
    public const string RoleAdmin = "ADMIN";

    public const string RoleCustomer = "CUSTOMER";

    public const string ClaimUserId = "sub";
    public const string ClaimUserName = "username";
    public const string ClaimRole = "role";
    public const string ClaimCustomerId = "customer_id";

    public const string JwtIssuer = "FishShop";
    public const string JwtAudience = "FishShop";
}
