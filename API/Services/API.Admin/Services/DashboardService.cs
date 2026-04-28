using API.Admin.Interfaces;
using API.Admin.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using netcore.Entities.Interfaces;

namespace API.Admin.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;

    public DashboardService(IUnitOfWork uow) => _uow = uow;

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var prevMonthStart = monthStart.AddMonths(-1);

        // Today stats
        var todayOrders = await _uow.Orders.Query()
            .Where(o => o.CreatedAt >= todayStart && o.OrderStatus != "CANCELLED")
            .ToListAsync(ct);

        var todayPaidOrderIds = todayOrders
            .Where(o => o.PaymentStatus == "PAID")
            .Select(o => o.Id)
            .ToList();

        var todayCost = todayPaidOrderIds.Count == 0
            ? 0
            : await _uow.OrderItems.Query()
                .Include(oi => oi.Product)
                .Where(oi => todayPaidOrderIds.Contains(oi.OrderId))
                .SumAsync(oi => oi.Quantity * (oi.Product != null ? oi.Product.CostPrice : 0), ct);

        var todayRevenue = todayOrders.Where(o => o.PaymentStatus == "PAID").Sum(o => o.TotalAmount);

        var today = new TodayStatsDto
        {
            NewOrders = todayOrders.Count,
            Revenue = todayRevenue,
            Profit = todayRevenue - todayCost,
            NewCustomers = await _uow.CustomerProfiles.Query()
                .Where(c => c.CreatedAt >= todayStart).CountAsync(ct),
        };

        // This month stats
        var monthOrders = await _uow.Orders.Query()
            .Where(o => o.CreatedAt >= monthStart && o.OrderStatus != "CANCELLED")
            .ToListAsync(ct);

        var prevMonthRevenue = await _uow.Orders.Query()
            .Where(o => o.CreatedAt >= prevMonthStart && o.CreatedAt < monthStart
                     && o.PaymentStatus == "PAID")
            .SumAsync(o => o.TotalAmount, ct);

        var monthRevenue = monthOrders.Where(o => o.PaymentStatus == "PAID").Sum(o => o.TotalAmount);
        var revenueGrowth = prevMonthRevenue == 0 ? 100
            : Math.Round((monthRevenue - prevMonthRevenue) / prevMonthRevenue * 100, 1);

        var month = new MonthStatsDto
        {
            TotalOrders = monthOrders.Count,
            TotalRevenue = monthRevenue,
            TotalCustomers = await _uow.CustomerProfiles.Query().CountAsync(ct),
            TotalProducts = await _uow.Products.Query().Where(p => p.Status == true).CountAsync(ct),
            RevenueGrowth = revenueGrowth,
        };

        // Revenue chart (last 30 days)
        var chartFrom = now.Date.AddDays(-29);
        var chartOrders = await _uow.Orders.Query()
            .Where(o => o.CreatedAt >= chartFrom && o.PaymentStatus == "PAID")
            .ToListAsync(ct);

        var chart = Enumerable.Range(0, 30)
            .Select(i =>
            {
                var date = chartFrom.AddDays(i);
                var dayOrders = chartOrders.Where(o => o.CreatedAt.Date == date).ToList();
                return new RevenueChartDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Revenue = dayOrders.Sum(o => o.TotalAmount),
                    Orders = dayOrders.Count,
                };
            }).ToList();

        // Top products (this month)
        var topProducts = await _uow.Products.Query()
            .Where(p => p.Status == true)
            .OrderByDescending(p => p.SoldQuantity)
            .Take(5)
            .Select(p => new TopProductDto
            {
                Id = (long)p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                SoldQuantity = (int)p.SoldQuantity,
                Revenue = p.SalePrice * p.SoldQuantity,
            })
            .ToListAsync(ct);

        // Recent orders (last 10)
        var recentOrders = await _uow.Orders.Query()
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new RecentOrderDto
            {
                OrderCode = o.OrderCode,
                CustomerName = o.CustomerName,
                TotalAmount = o.TotalAmount,
                Status = o.OrderStatus,
                CreatedAt = o.CreatedAt,
            })
            .ToListAsync(ct);

        // Order status counts
        var statusCounts = await _uow.Orders.Query()
            .GroupBy(o => o.OrderStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var orderStatus = new OrderStatusCountDto
        {
            Pending = statusCounts.FirstOrDefault(s => s.Status == "PENDING")?.Count ?? 0,
            Confirmed = statusCounts.FirstOrDefault(s => s.Status == "CONFIRMED")?.Count ?? 0,
            Processing = statusCounts.FirstOrDefault(s => s.Status == "PROCESSING")?.Count ?? 0,
            Shipped = statusCounts.FirstOrDefault(s => s.Status == "SHIPPING")?.Count ?? 0,
            Delivered = statusCounts.FirstOrDefault(s => s.Status == "COMPLETED")?.Count ?? 0,
            Cancelled = statusCounts.FirstOrDefault(s => s.Status == "CANCELLED")?.Count ?? 0,
        };

        return new DashboardStatsDto
        {
            Today = today,
            Month = month,
            RevenueChart = chart,
            TopProducts = topProducts,
            RecentOrders = recentOrders,
            OrderStatus = orderStatus,
        };
    }
}
