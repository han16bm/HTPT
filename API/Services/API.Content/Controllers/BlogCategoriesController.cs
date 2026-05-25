using API.Content.Interfaces;
using API.Content.Models.Commands;
using API.Content.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Content.Controllers;

[Audit]
[ApiKey]
[Route("blog-categories")]
public class BlogCategoriesController : BaseApiController
{
    private readonly IBlogService _service;

    public BlogCategoriesController(IBlogService service)
    {
        _service = service;
    }

    // GET /api/content/blog-categories
    [HttpGet]
    public async Task<ApiResponse<List<BlogCategoryDto>>> GetAll(CancellationToken ct)
    {
        var result = await _service.GetCategoriesAsync(ct);
        return ApiResponse.Ok(result);
    }

    // POST /api/content/blog-categories
    [HttpPost]
    [RequireAdmin]
    public async Task<ApiResponse<BlogCategoryDto>> Create([FromBody] UpsertBlogCategoryRequest request, CancellationToken ct)
    {
        request.Id = null;
        var result = await _service.UpsertCategoryAsync(request, ct);
        return ApiResponse.Ok(result, "Thêm danh mục thành công");
    }

    // PUT /api/content/blog-categories/1
    [HttpPut("{id:long}")]
    [RequireAdmin]
    public async Task<ApiResponse<BlogCategoryDto>> Update(
        [FromRoute] long id,
        [FromBody] UpsertBlogCategoryRequest request,
        CancellationToken ct)
    {
        request.Id = id;
        var result = await _service.UpsertCategoryAsync(request, ct);
        return ApiResponse.Ok(result, "Cập nhật danh mục thành công");
    }
}
