using API.Products.Constants;
using API.Products.Interfaces;
using API.Products.Models.Commands;
using API.Products.Models.DTOs;
using API.Products.Models.Queries;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Exceptions;
using netcore.Commons.Interfaces;
using netcore.Commons.Models;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;

namespace API.Products.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IObjectStorageService _storage;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork uow, IObjectStorageService storage, ILogger<ProductService> logger)
    {
        _uow = uow;
        _storage = storage;
        _logger = logger;
    }

    public async Task<PagedResult<ProductListDto>> GetAllAsync(ProductQuery query, CancellationToken ct = default)
    {
        var q = _uow.Products.Query()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Name))
            q = q.Where(p => p.Name.Contains(query.Name));

        if (query.CategoryId.HasValue)
            q = q.Where(p => p.CategoryId == (decimal)query.CategoryId.Value);

        if (query.Status.HasValue)
            q = q.Where(p => p.Status == query.Status.Value);

        if (query.IsFeatured.HasValue)
            q = q.Where(p => p.IsFeatured == query.IsFeatured.Value);

        if (query.MinPrice.HasValue)
            q = q.Where(p => p.SalePrice >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            q = q.Where(p => p.SalePrice <= query.MaxPrice.Value);

        q = (query.SortBy?.ToLower(), query.SortDir?.ToLower()) switch
        {
            ("price", "asc") => q.OrderBy(p => p.SalePrice),
            ("price", _) => q.OrderByDescending(p => p.SalePrice),
            ("name", "asc") => q.OrderBy(p => p.Name),
            ("name", _) => q.OrderByDescending(p => p.Name),
            ("sold", _) => q.OrderByDescending(p => p.SoldQuantity),
            _ => q.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await q.CountAsync(ct);
        var pageSize = Math.Min(query.PageSize, ProductConstants.MaxPageSize);
        var page = Math.Max(query.Page, 1);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToListDto(p))
            .ToListAsync(ct);

        return new PagedResult<ProductListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var product = await _uow.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == (decimal)id, ct)
            ?? throw new NotFoundException("Sản phẩm", id);

        return MapToDto(product);
    }

    public async Task<ProductDto> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var product = await _uow.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Slug == slug, ct)
            ?? throw new NotFoundException($"Không tìm thấy sản phẩm với slug: {slug}");

        return MapToDto(product);
    }

    public async Task<List<ProductListDto>> GetNoiBatAsync(int top = 8, CancellationToken ct = default)
    {
        return await _uow.Products.Query()
            .Include(p => p.Category)
            .Where(p => p.IsFeatured == true && p.Status == true)
            .OrderByDescending(p => p.SoldQuantity)
            .Take(top)
            .Select(p => MapToListDto(p))
            .ToListAsync(ct);
    }

    public async Task<ProductDto> UpsertAsync(UpsertProductRequest request, CancellationToken ct = default)
    {
        _ = await _uow.Categories.FirstOrDefaultAsync(c => c.Id == (decimal)request.CategoryId, ct)
            ?? throw new NotFoundException("Danh mục", request.CategoryId);

        Product product;

        if (request.Id.HasValue)
        {
            product = await _uow.Products.Query()
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == (decimal)request.Id.Value, ct)
                ?? throw new NotFoundException("Sản phẩm", request.Id.Value);

            product.CategoryId = (decimal)request.CategoryId;
            product.Name = request.Name;
            product.Slug = string.IsNullOrWhiteSpace(request.Slug) ? GenerateSlug(request.Name) : request.Slug;
            product.Sku = request.Sku;
            product.ShortDescription = request.ShortDescription;
            product.Description = request.Description;
            product.CostPrice = request.CostPrice;
            product.SalePrice = request.SalePrice;
            product.StockQuantity = (decimal)request.StockQuantity;
            product.WeightGrams = request.WeightGrams;
            product.Status = request.Status;
            product.IsFeatured = request.IsFeatured;
            product.UpdatedAt = DateTime.UtcNow;

            _uow.Products.Update(product);
        }
        else
        {
            product = new Product
            {
                CategoryId = (decimal)request.CategoryId,
                ProductCode = $"SP{DateTime.UtcNow:yyyyMMddHHmmss}",
                Name = request.Name,
                Slug = string.IsNullOrWhiteSpace(request.Slug) ? GenerateSlug(request.Name) : request.Slug,
                Sku = request.Sku,
                ShortDescription = request.ShortDescription,
                Description = request.Description,
                CostPrice = request.CostPrice,
                SalePrice = request.SalePrice,
                StockQuantity = (decimal)request.StockQuantity,
                SoldQuantity = 0,
                WeightGrams = request.WeightGrams,
                Status = request.Status,
                IsFeatured = request.IsFeatured,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _uow.Products.AddAsync(product, ct);
        }

        var productFolder = $"products/{product.ProductCode}";
        product.ImageUrl = await _storage.ReplaceImageAsync(
            request.ImageFile,
            product.ImageUrl,
            $"{productFolder}/primary",
            request.RemoveImage,
            request.ImageUrl,
            ct);

        await _uow.SaveChangesAsync(ct);
        await SyncProductImagesAsync(product, request.Images, productFolder, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Product upserted: {ProductCode} - {Name}", product.ProductCode, product.Name);
        return await GetByIdAsync((long)product.Id, ct);
    }

    public async Task DeleteAsync(DeleteProductRequest request, CancellationToken ct = default)
    {
        var product = await _uow.Products.Query()
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == (decimal)request.Id, ct)
            ?? throw new NotFoundException("Sản phẩm", request.Id);

        foreach (var image in product.ProductImages)
        {
            await _storage.RemoveIfManagedAsync(image.ImageUrl, ct);
        }

        await _storage.RemoveIfManagedAsync(product.ImageUrl, ct);
        _uow.ProductImages.RemoveRange(product.ProductImages);
        _uow.Products.Remove(product);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Product deleted: {Id}", request.Id);
    }

    private async Task SyncProductImagesAsync(
        Product product,
        List<UpsertProductImageRequest> requests,
        string productFolder,
        CancellationToken ct)
    {
        if (requests.Count == 0)
        {
            return;
        }

        var existingImages = await _uow.ProductImages.Query()
            .Where(i => i.ProductId == product.Id)
            .ToListAsync(ct);

        foreach (var item in requests)
        {
            if (item.Id.HasValue)
            {
                var existing = existingImages.FirstOrDefault(i => i.Id == (decimal)item.Id.Value);
                if (existing is null)
                {
                    continue;
                }

                if (item.Remove)
                {
                    await _storage.RemoveIfManagedAsync(existing.ImageUrl, ct);
                    _uow.ProductImages.Remove(existing);
                    continue;
                }

                existing.ImageUrl = await _storage.ReplaceImageAsync(
                    item.ImageFile,
                    existing.ImageUrl,
                    $"{productFolder}/gallery",
                    false,
                    item.ImageUrl,
                    ct) ?? existing.ImageUrl;
                existing.AltText = item.AltText;
                existing.IsPrimary = item.IsPrimary;
                existing.DisplayOrder = item.DisplayOrder;
                _uow.ProductImages.Update(existing);
                continue;
            }

            if (item.Remove || (item.ImageFile is null && string.IsNullOrWhiteSpace(item.ImageUrl)))
            {
                continue;
            }

            var imageUrl = await _storage.ReplaceImageAsync(
                item.ImageFile,
                null,
                $"{productFolder}/gallery",
                false,
                item.ImageUrl,
                ct);

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                continue;
            }

            await _uow.ProductImages.AddAsync(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = imageUrl,
                AltText = item.AltText,
                IsPrimary = item.IsPrimary,
                DisplayOrder = item.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            }, ct);
        }
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("đ", "d")
            .Trim('-');
    }

    private static ProductListDto MapToListDto(Product p) => new()
    {
        Id = (long)p.Id,
        ProductCode = p.ProductCode,
        Name = p.Name,
        Slug = p.Slug,
        ShortDescription = p.ShortDescription,
        Description = p.Description,
        ImageUrl = p.ImageUrl,
        SalePrice = p.SalePrice,
        StockQuantity = (int)p.StockQuantity,
        SoldQuantity = (int)p.SoldQuantity,
        Status = p.Status ?? true,
        IsFeatured = p.IsFeatured ?? false,
        CategoryId = (long)p.CategoryId,
        CategoryName = p.Category?.Name,
        CreatedAt = p.CreatedAt
    };

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = (long)p.Id,
        CategoryId = (long)p.CategoryId,
        CategoryName = p.Category?.Name,
        ProductCode = p.ProductCode,
        Sku = p.Sku,
        Name = p.Name,
        Slug = p.Slug,
        ShortDescription = p.ShortDescription,
        Description = p.Description,
        ImageUrl = p.ImageUrl,
        CostPrice = p.CostPrice,
        SalePrice = p.SalePrice,
        StockQuantity = (int)p.StockQuantity,
        SoldQuantity = (int)p.SoldQuantity,
        WeightGrams = p.WeightGrams,
        Status = p.Status ?? true,
        IsFeatured = p.IsFeatured ?? false,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Images = p.ProductImages?.Select(i => new ProductImageDto
        {
            Id = (long)i.Id,
            ImageUrl = i.ImageUrl,
            AltText = i.AltText,
            IsPrimary = i.IsPrimary ?? false,
            DisplayOrder = (int)i.DisplayOrder
        }).OrderBy(i => i.DisplayOrder).ToList() ?? []
    };
}
