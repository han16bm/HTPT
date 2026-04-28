using System.ComponentModel.DataAnnotations;

namespace API.Auth.Models.Commands;

public class CapNhatThongTinRequest
{
    [Required(ErrorMessage = "Họ tên không được để trống")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(20, ErrorMessage = "Giới tính không hợp lệ")]
    public string? Gender { get; set; }
}
