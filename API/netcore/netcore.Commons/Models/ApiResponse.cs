using System.ComponentModel.DataAnnotations;

namespace netcore.Commons.Models;

/// <summary>Request xóa theo Id — dùng chung cho các endpoint xóa</summary>
public class DeleteByIdRequest
{
    [Required]
    public long Id { get; set; }
}

/// <summary>
/// Chuẩn phản hồi API cho toàn bộ hệ thống Fish Shop
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };

    public static ApiResponse<T> Fail(List<string> errors) => new()
    {
        Success = false,
        Errors = errors,
        Message = "Có lỗi xảy ra"
    };
}

/// <summary>
/// Non-generic static factory — dùng ApiResponse.Ok(data) để type inference hoạt động
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>ApiResponse.Ok(data) hoặc ApiResponse.Ok(data, "msg")</summary>
    public static ApiResponse<T> Ok<T>(T data, string? message = null) => ApiResponse<T>.Ok(data, message);

    /// <summary>ApiResponse.OkEmpty() — response không có data</summary>
    public static ApiResponse OkEmpty(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    /// <summary>ApiResponse.Fail&lt;T&gt;("msg") — generic fail</summary>
    public static ApiResponse<T> Fail<T>(string message, List<string>? errors = null) => ApiResponse<T>.Fail(message, errors);

    /// <summary>ApiResponse.Fail("msg") — non-generic fail</summary>
    public static new ApiResponse Fail(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };
}
