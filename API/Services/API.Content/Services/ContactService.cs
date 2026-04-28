using API.Content.Interfaces;
using API.Content.Models.Commands;
using API.Content.Models.DTOs;
using API.Content.Models.Queries;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Exceptions;
using netcore.Commons.Models;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;

namespace API.Content.Services;

public class ContactService : IContactService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ContactService> _logger;

    public ContactService(IUnitOfWork uow, ILogger<ContactService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<ContactDto> SubmitAsync(SubmitContactRequest request, CancellationToken ct = default)
    {
        var contact = new ContactMessage
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Subject = request.Subject,
            Message = request.Message,
            Status = "NEW",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _uow.ContactMessages.AddAsync(contact, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Contact submitted from {FullName} ({Email})", request.FullName, request.Email);
        return MapToDto(contact);
    }

    public async Task<PagedResult<ContactDto>> GetAllAsync(ContactQuery query, CancellationToken ct = default)
    {
        var q = _uow.ContactMessages.Query().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(c => c.Status == query.Status);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(c => c.FullName.Contains(query.Keyword) ||
                (c.Email != null && c.Email.Contains(query.Keyword)) ||
                (c.Phone != null && c.Phone.Contains(query.Keyword)));

        q = q.OrderByDescending(c => c.CreatedAt);

        var totalCount = await q.CountAsync(ct);
        var pageSize = Math.Min(query.PageSize, 100);
        var page = Math.Max(query.Page, 1);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => MapToDto(c))
            .ToListAsync(ct);

        return new PagedResult<ContactDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ContactDto> UpdateStatusAsync(UpdateContactStatusRequest request, CancellationToken ct = default)
    {
        var contact = await _uow.ContactMessages.FirstOrDefaultAsync(c => c.Id == (decimal)request.Id, ct)
            ?? throw new NotFoundException("Tin nhắn liên hệ", request.Id);

        contact.Status = request.Status;
        contact.UpdatedAt = DateTime.UtcNow;
        _uow.ContactMessages.Update(contact);
        await _uow.SaveChangesAsync(ct);

        return MapToDto(contact);
    }

    private static ContactDto MapToDto(ContactMessage c) => new()
    {
        Id = (long)c.Id,
        FullName = c.FullName,
        Email = c.Email,
        Phone = c.Phone,
        Subject = c.Subject,
        Message = c.Message,
        Status = c.Status,
        CreatedAt = c.CreatedAt
    };
}
