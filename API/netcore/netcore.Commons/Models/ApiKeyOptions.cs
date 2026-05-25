namespace netcore.Commons.Models;

public class ApiKeyOptions
{
    public string HeaderName { get; set; } = "X-Api-Key";
    public string Key { get; set; } = string.Empty;
}
