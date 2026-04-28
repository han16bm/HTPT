namespace API.Auth.Constants;

/// <summary>
/// Role codes chuẩn — phải khớp với cột CODE trong bảng ROLES của Oracle.
/// </summary>
public static class AuthConstants
{
    public const string RoleAdmin = "ADMIN";
    public const string RoleStaff = "STAFF";
    public const string RoleCustomer = "CUSTOMER";

    public const string ClaimUserId = "sub";
    public const string ClaimUserName = "username";
    public const string ClaimRole = "role";
    public const string ClaimCustomerId = "customer_id";

    public const string JwtIssuer = "FishShop";
    public const string JwtAudience = "FishShop";
}
