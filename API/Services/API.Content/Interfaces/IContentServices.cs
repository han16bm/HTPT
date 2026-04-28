using API.Content.Models.Commands;
using API.Content.Models.DTOs;
using API.Content.Models.Queries;
using netcore.Commons.Models;

namespace API.Content.Interfaces;

public interface IBlogService
{
    Task<PagedResult<BlogPostListDto>> GetAllAsync(BlogQuery query, CancellationToken ct = default);
    Task<BlogPostDto> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<BlogPostDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<BlogPostDto> UpsertAsync(UpsertBlogPostRequest request, long authorId, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task<List<BlogCategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<BlogCategoryDto> UpsertCategoryAsync(UpsertBlogCategoryRequest request, CancellationToken ct = default);
}

public interface IContactService
{
    Task<ContactDto> SubmitAsync(SubmitContactRequest request, CancellationToken ct = default);
    Task<PagedResult<ContactDto>> GetAllAsync(ContactQuery query, CancellationToken ct = default);
    Task<ContactDto> UpdateStatusAsync(UpdateContactStatusRequest request, CancellationToken ct = default);
}
