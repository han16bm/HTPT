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
[Route("products")]
public class ProductsController : BaseApiController
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    // GET /api/product/products?name=guppy&page=1&pageSize=12
    [HttpGet]
    public async Task<ApiResponse<PagedResult<ProductListDto>>> GetAll([FromQuery] ProductQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return ApiResponse.Ok(result, "Lấy danh sách sản phẩm thành công");
    }

    // GET /api/product/products/featured?top=8
    [HttpGet("featured")]
    public async Task<ApiResponse<List<ProductListDto>>> GetFeatured([FromQuery] int top = 8, CancellationToken ct = default)
    {
        var result = await _service.GetNoiBatAsync(top, ct);
        return ApiResponse.Ok(result, "Lấy sản phẩm nổi bật thành công");
    }

    // GET /api/product/products/slug/ca-betta
    [HttpGet("slug/{slug}")]
    public async Task<ApiResponse<ProductDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var result = await _service.GetBySlugAsync(slug, ct);
        return ApiResponse.Ok(result, "Lấy sản phẩm thành công");
    }

    // GET /api/product/products/1
    [HttpGet("{id:long}")]
    public async Task<ApiResponse<ProductDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return ApiResponse.Ok(result, "Lấy chi tiết sản phẩm thành công");
    }

    // POST /api/product/products
    [Consumes("multipart/form-data")]
    [HttpPost]
    [RequireAdmin]
    public async Task<ApiResponse<ProductDto>> Create([FromForm] UpsertProductRequest request, CancellationToken ct)
    {
        request.Id = null;
        var result = await _service.UpsertAsync(request, ct);
        return ApiResponse.Ok(result, "Thêm sản phẩm thành công");
    }

    // PUT /api/product/products/1
    [Consumes("multipart/form-data")]
    [HttpPut("{id:long}")]
    [RequireAdmin]
    public async Task<ApiResponse<ProductDto>> Update(
        [FromRoute] long id,
        [FromForm] UpsertProductRequest request,
        CancellationToken ct)
    {
        request.Id = id;
        var result = await _service.UpsertAsync(request, ct);
        return ApiResponse.Ok(result, "Cập nhật sản phẩm thành công");
    }

    // DELETE /api/product/products/1
    [HttpDelete("{id:long}")]
    [RequireAdmin]
    public async Task<ApiResponse> Delete([FromRoute] long id, CancellationToken ct)
    {
        await _service.DeleteAsync(new DeleteProductRequest { Id = id }, ct);
        return ApiResponse.OkEmpty("Xóa sản phẩm thành công");
    }

    // GET /api/product/products/health
    [HttpGet("health")]
    public Task<ApiResponse<HealthCheckStatus>> Health()
        => Task.FromResult(ApiResponse.Ok(new HealthCheckStatus("Healthy", "API.Product", DateTime.UtcNow)));
}
