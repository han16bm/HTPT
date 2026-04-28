using API.Admin.Interfaces;
using API.Admin.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using netcore.Entities.Interfaces;

namespace API.Admin.Services;

public class SalesService : ISalesService
{
    private readonly IUnitOfWork _uow;

    public SalesService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<RevenueChartDto>> GetSalesStatsAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        var orders = await _uow.Orders.Query()
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate && o.PaymentStatus == "PAID")
            .ToListAsync(ct);

        var days = (int)(toDate.Date - fromDate.Date).TotalDays + 1;
        return Enumerable.Range(0, days)
            .Select(i =>
            {
                var date = fromDate.Date.AddDays(i);
                var dayOrders = orders.Where(o => o.CreatedAt.Date == date).ToList();
                return new RevenueChartDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Revenue = dayOrders.Sum(o => o.TotalAmount),
                    Orders = dayOrders.Count,
                };
            }).ToList();
    }
}
