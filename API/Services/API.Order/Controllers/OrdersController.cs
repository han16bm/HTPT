using API.Order.Interfaces;
using API.Order.Models.Commands;
using API.Order.Models.DTOs;
using API.Order.Models.Queries;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Order.Controllers;

[Audit]
[ApiKey]
[Route("orders")]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service) => _service = service;

    // GET /api/order/orders
    [HttpGet]
    [RequireAdmin]
    public async Task<ApiResponse<PagedResult<OrderListDto>>> GetAll([FromQuery] OrderQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllOrdersAsync(query, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/order/orders/me
    [HttpGet("me")]
    public async Task<ApiResponse<PagedResult<OrderListDto>>> GetMine([FromQuery] OrderQuery query, CancellationToken ct)
    {
        var result = await _service.GetMyOrdersAsync(GetUserId(), query, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/order/orders/DH2024...
    [HttpGet("{orderCode}")]
    public async Task<ApiResponse<OrderDto>> GetByCode([FromRoute] string orderCode, CancellationToken ct)
    {
        var result = await _service.GetByOrderCodeAsync(orderCode, ct);
        return ApiResponse.Ok(result);
    }

    // POST /api/order/orders
    [HttpPost]
    public async Task<ApiResponse<OrderDto>> CreateFromCart([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var result = await _service.CreateOrderFromCartAsync(GetUserId(), request, ct);
        return ApiResponse.Ok(result, "Đặt hàng thành công");
    }

    // POST /api/order/orders/direct
    [HttpPost("direct")]
    public async Task<ApiResponse<OrderDto>> CreateDirect([FromBody] DirectOrderRequest request, CancellationToken ct)
    {
        var result = await _service.CreateDirectOrderAsync(request, ct);
        return ApiResponse.Ok(result, "Tạo đơn hàng thành công");
    }

    // PATCH /api/order/orders/DH2024.../status
    [HttpPatch("{orderCode}/status")]
    [RequireAdmin]
    public async Task<ApiResponse<OrderDto>> UpdateStatus(
        [FromRoute] string orderCode,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken ct)
    {
        request.OrderCode = orderCode;
        var result = await _service.UpdateStatusAsync(request, ct);
        return ApiResponse.Ok(result, "Cập nhật trạng thái thành công");
    }

    // DELETE /api/order/orders/DH2024...?reason=...
    [HttpDelete("{orderCode}")]
    public async Task<ApiResponse<OrderDto>> Cancel(
        [FromRoute] string orderCode,
        [FromQuery] string? reason,
        CancellationToken ct)
    {
        var result = await _service.CancelOrderAsync(GetUserId(), new CancelOrderRequest
        {
            OrderCode = orderCode,
            Reason = reason
        }, ct);

        return ApiResponse.Ok(result, "Hủy đơn hàng thành công");
    }

    // GET /api/order/orders/health
    [HttpGet("health")]
    public Task<ApiResponse<HealthCheckStatus>> Health()
        => Task.FromResult(ApiResponse.Ok(new HealthCheckStatus("Healthy", "API.Order", DateTime.UtcNow)));
}
