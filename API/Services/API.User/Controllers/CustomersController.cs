using API.User.Interfaces;
using API.User.Models.Commands;
using API.User.Models.DTOs;
using API.User.Models.Queries;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.User.Controllers;

[Audit]
[ApiKey]
[RequireAdmin]
[Route("customers")]
public class CustomersController : BaseApiController
{
    private readonly ICustomerAdminService _service;

    public CustomersController(ICustomerAdminService service)
    {
        _service = service;
    }

    // GET /api/user/customers
    [HttpGet]
    public async Task<ApiResponse<PagedResult<CustomerListDto>>> GetAll([FromQuery] CustomerQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/user/customers/1
    [HttpGet("{id:long}")]
    public async Task<ApiResponse<CustomerDetailDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return ApiResponse.Ok(result);
    }

    // POST /api/user/customers
    [HttpPost]
    public async Task<ApiResponse<CustomerDetailDto>> Create(
        [FromBody] CustomerUpsertRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return ApiResponse.Ok(result, "Tạo khách hàng thành công");
    }

    // PUT /api/user/customers/1
    [HttpPut("{id:long}")]
    public async Task<ApiResponse<CustomerDetailDto>> Update(
        [FromRoute] long id,
        [FromBody] CustomerUpsertRequest request,
        CancellationToken ct)
    {
        request.Id = id;
        var result = await _service.UpsertAsync(request, ct);
        return ApiResponse.Ok(result, "Cập nhật khách hàng thành công");
    }

    // POST /api/user/customers/walk-ins
    [HttpPost("walk-ins")]
    public async Task<ApiResponse<CustomerDetailDto>> CreateWalkIn([FromBody] CustomerWalkInRequest request, CancellationToken ct)
    {
        var result = await _service.CreateWalkInAsync(request, ct);
        return ApiResponse.Ok(result, "Tạo khách hàng vãng lai thành công");
    }

    // DELETE /api/user/customers/1
    [HttpDelete("{id:long}")]
    public async Task<ApiResponse> Delete([FromRoute] long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return ApiResponse.OkEmpty("Xóa khách hàng thành công");
    }
}
