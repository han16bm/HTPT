using FishApp.API.Data;
using FishApp.API.Models.DTOs.Reviews;
using FishApp.API.Models.Entities;
using FishApp.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Common;
using System.Text.Json;

namespace FishApp.API.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;

    public ReviewService(AppDbContext db) => _db = db;

    public async Task<ApiResponse<ReviewsResponse>> GetProductReviewsAsync(int productId, int page, int pageSize)
    {
        var query = _db.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.IsApproved);

        var total = await query.CountAsync();
        var avg = total > 0 ? await query.AverageAsync(r => (double)r.Rating) : 0;

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => MapDto(r)).ToListAsync();

        return ApiResponse<ReviewsResponse>.Ok(new ReviewsResponse
        {
            Reviews = reviews, Total = total, AverageRating = Math.Round(avg, 1),
            Page = page, PageSize = pageSize
        });
    }

    public async Task<ApiResponse<ReviewDto>> CreateReviewAsync(int userId, int productId, CreateReviewRequest request)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null) return ApiResponse<ReviewDto>.Fail("Sản phẩm không tồn tại");

        // Check if user already reviewed this product
        if (await _db.Reviews.AnyAsync(r => r.UserId == userId && r.ProductId == productId))
            return ApiResponse<ReviewDto>.Fail("Bạn đã đánh giá sản phẩm này rồi");

        var review = new Review
        {
            UserId = userId,
            ProductId = productId,
            Rating = request.Rating,
            Comment = request.Comment,
            Images = request.Images != null ? JsonSerializer.Serialize(request.Images) : null
        };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        var created = await _db.Reviews.Include(r => r.User).FirstAsync(r => r.Id == review.Id);
        return ApiResponse<ReviewDto>.Ok(MapDto(created));
    }

    public async Task<ApiResponse<ReviewDto>> UpdateReviewAsync(int userId, int productId, int reviewId, UpdateReviewRequest request)
    {
        var review = await _db.Reviews.Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId && r.ProductId == productId);

        if (review == null) return ApiResponse<ReviewDto>.Fail("Không tìm thấy đánh giá");

        if (request.Rating.HasValue) review.Rating = request.Rating.Value;
        if (request.Comment != null) review.Comment = request.Comment;
        if (request.Images != null) review.Images = JsonSerializer.Serialize(request.Images);

        await _db.SaveChangesAsync();
        return ApiResponse<ReviewDto>.Ok(MapDto(review));
    }

    public async Task<ApiResponse<object>> DeleteReviewAsync(int userId, int productId, int reviewId)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r =>
            r.Id == reviewId && r.UserId == userId && r.ProductId == productId);

        if (review == null) return ApiResponse<object>.Fail("Không tìm thấy đánh giá");

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
        return ApiResponse<object>.Ok(null, "Đã xóa đánh giá");
    }

    private static ReviewDto MapDto(Review r) => new()
    {
        Id = r.Id,
        CustomerId = r.UserId,
        CustomerName = r.User?.FullName ?? "Ẩn danh",
        CustomerAvatar = r.User?.Avatar,
        Rating = r.Rating,
        Comment = r.Comment,
        Images = r.Images != null ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(r.Images) : null,
        CreatedAt = r.CreatedAt
    };
}
