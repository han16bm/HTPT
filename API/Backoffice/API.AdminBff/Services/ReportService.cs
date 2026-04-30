using API.Admin.Interfaces;
using API.Admin.Models.DTOs;
using API.Admin.Models.Queries;
using Microsoft.EntityFrameworkCore;
using netcore.Entities.Interfaces;

namespace API.Admin.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;

    public ReportService(IUnitOfWork uow) => _uow = uow;

    private static (DateTime fromInclusive, DateTime toExclusive) NormalizeDateRange(DateTime? fromDate, DateTime? toDate)
    {
        var fromInclusive = (fromDate ?? DateTime.UtcNow.AddDays(-30)).Date;
        var toExclusive = (toDate ?? DateTime.UtcNow).Date.AddDays(1);
        return (fromInclusive, toExclusive);
    }

    public async Task<ReportDto> GetRevenueReportAsync(ReportQuery query, CancellationToken ct = default)
    {
        var (fromInclusive, toExclusive) = NormalizeDateRange(query.FromDate, query.ToDate);

        var orders = await _uow.Orders.Query()
            .Where(o => o.CreatedAt >= fromInclusive && o.CreatedAt < toExclusive && o.OrderStatus != "CANCELLED")
            .ToListAsync(ct);

        var paidOrders = orders.Where(o => o.PaymentStatus == "PAID").ToList();

        var topProducts = await GetTopProductsAsync(5, fromInclusive, toExclusive.AddTicks(-1), ct);

        // Chart theo ngày / tuần / tháng
        var chartOrders = orders.Where(o => o.PaymentStatus == "PAID").ToList();
        var chart = query.GroupBy switch
        {
            "month" => chartOrders
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new RevenueChartDto
                {
                    Date = $"{g.Key.Year}-{g.Key.Month:00}",
                    Revenue = g.Sum(o => o.TotalAmount),
                    Orders = g.Count(),
                }).ToList(),
            "week" => chartOrders
                .GroupBy(o => $"{o.CreatedAt.Year}-W{System.Globalization.ISOWeek.GetWeekOfYear(o.CreatedAt):00}")
                .OrderBy(g => g.Key)
                .Select(g => new RevenueChartDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    Orders = g.Count(),
                }).ToList(),
            _ => chartOrders
                .GroupBy(o => o.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new RevenueChartDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(o => o.TotalAmount),
                    Orders = g.Count(),
                }).ToList(),
        };

        return new ReportDto
        {
            Period = $"{fromInclusive:dd/MM/yyyy} - {toExclusive.AddDays(-1):dd/MM/yyyy}",
            TotalRevenue = paidOrders.Sum(o => o.TotalAmount),
            TotalOrders = orders.Count,
            TotalProductsSold = 0, // Cần join OrderItems
            AverageOrderValue = paidOrders.Count > 0 ? paidOrders.Average(o => o.TotalAmount) : 0,
            Chart = chart,
            TopProducts = topProducts,
        };
    }

    public async Task<List<TopProductDto>> GetTopProductsAsync(int top, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        return await _uow.Products.Query()
            .Where(p => p.Status == true)
            .OrderByDescending(p => p.SoldQuantity)
            .Take(top)
            .Select(p => new TopProductDto
            {
                Id = (long)p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                SoldQuantity = (int)p.SoldQuantity,
                Revenue = p.SalePrice * p.SoldQuantity,
            })
            .ToListAsync(ct);
    }

    // ===== Report page endpoints (khớp FE) =====

    public async Task<List<RevenueReportRowDto>> GetRevenueChartAsync(ReportQuery query, CancellationToken ct = default)
    {
        var (fromInclusive, toExclusive) = NormalizeDateRange(query.FromDate, query.ToDate);

        var chartOrders = await _uow.Orders.Query()
            .Where(o => o.CreatedAt >= fromInclusive && o.CreatedAt < toExclusive
                        && o.OrderStatus != "CANCELLED" && o.PaymentStatus == "PAID")
            .ToListAsync(ct);

        return query.GroupBy switch
        {
            "month" => chartOrders
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new RevenueReportRowDto
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:00}",
                    Revenue = g.Sum(o => o.TotalAmount),
                    Orders = g.Count(),
                }).ToList(),
            "week" => chartOrders
                .GroupBy(o => $"{o.CreatedAt.Year}-W{System.Globalization.ISOWeek.GetWeekOfYear(o.CreatedAt):00}")
                .OrderBy(g => g.Key)
                .Select(g => new RevenueReportRowDto
                {
                    Period = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    Orders = g.Count(),
                }).ToList(),
            _ => chartOrders
                .GroupBy(o => o.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new RevenueReportRowDto
                {
                    Period = g.Key.ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(o => o.TotalAmount),
                    Orders = g.Count(),
                }).ToList(),
        };
    }

    public async Task<List<TopProductReportRowDto>> GetTopProductReportAsync(int top, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        return await _uow.Products.Query()
            .Where(p => p.Status == true)
            .OrderByDescending(p => p.SoldQuantity)
            .Take(top)
            .Select(p => new TopProductReportRowDto
            {
                ProductId = (long)p.Id,
                ProductName = p.Name,
                Sold = (int)p.SoldQuantity,
                Revenue = p.SalePrice * p.SoldQuantity,
            })
            .ToListAsync(ct);
    }

    public async Task<OrderSummaryReportDto> GetOrderSummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default)
    {
        var (fromInclusive, toExclusive) = NormalizeDateRange(fromDate, toDate);

        var orders = await _uow.Orders.Query()
            .Where(o => o.CreatedAt >= fromInclusive && o.CreatedAt < toExclusive)
            .ToListAsync(ct);

        var byStatus = orders
            .GroupBy(o => o.OrderStatus)
            .Select(g => new OrderStatusSummaryDto
            {
                Status = g.Key,
                Count = g.Count(),
                Revenue = g.Where(o => o.PaymentStatus == "PAID").Sum(o => o.TotalAmount),
            })
            .ToList();

        return new OrderSummaryReportDto
        {
            TotalOrders = orders.Count,
            ByStatus = byStatus,
        };
    }
}
