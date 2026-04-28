using System.ComponentModel.DataAnnotations;

namespace API.Auth.Models.Commands;

public class LamMoiTokenRequest
{
    [Required(ErrorMessage = "RefreshToken không được để trống")]
    public string RefreshToken { get; set; } = string.Empty;
}
