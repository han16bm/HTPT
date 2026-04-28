using API.Content.Interfaces;
using API.Content.Models.Commands;
using API.Content.Models.DTOs;
using API.Content.Models.Queries;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Content.Controllers;

[Audit]
[ApiKey]
[Route("[controller]")]
public class BlogController : BaseApiController
{
    private readonly IBlogService _service;

    public BlogController(IBlogService service) => _service = service;

    // GET /api/content/blog/tim-kiem
    [HttpGet("tim-kiem")]
    public async Task<ApiResponse<PagedResult<BlogPostListDto>>> TimKiem([FromQuery] BlogQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/content/blog/chi-tiet-theo-slug?slug=ten-bai-viet
    [HttpGet("chi-tiet-theo-slug")]
    public async Task<ApiResponse<BlogPostDto>> ChiTiet([FromQuery] string slug, CancellationToken ct)
    {
        var result = await _service.GetBySlugAsync(slug, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/content/blog/theo-id?id=1
    [HttpGet("theo-id")]
    public async Task<ApiResponse<BlogPostDto>> TheoId([FromQuery] long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return ApiResponse.Ok(result);
    }

    // POST /api/content/blog/them-moi-cap-nhat
    [Consumes("multipart/form-data")]
    [HttpPost("them-moi-cap-nhat")]
    public async Task<ApiResponse<BlogPostDto>> ThemMoiCapNhat([FromForm] UpsertBlogPostRequest request, CancellationToken ct)
    {
        var result = await _service.UpsertAsync(request, GetUserId(), ct);
        var msg = request.Id.HasValue ? "Cập nhật bài viết thành công" : "Thêm bài viết thành công";
        return ApiResponse.Ok(result, msg);
    }

    // POST /api/content/blog/xoa
    [HttpPost("xoa")]
    public async Task<ApiResponse> Xoa([FromBody] DeleteByIdRequest request, CancellationToken ct)
    {
        await _service.DeleteAsync(request.Id, ct);
        return ApiResponse.OkEmpty("Xóa bài viết thành công");
    }

    // GET /api/content/blog/healthcheck
    [HttpGet("healthcheck")]
    public Task<ApiResponse<HealthCheckStatus>> Healthcheck()
        => Task.FromResult(ApiResponse.Ok(new HealthCheckStatus("Healthy", "API.Content", DateTime.UtcNow)));
}
