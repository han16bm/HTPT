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
[Route("payments")]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _service;

    public PaymentsController(IPaymentService service)
    {
        _service = service;
    }

    // POST /api/order/payments
    [HttpPost]
    public async Task<ApiResponse<PaymentDto>> Create([FromBody] CreatePaymentRequest request, CancellationToken ct)
    {
        var result = await _service.CreatePaymentAsync(request, ct);
        return ApiResponse.Ok(result, "Tạo giao dịch thành công");
    }

    // GET /api/order/payments/order/1
    [HttpGet("order/{orderId:long}")]
    public async Task<ApiResponse<PaymentDto?>> GetByOrderId([FromRoute] long orderId, CancellationToken ct)
    {
        var result = await _service.GetByOrderIdAsync(orderId, ct);
        return ApiResponse.Ok(result);
    }
}

[Audit]
[ApiKey]
[Route("promotions")]
public class PromotionsController : BaseApiController
{
    private readonly IPromotionService _service;

    public PromotionsController(IPromotionService service)
    {
        _service = service;
    }

    // GET /api/order/promotions
    [HttpGet]
    public async Task<ApiResponse<PagedResult<PromotionDto>>> GetAll([FromQuery] PromotionQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return ApiResponse.Ok(result);
    }

    // POST /api/order/promotions
    [HttpPost]
    [RequireAdmin]
    public async Task<ApiResponse<PromotionDto>> Create([FromBody] UpsertPromotionRequest request, CancellationToken ct)
    {
        request.Id = null;
        var result = await _service.UpsertAsync(request, ct);
        return ApiResponse.Ok(result, "Thêm khuyến mãi thành công");
    }

    // PUT /api/order/promotions/1
    [HttpPut("{id:long}")]
    [RequireAdmin]
    public async Task<ApiResponse<PromotionDto>> Update(
        [FromRoute] long id,
        [FromBody] UpsertPromotionRequest request,
        CancellationToken ct)
    {
        request.Id = id;
        var result = await _service.UpsertAsync(request, ct);
        return ApiResponse.Ok(result, "Cập nhật khuyến mãi thành công");
    }

    // DELETE /api/order/promotions/1
    [HttpDelete("{id:long}")]
    [RequireAdmin]
    public async Task<ApiResponse> Delete([FromRoute] long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return ApiResponse.OkEmpty("Xóa khuyến mãi thành công");
    }

    // POST /api/order/promotions/validate
    [HttpPost("validate")]
    public async Task<ApiResponse<PromotionValidationDto>> Validate([FromBody] ValidatePromotionRequest request, CancellationToken ct)
    {
        var result = await _service.ValidatePromotionAsync(request, ct);
        return ApiResponse.Ok(result);
    }
}
