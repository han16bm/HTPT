using System;
using System.Collections.Generic;

namespace netcore.Entities.Entities;

public partial class BlogPost
{
    public decimal Id { get; set; }

    public decimal? CategoryId { get; set; }

    public decimal? AuthorId { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Summary { get; set; }

    public string Content { get; set; } = null!;

    public string? ThumbnailUrl { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


