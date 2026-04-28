using API.Auth.Interfaces;

namespace API.Auth.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword(string plainText)
        => BCrypt.Net.BCrypt.HashPassword(plainText, workFactor: 12);

    public bool VerifyPassword(string plainText, string hash)
        => BCrypt.Net.BCrypt.Verify(plainText, hash);
}
