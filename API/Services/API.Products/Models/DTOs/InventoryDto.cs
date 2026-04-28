namespace API.Products.Models.DTOs;

public class InventoryTransactionDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public long? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? Note { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LowStockProductDto
{
    public long Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public int SoldQuantity { get; set; }
}
