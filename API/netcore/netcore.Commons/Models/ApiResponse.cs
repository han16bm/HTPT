using System.ComponentModel.DataAnnotations;

namespace netcore.Commons.Models;

public class DeleteByIdRequest
{
    [Required]
    public long Id { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
        };
    }

    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
        };
    }

    public static ApiResponse<T> Fail(List<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = errors,
            Message = "Có lỗi xảy ra",
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse<T> Ok<T>(T data, string? message = null)
    {
        return ApiResponse<T>.Ok(data, message);
    }

    public static ApiResponse OkEmpty(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
        };
    }

    public static ApiResponse<T> Fail<T>(string message, List<string>? errors = null)
    {
        return ApiResponse<T>.Fail(message, errors);
    }

    public static new ApiResponse Fail(string message, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors,
        };
    }
}
