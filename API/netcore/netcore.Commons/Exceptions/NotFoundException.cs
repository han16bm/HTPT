namespace netcore.Commons.Exceptions;

/// <summary>
/// Ném ra khi không tìm thấy resource — tự động map thành HTTP 404.
/// </summary>
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
