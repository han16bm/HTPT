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
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _service;

    public PaymentsController(IPaymentService service) => _service = service;

    // POST /api/orders/payments/tao-giao-dich
    [HttpPost("tao-giao-dich")]
    public async Task<ApiResponse<PaymentDto>> TaoGiaoDich([FromBody] CreatePaymentRequest request, CancellationToken ct)
    {
        var result = await _service.CreatePaymentAsync(request, ct);
        return ApiResponse.Ok(result, "Tạo giao dịch thành công");
    }

    // GET /api/orders/payments/trang-thai?orderId=1
    [HttpGet("trang-thai")]
    public async Task<ApiResponse<PaymentDto?>> TrangThaiThanhToan([FromQuery] long orderId, CancellationToken ct)
    {
        var result = await _service.GetByOrderIdAsync(orderId, ct);
        return ApiResponse.Ok(result);
    }
}

[Audit]
[ApiKey]
[Route("[controller]")]
public class PromotionsController : BaseApiController
{
    private readonly IPromotionService _service;

    public PromotionsController(IPromotionService service) => _service = service;

    // POST /api/orders/promotions/kiem-tra-ma
    [HttpPost("kiem-tra-ma")]
    public async Task<ApiResponse<PromotionValidationDto>> KiemTraMa([FromBody] ValidatePromotionRequest request, CancellationToken ct)
    {
        var result = await _service.ValidatePromotionAsync(request, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/orders/promotions/tim-kiem
    [HttpGet("tim-kiem")]
    public async Task<ApiResponse<PagedResult<PromotionDto>>> TimKiem([FromQuery] PromotionQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return ApiResponse.Ok(result);
    }

    // POST /api/orders/promotions/them-moi-cap-nhat
    [HttpPost("them-moi-cap-nhat")]
    public async Task<ApiResponse<PromotionDto>> ThemMoiCapNhat([FromBody] UpsertPromotionRequest request, CancellationToken ct)
    {
        var result = await _service.UpsertAsync(request, ct);
        var msg = request.Id.HasValue ? "Cập nhật khuyến mãi thành công" : "Thêm khuyến mãi thành công";
        return ApiResponse.Ok(result, msg);
    }

    // POST /api/orders/promotions/xoa
    [HttpPost("xoa")]
    public async Task<ApiResponse> Xoa([FromBody] DeleteByIdRequest request, CancellationToken ct)
    {
        await _service.DeleteAsync(request.Id, ct);
        return ApiResponse.OkEmpty("Xóa khuyến mãi thành công");
    }
}
