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
[Route("blogs")]
public class BlogController : BaseApiController
{
    private readonly IBlogService _service;

    public BlogController(IBlogService service) => _service = service;

    // GET /api/content/blogs
    [HttpGet]
    public async Task<ApiResponse<PagedResult<BlogPostListDto>>> GetAll([FromQuery] BlogQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/content/blogs/1
    [HttpGet("{id:long}")]
    public async Task<ApiResponse<BlogPostDto>> GetById([FromRoute] long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return ApiResponse.Ok(result);
    }

    // GET /api/content/blogs/slug/ten-bai-viet
    [HttpGet("slug/{slug}")]
    public async Task<ApiResponse<BlogPostDto>> GetBySlug([FromRoute] string slug, CancellationToken ct)
    {
        var result = await _service.GetBySlugAsync(slug, ct);
        return ApiResponse.Ok(result);
    }

    // POST /api/content/blogs
    [Consumes("multipart/form-data")]
    [HttpPost]
    [RequireAdmin]
    public async Task<ApiResponse<BlogPostDto>> Create([FromForm] UpsertBlogPostRequest request, CancellationToken ct)
    {
        request.Id = null;
        var result = await _service.UpsertAsync(request, GetUserId(), ct);
        return ApiResponse.Ok(result, "Them bai viet thanh cong");
    }

    // PUT /api/content/blogs/1
    [Consumes("multipart/form-data")]
    [HttpPut("{id:long}")]
    [RequireAdmin]
    public async Task<ApiResponse<BlogPostDto>> Update(
        [FromRoute] long id,
        [FromForm] UpsertBlogPostRequest request,
        CancellationToken ct)
    {
        request.Id = id;
        var result = await _service.UpsertAsync(request, GetUserId(), ct);
        return ApiResponse.Ok(result, "Cap nhat bai viet thanh cong");
    }

    // DELETE /api/content/blogs/1
    [HttpDelete("{id:long}")]
    [RequireAdmin]
    public async Task<ApiResponse> Delete([FromRoute] long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return ApiResponse.OkEmpty("Xoa bai viet thanh cong");
    }

    // GET /api/content/blogs/health
    [HttpGet("health")]
    public Task<ApiResponse<HealthCheckStatus>> Health()
        => Task.FromResult(ApiResponse.Ok(new HealthCheckStatus("Healthy", "API.Content", DateTime.UtcNow)));
}
