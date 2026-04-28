using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace API.Content.Models.Commands;

public class UpsertBlogPostRequest
{
    public long? Id { get; set; }

    [Required]
    public long? CategoryId { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; }
    public string? Summary { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }
    public IFormFile? ThumbnailFile { get; set; }
    public bool RemoveThumbnail { get; set; }
    public string Status { get; set; } = "DRAFT"; // DRAFT | PUBLISHED
}

public class UpsertBlogCategoryRequest
{
    public long? Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Slug { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SubmitContactRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Subject { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;
}

public class UpdateContactStatusRequest
{
    [Required]
    public long Id { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty; // NEW | PROCESSING | RESOLVED | CLOSED
}
