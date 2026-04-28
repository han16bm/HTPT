namespace netcore.Commons.Exceptions;

/// <summary>
/// Ném ra khi không có quyền truy cập — tự động map thành HTTP 401/403.
/// </summary>
public class UnauthorizedException : MessageException
{
    public UnauthorizedException(string message = "Bạn không có quyền thực hiện thao tác này")
        : base(message, 401)
    {
    }
}
