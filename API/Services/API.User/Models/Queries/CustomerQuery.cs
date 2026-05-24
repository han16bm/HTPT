namespace API.User.Models.Queries;

public class CustomerQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } = "created_at";
    public string? SortDir { get; set; } = "desc";
}

public class ReportQuery
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string GroupBy { get; set; } = "day"; // day | week | month
}
