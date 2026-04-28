namespace netcore.Commons.Models;

public sealed class ObjectStorageFileResult
{
    public required byte[] Content { get; init; }
    public required string ContentType { get; init; }
}
