using API.Orders.Interfaces;
using API.Orders.Models.Commands;
using API.Orders.Models.DTOs;
using API.Orders.Models.Queries;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Exceptions;
using netcore.Commons.Models;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;

namespace API.Orders.Services;

public class PromotionService : IPromotionService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PromotionService> _logger;

    public PromotionService(IUnitOfWork uow, ILogger<PromotionService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<PromotionValidationDto> ValidatePromotionAsync(ValidatePromotionRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var promo = await _uow.Promotions.FirstOrDefaultAsync(
            p => p.PromoCode == request.Code && p.Status == true, ct);

        if (promo is null)
            return new PromotionValidationDto { IsValid = false, Message = "Mã giảm giá không tồn tại hoặc đã hết hạn." };

        if (now < promo.StartAt || now > promo.EndAt)
            return new PromotionValidationDto { IsValid = false, Message = "Mã giảm giá không còn hiệu lực." };

        if (promo.UsageLimit.HasValue && promo.UsedCount >= promo.UsageLimit.Value)
            return new PromotionValidationDto { IsValid = false, Message = "Mã giảm giá đã hết lượt dùng." };

        if (request.OrderAmount < promo.MinOrderValue)
            return new PromotionValidationDto
            {
                IsValid = false,
                Message = $"Đơn hàng tối thiểu {promo.MinOrderValue:N0}đ để dùng mã này."
            };

        decimal discount = promo.DiscountType == "PERCENT"
            ? request.OrderAmount * promo.DiscountValue / 100
            : promo.DiscountValue;

        if (promo.MaxDiscountValue.HasValue)
            discount = Math.Min(discount, promo.MaxDiscountValue.Value);

        discount = Math.Min(discount, request.OrderAmount);

        return new PromotionValidationDto
        {
            IsValid = true,
            Message = $"Áp dụng mã '{promo.PromoCode}' thành công!",
            Code = promo.PromoCode,
            DiscountType = promo.DiscountType,
            DiscountValue = promo.DiscountValue,
            DiscountAmount = discount,
            FinalAmount = request.OrderAmount - discount,
        };
    }

    public async Task<PagedResult<PromotionDto>> GetAllAsync(PromotionQuery query, CancellationToken ct = default)
    {
        var q = _uow.Promotions.Query().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(p => p.PromoCode.Contains(query.Keyword) || p.Title.Contains(query.Keyword));

        if (query.Status.HasValue)
            q = q.Where(p => p.Status == (query.Status.Value == 1));

        q = q.OrderByDescending(p => p.CreatedAt);

        var totalCount = await q.CountAsync(ct);
        var pageSize = Math.Min(query.PageSize, 100);
        var page = Math.Max(query.Page, 1);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToDto(p))
            .ToListAsync(ct);

        return new PagedResult<PromotionDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PromotionDto> UpsertAsync(UpsertPromotionRequest request, CancellationToken ct = default)
    {
        Promotion promo;

        if (request.Id.HasValue)
        {
            promo = await _uow.Promotions.FirstOrDefaultAsync(p => p.Id == (decimal)request.Id.Value, ct)
                ?? throw new NotFoundException("Khuyến mãi", request.Id.Value);

            promo.PromoCode = request.PromoCode;
            promo.Title = request.Title;
            promo.Description = request.Description;
            promo.DiscountType = request.DiscountType;
            promo.DiscountValue = request.DiscountValue;
            promo.MaxDiscountValue = request.MaxDiscountValue;
            promo.MinOrderValue = request.MinOrderValue ?? 0;
            promo.UsageLimit = request.UsageLimit;
            promo.Status = request.Status == 1;
            promo.StartAt = request.StartAt ?? DateTime.UtcNow;
            promo.EndAt = request.EndAt ?? DateTime.UtcNow.AddDays(30);
            promo.UpdatedAt = DateTime.UtcNow;
            _uow.Promotions.Update(promo);
        }
        else
        {
            // Kiểm tra code trùng
            if (await _uow.Promotions.AnyAsync(p => p.PromoCode == request.PromoCode, ct))
                throw new MessageException($"Mã giảm giá '{request.PromoCode}' đã tồn tại.");

            promo = new Promotion
            {
                PromoCode = request.PromoCode,
                Title = request.Title,
                Description = request.Description,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MaxDiscountValue = request.MaxDiscountValue,
                MinOrderValue = request.MinOrderValue ?? 0,
                UsageLimit = request.UsageLimit,
                UsedCount = 0,
                Status = request.Status == 1,
                StartAt = request.StartAt ?? DateTime.UtcNow,
                EndAt = request.EndAt ?? DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            await _uow.Promotions.AddAsync(promo, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return MapToDto(promo);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var promo = await _uow.Promotions.FirstOrDefaultAsync(p => p.Id == (decimal)id, ct)
            ?? throw new NotFoundException("Khuyến mãi", id);

        _uow.Promotions.Remove(promo);
        await _uow.SaveChangesAsync(ct);
    }

    private static PromotionDto MapToDto(Promotion p) => new()
    {
        Id = (long)p.Id,
        PromoCode = p.PromoCode,
        Title = p.Title,
        Description = p.Description,
        DiscountType = p.DiscountType,
        DiscountValue = p.DiscountValue,
        MinOrderValue = p.MinOrderValue,
        MaxDiscountValue = p.MaxDiscountValue,
        UsageLimit = p.UsageLimit.HasValue ? (int?)Convert.ToInt32(p.UsageLimit.Value) : null,
        UsedCount = Convert.ToInt32(p.UsedCount),
        Status = (p.Status ?? false) ? 1 : 0,
        StartAt = p.StartAt,
        EndAt = p.EndAt,
    };
}
