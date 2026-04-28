using API.Products.Models.Commands;
using API.Products.Models.DTOs;
using API.Products.Models.Queries;
using netcore.Commons.Models;

namespace API.Products.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductListDto>> GetAllAsync(ProductQuery query, CancellationToken ct = default);
    Task<ProductDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ProductDto> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<List<ProductListDto>> GetNoiBatAsync(int top = 8, CancellationToken ct = default);
    Task<ProductDto> UpsertAsync(UpsertProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(DeleteProductRequest request, CancellationToken ct = default);
}
