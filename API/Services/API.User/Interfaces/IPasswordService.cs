namespace API.User.Interfaces;

public interface IPasswordService
{
    string HashPassword(string plainText);
    bool VerifyPassword(string plainText, string hash);
}
