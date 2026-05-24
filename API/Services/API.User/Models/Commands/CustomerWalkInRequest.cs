using System.ComponentModel.DataAnnotations;

namespace API.User.Models.Commands;

public class CustomerWalkInRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
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
}
