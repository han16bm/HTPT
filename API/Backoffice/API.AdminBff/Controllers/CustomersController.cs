using API.Admin.Interfaces;
using API.Admin.Models.Commands;
using API.Admin.Models.DTOs;
using API.Admin.Models.Queries;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Admin.Controllers;

[Audit]
[ApiKey]
[Route("[controller]")]
public class CustomersController : BaseApiController
{
    private readonly ICustomerAdminService _service;

    public CustomersController(ICustomerAdminService service) => _service = service;

    // GET /api/admin/customers/tim-kiem
    [HttpGet("tim-kiem")]
    public async Task<ApiResponse<PagedResult<CustomerListDto>>> TimKiem([FromQuery] CustomerQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/admin/customers/chi-tiet?id=1
    [HttpGet("chi-tiet")]
    public async Task<ApiResponse<CustomerDetailDto>> ChiTiet([FromQuery] long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return ApiResponse.Ok(result);
    }

    // POST /api/admin/customers/them-moi-cap-nhat
    [HttpPost("them-moi-cap-nhat")]
    public async Task<ApiResponse<CustomerDetailDto>> ThemMoiCapNhat([FromBody] CustomerUpsertRequest request, CancellationToken ct)
    {
        var result = await _service.UpsertAsync(request, ct);
        return ApiResponse.Ok(result, "Cập nhật khách hàng thành công");
    }

    // POST /api/admin/customers/tao-khach-vang-lai
    [HttpPost("tao-khach-vang-lai")]
    public async Task<ApiResponse<CustomerDetailDto>> TaoKhachVangLai([FromBody] CustomerWalkInRequest request, CancellationToken ct)
    {
        var result = await _service.CreateWalkInAsync(request, ct);
        return ApiResponse.Ok(result, "Tạo khách hàng vãng lai thành công");
    }
}
