using API.Products.Interfaces;
using API.Products.Models.Commands;
using API.Products.Models.DTOs;
using API.Products.Models.Queries;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Products.Controllers;

[Audit]
[ApiKey]
[Route("[controller]")]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _service;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService service, ILogger<CategoriesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // GET /api/products/categories/tim-kiem
    [HttpGet("tim-kiem")]
    public async Task<ApiResponse<List<CategoryDto>>> TimKiemDanhMuc([FromQuery] CategoryQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return ApiResponse.Ok(result, "Lấy danh sách danh mục thành công");
    }

    // GET /api/products/categories/cay-phan-cap
    [HttpGet("cay-phan-cap")]
    public async Task<ApiResponse<List<CategoryDto>>> CayPhanCap(CancellationToken ct)
    {
        var result = await _service.GetTreeAsync(ct);
        return ApiResponse.Ok(result, "Lấy cây danh mục thành công");
    }

    // GET /api/products/categories/chi-tiet?id=1
    [HttpGet("chi-tiet")]
    public async Task<ApiResponse<CategoryDto>> ChiTietDanhMuc([FromQuery] long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return ApiResponse.Ok(result, "Lấy chi tiết danh mục thành công");
    }

    // POST /api/products/categories/them-moi-cap-nhat
    [Consumes("multipart/form-data")]
    [HttpPost("them-moi-cap-nhat")]
    public async Task<ApiResponse<CategoryDto>> ThemMoiCapNhatDanhMuc(
        [FromForm] UpsertCategoryRequest request, CancellationToken ct)
    {
        var result = await _service.UpsertAsync(request, ct);
        var msg = request.Id.HasValue ? "Cập nhật danh mục thành công" : "Thêm danh mục thành công";
        return ApiResponse.Ok(result, msg);
    }

    // POST /api/products/categories/xoa
    [HttpPost("xoa")]
    public async Task<ApiResponse> XoaDanhMuc([FromBody] DeleteCategoryRequest request, CancellationToken ct)
    {
        await _service.DeleteAsync(request, ct);
        return ApiResponse.OkEmpty("Xóa danh mục thành công");
    }
}
