using API.Product.Interfaces;
using API.Product.Models.Commands;
using API.Product.Models.DTOs;
using API.Product.Models.Queries;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Exceptions;
using netcore.Commons.Interfaces;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;

namespace API.Product.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IObjectStorageService _storage;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IUnitOfWork uow, IObjectStorageService storage, ILogger<CategoryService> logger)
    {
        _uow = uow;
        _storage = storage;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetAllAsync(CategoryQuery query, CancellationToken ct = default)
    {
        var q = _uow.Categories.Query().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Name))
            q = q.Where(c => c.Name.Contains(query.Name));

        if (query.ParentId.HasValue)
            q = q.Where(c => c.ParentId == (decimal)query.ParentId.Value);

        if (query.Status.HasValue)
            q = q.Where(c => c.Status == query.Status.Value);

        var items = await q
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

        return items.Select(MapToDto).ToList();
    }

    public async Task<List<CategoryDto>> GetTreeAsync(CancellationToken ct = default)
    {
        var all = await _uow.Categories.Query()
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

        var dtos = all.Select(MapToDto).ToList();
        var map = dtos.ToDictionary(d => d.Id);
        var roots = new List<CategoryDto>();

        foreach (var dto in dtos)
        {
            if (dto.ParentId.HasValue && map.TryGetValue(dto.ParentId.Value, out var parent))
                parent.Children.Add(dto);
            else
                roots.Add(dto);
        }

        return roots;
    }

    public async Task<CategoryDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var cat = await _uow.Categories.FirstOrDefaultAsync(c => c.Id == (decimal)id, ct)
            ?? throw new NotFoundException("Danh mục", id);

        return MapToDto(cat);
    }

    public async Task<CategoryDto> UpsertAsync(UpsertCategoryRequest request, CancellationToken ct = default)
    {
        Category category;

        if (request.Id.HasValue)
        {
            category = await _uow.Categories.FirstOrDefaultAsync(c => c.Id == (decimal)request.Id.Value, ct)
                ?? throw new NotFoundException("Danh mục", request.Id.Value);

            if (!string.IsNullOrWhiteSpace(request.CategoryCode))
            {
                category.CategoryCode = request.CategoryCode.Trim().ToUpperInvariant();
            }

            category.Name = request.Name;
            category.ParentId = request.ParentId.HasValue ? (decimal)request.ParentId.Value : null;
            category.Slug = string.IsNullOrWhiteSpace(request.Slug) ? GenerateSlug(request.Name) : request.Slug;
            category.Description = request.Description;
            category.DisplayOrder = (decimal)request.DisplayOrder;
            category.Status = request.Status;
            category.UpdatedAt = DateTime.UtcNow;

            _uow.Categories.Update(category);
        }
        else
        {
            category = new Category
            {
                CategoryCode = string.IsNullOrWhiteSpace(request.CategoryCode)
                    ? $"DM{DateTime.UtcNow:yyyyMMddHHmmss}"
                    : request.CategoryCode.Trim().ToUpperInvariant(),
                Name = request.Name,
                ParentId = request.ParentId.HasValue ? (decimal)request.ParentId.Value : null,
                Slug = string.IsNullOrWhiteSpace(request.Slug) ? GenerateSlug(request.Name) : request.Slug,
                Description = request.Description,
                DisplayOrder = (decimal)request.DisplayOrder,
                Status = request.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _uow.Categories.AddAsync(category, ct);
        }

        category.ImageUrl = await _storage.ReplaceImageAsync(
            request.ImageFile,
            category.ImageUrl,
            $"categories/{category.CategoryCode}/primary",
            request.RemoveImage,
            request.ImageUrl,
            ct);

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Category upserted: {Id} - {Name}", category.Id, category.Name);
        return MapToDto(category);
    }

    public async Task DeleteAsync(DeleteCategoryRequest request, CancellationToken ct = default)
    {
        var cat = await _uow.Categories.FirstOrDefaultAsync(c => c.Id == (decimal)request.Id, ct)
            ?? throw new NotFoundException("Danh mục", request.Id);

        var hasProducts = await _uow.Products.AnyAsync(p => p.CategoryId == (decimal)request.Id, ct);
        if (hasProducts)
            throw new MessageException("Không thể xóa danh mục đang có sản phẩm. Hãy chuyển sản phẩm trước.");

        await _storage.RemoveIfManagedAsync(cat.ImageUrl, ct);
        _uow.Categories.Remove(cat);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Category deleted: {Id}", request.Id);
    }

    private static string GenerateSlug(string name) =>
        name.ToLowerInvariant().Replace(" ", "-").Replace("đ", "d").Trim('-');

    private static CategoryDto MapToDto(Category c) => new()
    {
        Id = (long)c.Id,
        ParentId = c.ParentId.HasValue ? (long)c.ParentId.Value : null,
        CategoryCode = c.CategoryCode,
        Name = c.Name,
        Slug = c.Slug,
        Description = c.Description,
        ImageUrl = c.ImageUrl,
        DisplayOrder = (int)c.DisplayOrder,
        Status = c.Status ?? true,
        CreatedAt = c.CreatedAt
    };
}
