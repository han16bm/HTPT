namespace netcore.Commons.Models;

/// <summary>
/// Payload chuẩn cho endpoint healthcheck của mọi API service.
/// Dùng trong <c>Task&lt;ApiResponse&lt;HealthCheckStatus&gt;&gt;</c> để Swagger
/// generate schema đúng cho client.
/// </summary>
public record HealthCheckStatus(string Status, string Service, DateTime Time);
