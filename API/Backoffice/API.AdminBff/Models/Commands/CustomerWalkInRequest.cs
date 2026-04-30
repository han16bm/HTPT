namespace API.Admin.Models.Commands;

public class CustomerWalkInRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
}
