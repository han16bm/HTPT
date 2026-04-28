namespace netcore.Commons.Exceptions;

/// <summary>
/// Exception có message an toàn để hiển thị trực tiếp cho client.
/// Dùng khi cần throw lỗi business logic với message tiếng Việt.
/// </summary>
public class MessageException : Exception
{
    public int StatusCode { get; }

    public MessageException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
