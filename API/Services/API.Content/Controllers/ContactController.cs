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
[Route("contacts")]
public class ContactController : BaseApiController
{
    private readonly IContactService _service;

    public ContactController(IContactService service)
    {
        _service = service;
    }

    // GET /api/content/contacts
    [HttpGet]
    public async Task<ApiResponse<PagedResult<ContactDto>>> GetAll([FromQuery] ContactQuery query, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return ApiResponse.Ok(result);
    }

    // POST /api/content/contacts
    [HttpPost]
    public async Task<ApiResponse<ContactDto>> Create([FromBody] SubmitContactRequest request, CancellationToken ct)
    {
        var result = await _service.SubmitAsync(request, ct);
        return ApiResponse.Ok(result, "Gửi liên hệ thành công");
    }

    // PATCH /api/content/contacts/1/status
    [HttpPatch("{id:long}/status")]
    public async Task<ApiResponse<ContactDto>> UpdateStatus(
        [FromRoute] long id,
        [FromBody] UpdateContactStatusRequest request,
        CancellationToken ct)
    {
        request.Id = id;
        var result = await _service.UpdateStatusAsync(request, ct);
        return ApiResponse.Ok(result, "Cập nhật trạng thái thành công");
    }
}
