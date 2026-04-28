namespace API.Admin.Models.DTOs;

public class CustomerDetailDto
{
    public long Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public bool IsAdmin { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CustomerOrderSummaryDto> RecentOrders { get; set; } = [];
}

public class CustomerListDto
{
    public long Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public bool IsActive { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerOrderSummaryDto
{
    public string OrderCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReportDto
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProductsSold { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<RevenueChartDto> Chart { get; set; } = [];
    public List<TopProductDto> TopProducts { get; set; } = [];
}

// ===== Report page DTOs (khớp FE interfaces) =====

public class RevenueReportRowDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}

public class TopProductReportRowDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Sold { get; set; }
    public decimal Revenue { get; set; }
}

public class OrderSummaryReportDto
{
    public int TotalOrders { get; set; }
    public List<OrderStatusSummaryDto> ByStatus { get; set; } = [];
}

public class OrderStatusSummaryDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}
