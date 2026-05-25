namespace netcore.Commons.Exceptions;

public class UnauthorizedException : MessageException
{
    public UnauthorizedException(string message = "Bạn không có quyền thực hiện thao tác này")
        : base(message, 401)
    {
    }
}
