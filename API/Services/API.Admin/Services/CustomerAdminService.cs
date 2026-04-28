using API.Admin.Interfaces;
using API.Admin.Models.Commands;
using API.Admin.Models.DTOs;
using API.Admin.Models.Queries;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Exceptions;
using netcore.Commons.Models;
using netcore.Entities.Interfaces;

namespace API.Admin.Services;

public class CustomerAdminService : ICustomerAdminService
{
    private readonly IUnitOfWork _uow;

    public CustomerAdminService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<CustomerListDto>> GetAllAsync(
        CustomerQuery query, CancellationToken ct = default)
    {
        var q = _uow.CustomerProfiles.Query().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(c =>
                c.FullName.Contains(query.Keyword) ||
                c.Phone.Contains(query.Keyword) ||
                (c.Email != null && c.Email.Contains(query.Keyword)));

        if (query.IsActive.HasValue)
            q = q.Where(c => c.Status == query.IsActive.Value);

        q = q.OrderByDescending(c => c.CreatedAt);

        var totalCount = await q.CountAsync(ct);
        var pageSize = Math.Min(query.PageSize, 100);
        var page = Math.Max(query.Page, 1);

        var profiles = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var profileIds = profiles.Select(p => p.Id).ToList();

        var orderStats = await _uow.Orders.Query()
            .Where(o => o.CustomerId != null && profileIds.Contains(o.CustomerId.Value))
            .GroupBy(o => o.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key!.Value,
                TotalOrders = g.Count(),
                TotalSpent = g.Where(o => o.PaymentStatus == "PAID").Sum(o => o.TotalAmount),
            })
            .ToListAsync(ct);

        var items = profiles.Select(p =>
        {
            var stats = orderStats.FirstOrDefault(s => s.CustomerId == p.Id);
            return new CustomerListDto
            {
                Id = (long)p.Id,
                CustomerCode = p.CustomerCode,
                FullName = p.FullName,
                Phone = p.Phone,
                Email = p.Email,
                Gender = p.Gender,
                IsActive = p.Status ?? true,
                TotalOrders = stats?.TotalOrders ?? 0,
                TotalSpent = stats?.TotalSpent ?? 0,
                CreatedAt = p.CreatedAt,
            };
        }).ToList();

        return new PagedResult<CustomerListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<CustomerDetailDto> GetByIdAsync(long customerId, CancellationToken ct = default)
    {
        var profile = await _uow.CustomerProfiles.Query()
            .Include(p => p.User)
            .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(p => p.Id == (decimal)customerId, ct)
            ?? throw new NotFoundException("Khách hàng", customerId);

        var orders = await _uow.Orders.Query()
            .Where(o => o.CustomerId == profile.Id)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .ToListAsync(ct);

        var allOrders = await _uow.Orders.Query()
            .Where(o => o.CustomerId == profile.Id)
            .ToListAsync(ct);

        return new CustomerDetailDto
        {
            Id = (long)profile.Id,
            CustomerCode = profile.CustomerCode,
            Username = profile.User?.Username,
            FullName = profile.FullName,
            Phone = profile.Phone,
            Email = profile.Email,
            Gender = profile.Gender,
            DateOfBirth = profile.DateOfBirth,
            IsActive = profile.Status ?? true,
            IsAdmin = profile.User?.IsAdmin == true || profile.User?.Role?.Code == "ADMIN",
            TotalOrders = allOrders.Count,
            TotalSpent = allOrders.Where(o => o.PaymentStatus == "PAID").Sum(o => o.TotalAmount),
            CreatedAt = profile.CreatedAt,
            RecentOrders = orders.Select(o => new CustomerOrderSummaryDto
            {
                OrderCode = o.OrderCode,
                Status = o.OrderStatus,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
            }).ToList(),
        };
    }

    public async Task<CustomerDetailDto> UpsertAsync(CustomerUpsertRequest request, CancellationToken ct = default)
    {
        if (request.Id <= 0)
            throw new MessageException("Chỉ hỗ trợ cập nhật khách hàng đã tồn tại.");

        var profile = await _uow.CustomerProfiles.Query()
            .Include(p => p.User)
            .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(p => p.Id == (decimal)request.Id, ct)
            ?? throw new NotFoundException("Khách hàng", request.Id);

        var user = profile.User
            ?? throw new MessageException("Khách hàng này chưa liên kết tài khoản người dùng.");

        var normalizedFullName = request.FullName.Trim();
        var normalizedPhone = request.Phone.Trim();
        var normalizedEmail = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        var normalizedGender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim();
        var isActive = request.Status == 1;

        if (string.IsNullOrWhiteSpace(normalizedFullName))
            throw new MessageException("Họ và tên không được để trống.");

        if (string.IsNullOrWhiteSpace(normalizedPhone))
            throw new MessageException("Số điện thoại không được để trống.");

        var duplicateProfilePhone = await _uow.CustomerProfiles.AnyAsync(
            p => p.Phone == normalizedPhone && p.Id != profile.Id, ct);
        if (duplicateProfilePhone)
            throw new MessageException("Số điện thoại đã được sử dụng.");

        var duplicateUserPhone = await _uow.Users.AnyAsync(
            u => u.Phone == normalizedPhone && u.Id != user.Id, ct);
        if (duplicateUserPhone)
            throw new MessageException("Số điện thoại đã được sử dụng.");

        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            var duplicateProfileEmail = await _uow.CustomerProfiles.AnyAsync(
                p => p.Email == normalizedEmail && p.Id != profile.Id, ct);
            if (duplicateProfileEmail)
                throw new MessageException("Email đã được sử dụng.");

            var duplicateUserEmail = await _uow.Users.AnyAsync(
                u => u.Email == normalizedEmail && u.Id != user.Id, ct);
            if (duplicateUserEmail)
                throw new MessageException("Email đã được sử dụng.");
        }

        var targetRoleCode = request.IsAdmin ? "ADMIN" : "CUSTOMER";
        var targetRole = await _uow.Roles.FirstOrDefaultAsync(r => r.Code == targetRoleCode, ct)
            ?? throw new MessageException($"Không tìm thấy role '{targetRoleCode}'.");

        profile.FullName = normalizedFullName;
        profile.Phone = normalizedPhone;
        profile.Email = normalizedEmail;
        profile.DateOfBirth = request.DateOfBirth;
        profile.Gender = normalizedGender;
        profile.Status = isActive;
        profile.UpdatedAt = DateTime.UtcNow;

        user.FullName = normalizedFullName;
        user.Phone = normalizedPhone;
        user.Email = normalizedEmail;
        user.Status = isActive;
        user.RoleId = targetRole.Id;
        user.Role = targetRole;
        user.IsAdmin = request.IsAdmin;
        user.UpdatedAt = DateTime.UtcNow;

        _uow.CustomerProfiles.Update(profile);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(request.Id, ct);
    }

    public async Task<CustomerDetailDto> CreateWalkInAsync(CustomerWalkInRequest request, CancellationToken ct = default)
    {
        var normalizedFullName = request.FullName.Trim();
        var normalizedPhone = request.Phone.Trim();
        var normalizedEmail = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();

        if (string.IsNullOrWhiteSpace(normalizedFullName))
            throw new netcore.Commons.Exceptions.MessageException("Họ và tên không được để trống.");

        if (string.IsNullOrWhiteSpace(normalizedPhone))
            throw new netcore.Commons.Exceptions.MessageException("Số điện thoại không được để trống.");

        if (await _uow.CustomerProfiles.AnyAsync(p => p.Phone == normalizedPhone, ct))
            throw new netcore.Commons.Exceptions.MessageException("Số điện thoại đã được sử dụng bởi khách hàng khác.");

        if (normalizedEmail is not null && await _uow.CustomerProfiles.AnyAsync(p => p.Email == normalizedEmail, ct))
            throw new netcore.Commons.Exceptions.MessageException("Email đã được sử dụng bởi khách hàng khác.");

        var profile = new netcore.Entities.Entities.CustomerProfile
        {
            UserId = null, // walk-in: no linked account
            CustomerCode = $"KH{DateTime.UtcNow:yyyyMMddHHmmss}",
            FullName = normalizedFullName,
            Phone = normalizedPhone,
            Email = normalizedEmail,
            Status = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _uow.CustomerProfiles.AddAsync(profile, ct);
        await _uow.SaveChangesAsync(ct);

        return new CustomerDetailDto
        {
            Id = (long)profile.Id,
            CustomerCode = profile.CustomerCode,
            FullName = profile.FullName,
            Phone = profile.Phone,
            Email = profile.Email,
            IsActive = true,
            TotalOrders = 0,
            TotalSpent = 0,
            CreatedAt = profile.CreatedAt,
            RecentOrders = [],
        };
    }
}
