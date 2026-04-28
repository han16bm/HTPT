namespace netcore.Commons.Models;

public class CloudinaryOptions
{
    public string CloudinaryUrl { get; set; } = string.Empty;
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string UploadPreset { get; set; } = string.Empty;
    public string SignatureAlgorithm { get; set; } = "sha1";
}
