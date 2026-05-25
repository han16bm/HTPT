namespace netcore.Commons.Exceptions;

public class MessageException : Exception
{
    public int StatusCode { get; }

    public MessageException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
