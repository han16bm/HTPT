using API.Product.Models.Commands;
using API.Product.Models.DTOs;
using API.Product.Models.Queries;
using netcore.Commons.Models;

namespace API.Product.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductListDto>> GetAllAsync(ProductQuery query, CancellationToken ct = default);
    Task<ProductDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ProductDto> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<List<ProductListDto>> GetNoiBatAsync(int top = 8, CancellationToken ct = default);
    Task<ProductDto> UpsertAsync(UpsertProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(DeleteProductRequest request, CancellationToken ct = default);
}
