using System.ComponentModel.DataAnnotations;

namespace API.User.Models.Commands;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Họ tên không được để trống")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }

    [MaxLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
    public string? Address { get; set; }

    [MaxLength(500, ErrorMessage = "Địa chỉ cụ thể không được vượt quá 500 ký tự")]
    public string? AddressLine { get; set; }

    [MaxLength(100, ErrorMessage = "Phường/xã không được vượt quá 100 ký tự")]
    public string? Ward { get; set; }

    [MaxLength(100, ErrorMessage = "Quận/huyện không được vượt quá 100 ký tự")]
    public string? District { get; set; }

    [MaxLength(100, ErrorMessage = "Tỉnh/thành phố không được vượt quá 100 ký tự")]
    public string? Province { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(20, ErrorMessage = "Giới tính không hợp lệ")]
    public string? Gender { get; set; }
}
