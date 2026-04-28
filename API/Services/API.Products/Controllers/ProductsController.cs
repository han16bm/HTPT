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
public class ProductsController : BaseApiController
{
    private readonly IProductService _service;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService service, ILogger<ProductsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // GET /api/products/products/tim-kiem?name=guppy&page=1&pageSize=12
    [HttpGet("tim-kiem")]
    public async Task<ApiResponse<PagedResult<ProductListDto>>> TimKiemSanPham([FromQuery] ProductQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return ApiResponse.Ok(result, "Lấy danh sách sản phẩm thành công");
    }

    // GET /api/products/products/chi-tiet?id=1
    [HttpGet("chi-tiet")]
    public async Task<ApiResponse<ProductDto>> ChiTietSanPham([FromQuery] long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return ApiResponse.Ok(result, "Lấy chi tiết sản phẩm thành công");
    }

    // GET /api/products/products/theo-slug?slug=ca-betta
    [HttpGet("theo-slug")]
    public async Task<ApiResponse<ProductDto>> SanPhamTheoSlug([FromQuery] string slug, CancellationToken ct)
    {
        var result = await _service.GetBySlugAsync(slug, ct);
        return ApiResponse.Ok(result, "Lấy sản phẩm thành công");
    }

    // GET /api/products/products/san-pham-noi-bat?top=8
    [HttpGet("san-pham-noi-bat")]
    public async Task<ApiResponse<List<ProductListDto>>> SanPhamNoiBat([FromQuery] int top = 8, CancellationToken ct = default)
    {
        var result = await _service.GetNoiBatAsync(top, ct);
        return ApiResponse.Ok(result, "Lấy sản phẩm nổi bật thành công");
    }

    // POST /api/products/products/them-moi-cap-nhat
    [Consumes("multipart/form-data")]
    [HttpPost("them-moi-cap-nhat")]
    public async Task<ApiResponse<ProductDto>> ThemMoiCapNhatSanPham(
        [FromForm] UpsertProductRequest request, CancellationToken ct)
    {
        var result = await _service.UpsertAsync(request, ct);
        var msg = request.Id.HasValue ? "Cập nhật sản phẩm thành công" : "Thêm sản phẩm thành công";
        return ApiResponse.Ok(result, msg);
    }

    // POST /api/products/products/xoa
    [HttpPost("xoa")]
    public async Task<ApiResponse> XoaSanPham([FromBody] DeleteProductRequest request, CancellationToken ct)
    {
        await _service.DeleteAsync(request, ct);
        return ApiResponse.OkEmpty("Xóa sản phẩm thành công");
    }

    // GET /api/products/products/healthcheck
    [HttpGet("healthcheck")]
    public Task<ApiResponse<HealthCheckStatus>> HealthCheck()
        => Task.FromResult(ApiResponse.Ok(new HealthCheckStatus("Healthy", "API.Products", DateTime.UtcNow)));
}
