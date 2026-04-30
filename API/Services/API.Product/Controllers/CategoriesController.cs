using API.Product.Interfaces;
using API.Product.Models.Commands;
using API.Product.Models.DTOs;
using API.Product.Models.Queries;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Product.Controllers;

[Audit]
[ApiKey]
[Route("categories")]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    // GET /api/product/categories
    [HttpGet]
    public async Task<ApiResponse<List<CategoryDto>>> GetAll([FromQuery] CategoryQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return ApiResponse.Ok(result, "Lay danh sach danh muc thanh cong");
    }

    // GET /api/product/categories/tree
    [HttpGet("tree")]
    public async Task<ApiResponse<List<CategoryDto>>> GetTree(CancellationToken ct)
    {
        var result = await _service.GetTreeAsync(ct);
        return ApiResponse.Ok(result, "Lay cay danh muc thanh cong");
    }

    // GET /api/product/categories/1
    [HttpGet("{id:long}")]
    public async Task<ApiResponse<CategoryDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return ApiResponse.Ok(result, "Lay chi tiet danh muc thanh cong");
    }

    // POST /api/product/categories
    [Consumes("multipart/form-data")]
    [HttpPost]
    [RequireAdmin]
    public async Task<ApiResponse<CategoryDto>> Create([FromForm] UpsertCategoryRequest request, CancellationToken ct)
    {
        request.Id = null;
        var result = await _service.UpsertAsync(request, ct);
        return ApiResponse.Ok(result, "Them danh muc thanh cong");
    }

    // PUT /api/product/categories/1
    [Consumes("multipart/form-data")]
    [HttpPut("{id:long}")]
    [RequireAdmin]
    public async Task<ApiResponse<CategoryDto>> Update(
        [FromRoute] long id,
        [FromForm] UpsertCategoryRequest request,
        CancellationToken ct)
    {
        request.Id = id;
        var result = await _service.UpsertAsync(request, ct);
        return ApiResponse.Ok(result, "Cap nhat danh muc thanh cong");
    }

    // DELETE /api/product/categories/1
    [HttpDelete("{id:long}")]
    [RequireAdmin]
    public async Task<ApiResponse> Delete([FromRoute] long id, CancellationToken ct)
    {
        await _service.DeleteAsync(new DeleteCategoryRequest { Id = id }, ct);
        return ApiResponse.OkEmpty("Xoa danh muc thanh cong");
    }
}
