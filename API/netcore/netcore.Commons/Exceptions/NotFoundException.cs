namespace netcore.Commons.Exceptions;

public class NotFoundException : MessageException
{
    public NotFoundException(string resourceName, object identifier)
        : base($"Không tìm thấy {resourceName} với id = {identifier}", 404)
    {
    }

    public NotFoundException(string message)
        : base(message, 404)
    {
    }
}
