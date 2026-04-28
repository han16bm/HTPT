using API.Content.Interfaces;
using API.Content.Models.Commands;
using API.Content.Models.DTOs;
using API.Content.Models.Queries;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Exceptions;
using netcore.Commons.Interfaces;
using netcore.Commons.Models;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;

namespace API.Content.Services;

public class BlogService : IBlogService
{
    private readonly IUnitOfWork _uow;
    private readonly IObjectStorageService _storage;
    private readonly ILogger<BlogService> _logger;

    public BlogService(IUnitOfWork uow, IObjectStorageService storage, ILogger<BlogService> logger)
    {
        _uow = uow;
        _storage = storage;
        _logger = logger;
    }

    public async Task<PagedResult<BlogPostListDto>> GetAllAsync(BlogQuery query, CancellationToken ct = default)
    {
        var q = _uow.BlogPosts.Query()
            .Include(b => b.BlogCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(b => b.Title.Contains(query.Keyword) || (b.Summary != null && b.Summary.Contains(query.Keyword)));

        if (query.CategoryId.HasValue)
            q = q.Where(b => b.CategoryId == (decimal)query.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(b => b.Status == query.Status);
        else
            q = q.Where(b => b.Status == "PUBLISHED");

        q = q.OrderByDescending(b => b.PublishedAt ?? b.CreatedAt);

        var totalCount = await q.CountAsync(ct);
        var pageSize = Math.Min(query.PageSize, 50);
        var page = Math.Max(query.Page, 1);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BlogPostListDto
            {
                Id = (long)b.Id,
                CategoryId = b.CategoryId.HasValue ? (long)b.CategoryId.Value : null,
                CategoryName = b.BlogCategory != null ? b.BlogCategory.Name : null,
                Title = b.Title,
                Slug = b.Slug,
                Summary = b.Summary,
                ThumbnailUrl = b.ThumbnailUrl,
                Status = b.Status,
                PublishedAt = b.PublishedAt,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<BlogPostListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<BlogPostDto> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var post = await _uow.BlogPosts.Query()
            .Include(b => b.BlogCategory)
            .FirstOrDefaultAsync(b => b.Slug == slug, ct)
            ?? throw new NotFoundException($"Không tìm thấy bài viết với slug: {slug}");

        return MapToDto(post);
    }

    public async Task<BlogPostDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var post = await _uow.BlogPosts.Query()
            .Include(b => b.BlogCategory)
            .FirstOrDefaultAsync(b => b.Id == (decimal)id, ct)
            ?? throw new NotFoundException("Bài viết", id);

        return MapToDto(post);
    }

    public async Task<BlogPostDto> UpsertAsync(UpsertBlogPostRequest request, long authorId, CancellationToken ct = default)
    {
        BlogPost post;

        if (request.Id.HasValue)
        {
            post = await _uow.BlogPosts.FirstOrDefaultAsync(b => b.Id == (decimal)request.Id.Value, ct)
                ?? throw new NotFoundException("Bài viết", request.Id.Value);

            post.CategoryId = request.CategoryId.HasValue ? (decimal)request.CategoryId.Value : null;
            post.Title = request.Title;
            post.Slug = string.IsNullOrWhiteSpace(request.Slug) ? GenerateSlug(request.Title) : request.Slug;
            post.Summary = request.Summary;
            post.Content = request.Content;
            post.Status = request.Status;
            post.UpdatedAt = DateTime.UtcNow;

            if (request.Status == "PUBLISHED" && post.PublishedAt is null)
                post.PublishedAt = DateTime.UtcNow;

            _uow.BlogPosts.Update(post);
        }
        else
        {
            post = new BlogPost
            {
                CategoryId = request.CategoryId.HasValue ? (decimal)request.CategoryId.Value : null,
                AuthorId = (decimal)authorId,
                Title = request.Title,
                Slug = string.IsNullOrWhiteSpace(request.Slug) ? GenerateSlug(request.Title) : request.Slug,
                Summary = request.Summary,
                Content = request.Content,
                Status = request.Status,
                PublishedAt = request.Status == "PUBLISHED" ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _uow.BlogPosts.AddAsync(post, ct);
        }

        post.ThumbnailUrl = await _storage.ReplaceImageAsync(
            request.ThumbnailFile,
            post.ThumbnailUrl,
            $"blogs/{post.Slug}/thumbnail",
            request.RemoveThumbnail,
            request.ThumbnailUrl,
            ct);

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Blog post upserted: {Slug}", post.Slug);
        return await GetByIdAsync((long)post.Id, ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var post = await _uow.BlogPosts.FirstOrDefaultAsync(b => b.Id == (decimal)id, ct)
            ?? throw new NotFoundException("Bài viết", id);

        await _storage.RemoveIfManagedAsync(post.ThumbnailUrl, ct);
        _uow.BlogPosts.Remove(post);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<List<BlogCategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await _uow.BlogCategories.Query().ToListAsync(ct);
        var postCounts = await _uow.BlogPosts.Query()
            .Where(b => b.CategoryId != null)
            .GroupBy(b => b.CategoryId)
            .Select(g => new { CategoryId = g.Key!.Value, Count = g.Count() })
            .ToListAsync(ct);

        return categories.Select(c => new BlogCategoryDto
        {
            Id = (long)c.Id,
            Name = c.Name,
            Slug = c.Slug,
            PostCount = postCounts.FirstOrDefault(p => p.CategoryId == c.Id)?.Count ?? 0
        }).ToList();
    }

    public async Task<BlogCategoryDto> UpsertCategoryAsync(UpsertBlogCategoryRequest request, CancellationToken ct = default)
    {
        BlogCategory cat;

        if (request.Id.HasValue)
        {
            cat = await _uow.BlogCategories.FirstOrDefaultAsync(c => c.Id == (decimal)request.Id.Value, ct)
                ?? throw new NotFoundException("Danh mục blog", request.Id.Value);

            cat.Name = request.Name;
            cat.Slug = string.IsNullOrWhiteSpace(request.Slug) ? GenerateSlug(request.Name) : request.Slug;
            _uow.BlogCategories.Update(cat);
        }
        else
        {
            cat = new BlogCategory
            {
                Name = request.Name,
                Slug = string.IsNullOrWhiteSpace(request.Slug) ? GenerateSlug(request.Name) : request.Slug
            };
            await _uow.BlogCategories.AddAsync(cat, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return new BlogCategoryDto { Id = (long)cat.Id, Name = cat.Name, Slug = cat.Slug };
    }

    private static string GenerateSlug(string title)
        => title.ToLowerInvariant().Replace(" ", "-").Replace("đ", "d").Trim('-');

    private static BlogPostDto MapToDto(BlogPost b) => new()
    {
        Id = (long)b.Id,
        CategoryId = b.CategoryId.HasValue ? (long)b.CategoryId.Value : null,
        CategoryName = b.BlogCategory?.Name,
        Title = b.Title,
        Slug = b.Slug,
        Summary = b.Summary,
        Content = b.Content,
        ThumbnailUrl = b.ThumbnailUrl,
        Status = b.Status,
        PublishedAt = b.PublishedAt,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}
