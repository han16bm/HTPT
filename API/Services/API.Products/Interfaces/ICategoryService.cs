using API.Products.Models.Commands;
using API.Products.Models.DTOs;
using API.Products.Models.Queries;

namespace API.Products.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(CategoryQuery query, CancellationToken ct = default);
    Task<List<CategoryDto>> GetTreeAsync(CancellationToken ct = default);
    Task<CategoryDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<CategoryDto> UpsertAsync(UpsertCategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(DeleteCategoryRequest request, CancellationToken ct = default);
}
