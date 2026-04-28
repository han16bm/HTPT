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
public class ContactController : BaseApiController
{
    private readonly IContactService _service;

    public ContactController(IContactService service) => _service = service;

    // POST /api/content/contact/gui-lien-he
    [HttpPost("gui-lien-he")]
    public async Task<ApiResponse<ContactDto>> GuiLienHe([FromBody] SubmitContactRequest request, CancellationToken ct)
    {
        var result = await _service.SubmitAsync(request, ct);
        return ApiResponse.Ok(result, "Gửi liên hệ thành công. Chúng tôi sẽ phản hồi sớm nhất!");
    }

    // GET /api/content/contact/tim-kiem  (Admin)
    [HttpGet("tim-kiem")]
    public async Task<ApiResponse<PagedResult<ContactDto>>> TimKiem([FromQuery] ContactQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return ApiResponse.Ok(result);
    }

    // POST /api/content/contact/cap-nhat-trang-thai  (Admin)
    [HttpPost("cap-nhat-trang-thai")]
    public async Task<ApiResponse<ContactDto>> CapNhatTrangThai([FromBody] UpdateContactStatusRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateStatusAsync(request, ct);
        return ApiResponse.Ok(result, "Cập nhật trạng thái thành công");
    }
}
