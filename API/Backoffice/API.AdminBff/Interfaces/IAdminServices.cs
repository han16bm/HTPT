using API.Admin.Models.Commands;
using API.Admin.Models.DTOs;

namespace API.Admin.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default);
}

public interface ISalesService
{
    // POS — tạo đơn hàng tại quầy (dùng lại OrderService logic qua URL hoặc inject)
    // Thống kê doanh thu theo ngày
    Task<List<RevenueChartDto>> GetSalesStatsAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default);
}

public interface ICustomerAdminService
{
    Task<netcore.Commons.Models.PagedResult<CustomerListDto>> GetAllAsync(Models.Queries.CustomerQuery query, CancellationToken ct = default);
    Task<CustomerDetailDto> GetByIdAsync(long customerId, CancellationToken ct = default);
    Task<CustomerDetailDto> UpsertAsync(CustomerUpsertRequest request, CancellationToken ct = default);
    Task<CustomerDetailDto> CreateWalkInAsync(CustomerWalkInRequest request, CancellationToken ct = default);
}

public interface IReportService
{
    Task<ReportDto> GetRevenueReportAsync(Models.Queries.ReportQuery query, CancellationToken ct = default);
    Task<List<TopProductDto>> GetTopProductsAsync(int top, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<List<RevenueReportRowDto>> GetRevenueChartAsync(Models.Queries.ReportQuery query, CancellationToken ct = default);
    Task<List<TopProductReportRowDto>> GetTopProductReportAsync(int top, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<OrderSummaryReportDto> GetOrderSummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
}
