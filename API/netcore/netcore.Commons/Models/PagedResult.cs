namespace netcore.Commons.Models;

/// <summary>
/// Kết quả phân trang chuẩn cho tất cả danh sách
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public static PagedResult<T> Create(List<T> items, int totalCount, int page, int pageSize) => new()
    {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };

    public static PagedResult<T> Empty(int page = 1, int pageSize = 20) => new()
    {
        Items = [],
        TotalCount = 0,
        Page = page,
        PageSize = pageSize
    };
}
