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
[Route("[controller]")]
public class BlogCategoriesController : BaseApiController
{
    private readonly IBlogService _service;

    public BlogCategoriesController(IBlogService service) => _service = service;

    // GET /api/content/blog-categories/danh-sach
    [HttpGet("danh-sach")]
    public async Task<ApiResponse<List<BlogCategoryDto>>> DanhSach(CancellationToken ct)
    {
        var result = await _service.GetCategoriesAsync(ct);
        return ApiResponse.Ok(result);
    }

    // POST /api/content/blog-categories/them-moi-cap-nhat
    [HttpPost("them-moi-cap-nhat")]
    public async Task<ApiResponse<BlogCategoryDto>> ThemMoiCapNhat([FromBody] UpsertBlogCategoryRequest request, CancellationToken ct)
    {
        var result = await _service.UpsertCategoryAsync(request, ct);
        var msg = request.Id.HasValue ? "Cập nhật danh mục thành công" : "Thêm danh mục thành công";
        return ApiResponse.Ok(result, msg);
    }
}
