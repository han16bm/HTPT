namespace API.Content.Models.Queries;

public class BlogQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? Keyword { get; set; }
    public long? CategoryId { get; set; }
    public string? Status { get; set; } // DRAFT | PUBLISHED (null = all)
    public string? SortBy { get; set; } = "published_at";
    public string? SortDir { get; set; } = "desc";
}

public class ContactQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
    public string? Keyword { get; set; }
}
