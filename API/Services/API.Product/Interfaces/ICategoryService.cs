using API.Product.Models.Commands;
using API.Product.Models.DTOs;
using API.Product.Models.Queries;

namespace API.Product.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(CategoryQuery query, CancellationToken ct = default);
    Task<List<CategoryDto>> GetTreeAsync(CancellationToken ct = default);
    Task<CategoryDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<CategoryDto> UpsertAsync(UpsertCategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(DeleteCategoryRequest request, CancellationToken ct = default);
}
