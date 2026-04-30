namespace API.Admin.Models.DTOs;

public class DashboardStatsDto
{
    public TodayStatsDto Today { get; set; } = new();
    public MonthStatsDto Month { get; set; } = new();
    public List<RevenueChartDto> RevenueChart { get; set; } = [];
    public List<TopProductDto> TopProducts { get; set; } = [];
    public List<RecentOrderDto> RecentOrders { get; set; } = [];
    public OrderStatusCountDto OrderStatus { get; set; } = new();
}

public class TodayStatsDto
{
    public int NewOrders { get; set; }
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
    public int NewCustomers { get; set; }
}

public class MonthStatsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public decimal RevenueGrowth { get; set; } // So với tháng trước (%)
}

public class RevenueChartDto
{
    public string Date { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}

public class TopProductDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SoldQuantity { get; set; }
    public decimal Revenue { get; set; }
}

public class RecentOrderDto
{
    public string OrderCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class OrderStatusCountDto
{
    public int Pending { get; set; }
    public int Confirmed { get; set; }
    public int Processing { get; set; }
    public int Shipped { get; set; }
    public int Delivered { get; set; }
    public int Cancelled { get; set; }
}
