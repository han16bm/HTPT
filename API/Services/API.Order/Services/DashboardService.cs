using API.Order.Interfaces;
using API.Order.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using netcore.Entities.Interfaces;

namespace API.Order.Services;

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

        var todayRevenue = todayOrders.Where(o => o.PaymentStatus == "PAID").Sum(o => o.TotalAmount);

        var today = new TodayStatsDto
        {
            NewOrders = todayOrders.Count,
            Revenue = todayRevenue,
            Profit = 0,
            NewCustomers = todayOrders
                .Where(o => o.CustomerId.HasValue)
                .Select(o => o.CustomerId!.Value)
                .Distinct()
                .Count(),
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
            TotalCustomers = await _uow.Orders.Query()
                .Where(o => o.CustomerId.HasValue)
                .Select(o => o.CustomerId!.Value)
                .Distinct()
                .CountAsync(ct),
            TotalProducts = await _uow.OrderItems.Query()
                .Select(oi => oi.ProductId)
                .Distinct()
                .CountAsync(ct),
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
        var topProducts = await _uow.OrderItems.Query()
            .GroupBy(oi => new { oi.ProductId, oi.ProductName, oi.ImageUrl })
            .OrderByDescending(g => g.Sum(oi => oi.Quantity))
            .Take(5)
            .Select(g => new TopProductDto
            {
                Id = (long)g.Key.ProductId,
                Name = g.Key.ProductName,
                ImageUrl = g.Key.ImageUrl,
                SoldQuantity = (int)g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.LineTotal),
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
