namespace API.Products.Models.Queries;

public class CategoryQuery
{
    public string? Name { get; set; }
    public long? ParentId { get; set; }
    public bool? Status { get; set; }
}

public class InventoryQuery
{
    public long? ProductId { get; set; }
    public string? TransactionType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
