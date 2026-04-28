using API.Orders.Interfaces;
using API.Orders.Models.Commands;
using API.Orders.Models.DTOs;
using API.Orders.Models.Queries;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Orders.Controllers;

[Audit]
[ApiKey]
[Route("[controller]")]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service) => _service = service;

    // POST /api/orders/orders/dat-hang
    [HttpPost("dat-hang")]
    public async Task<ApiResponse<OrderDto>> DatHang([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var result = await _service.CreateOrderFromCartAsync(GetUserId(), request, ct);
        return ApiResponse.Ok(result, "Đặt hàng thành công");
    }

    // POST /api/orders/orders/dat-hang-truc-tiep  (POS/Admin)
    [HttpPost("dat-hang-truc-tiep")]
    public async Task<ApiResponse<OrderDto>> DatHangTrucTiep([FromBody] DirectOrderRequest request, CancellationToken ct)
    {
        var result = await _service.CreateDirectOrderAsync(request, ct);
        return ApiResponse.Ok(result, "Tạo đơn hàng thành công");
    }

    // GET /api/orders/orders/don-hang-cua-toi
    [HttpGet("don-hang-cua-toi")]
    public async Task<ApiResponse<PagedResult<OrderListDto>>> DonHangCuaToi([FromQuery] OrderQuery query, CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _service.GetMyOrdersAsync(userId, query, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/orders/orders/tim-kiem  (Admin)
    [HttpGet("tim-kiem")]
    public async Task<ApiResponse<PagedResult<OrderListDto>>> TatCaDonHang([FromQuery] OrderQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllOrdersAsync(query, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/orders/orders/chi-tiet?order_code=DH2024...
    [HttpGet("chi-tiet")]
    public async Task<ApiResponse<OrderDto>> ChiTietDonHang([FromQuery(Name = "order_code")] string orderCode, CancellationToken ct)
    {
        var result = await _service.GetByOrderCodeAsync(orderCode, ct);
        return ApiResponse.Ok(result);
    }

    // POST /api/orders/orders/huy-don
    [HttpPost("huy-don")]
    public async Task<ApiResponse<OrderDto>> HuyDon([FromBody] CancelOrderRequest request, CancellationToken ct)
    {
        var result = await _service.CancelOrderAsync(GetUserId(), request, ct);
        return ApiResponse.Ok(result, "Hủy đơn hàng thành công");
    }

    // POST /api/orders/orders/cap-nhat-trang-thai  (Admin)
    [HttpPost("cap-nhat-trang-thai")]
    public async Task<ApiResponse<OrderDto>> CapNhatTrangThai([FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateStatusAsync(request, ct);
        return ApiResponse.Ok(result, "Cập nhật trạng thái thành công");
    }

    // GET /api/orders/orders/healthcheck
    [HttpGet("healthcheck")]
    public Task<ApiResponse<HealthCheckStatus>> Healthcheck()
        => Task.FromResult(ApiResponse.Ok(new HealthCheckStatus("Healthy", "API.Orders", DateTime.UtcNow)));
}
