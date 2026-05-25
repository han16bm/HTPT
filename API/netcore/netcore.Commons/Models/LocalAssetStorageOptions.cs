namespace netcore.Commons.Models;

public sealed class LocalAssetStorageOptions
{
    public string RootPath { get; set; } = "assets";
    public string RequestPath { get; set; } = "/assets";
    public long MaxImageBytes { get; set; } = 10 * 1024 * 1024;
}
