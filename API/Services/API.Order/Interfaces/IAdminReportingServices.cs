using API.Order.Models.DTOs;
using API.Order.Models.Queries;

namespace API.Order.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default);
}

public interface ISalesService
{
    Task<List<RevenueChartDto>> GetSalesStatsAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default);
}

public interface IReportService
{
    Task<ReportDto> GetRevenueReportAsync(ReportQuery query, CancellationToken ct = default);
    Task<List<TopProductDto>> GetTopProductsAsync(int top, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<List<RevenueReportRowDto>> GetRevenueChartAsync(ReportQuery query, CancellationToken ct = default);
    Task<List<TopProductReportRowDto>> GetTopProductReportAsync(int top, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<OrderSummaryReportDto> GetOrderSummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
}
