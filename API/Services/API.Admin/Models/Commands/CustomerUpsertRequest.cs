namespace API.Admin.Models.Commands;

public class CustomerUpsertRequest
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public int Status { get; set; } = 1;
    public bool IsAdmin { get; set; }
}
