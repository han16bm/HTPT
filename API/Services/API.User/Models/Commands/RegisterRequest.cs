using System.ComponentModel.DataAnnotations;

namespace API.User.Models.Commands;

public class RegisterRequest
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    [MinLength(4, ErrorMessage = "Tên đăng nhập tối thiểu 4 ký tự")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ tên không được để trống")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }
}
