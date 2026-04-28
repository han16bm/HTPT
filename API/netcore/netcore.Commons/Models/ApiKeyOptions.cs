namespace netcore.Commons.Models;

/// <summary>
/// Cấu hình cho [ApiKey] attribute — đọc từ appsettings.json section "ApiKey"
/// </summary>
public class ApiKeyOptions
{
    public string HeaderName { get; set; } = "X-Api-Key";
    public string Key { get; set; } = string.Empty;
}
