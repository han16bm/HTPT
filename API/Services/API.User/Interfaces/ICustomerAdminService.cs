using API.User.Models.Commands;
using API.User.Models.DTOs;
using API.User.Models.Queries;
using netcore.Commons.Models;

namespace API.User.Interfaces;

public interface ICustomerAdminService
{
    Task<PagedResult<CustomerListDto>> GetAllAsync(CustomerQuery query, CancellationToken ct = default);
    Task<CustomerDetailDto> GetByIdAsync(long customerId, CancellationToken ct = default);
    Task<CustomerDetailDto> CreateAsync(CustomerUpsertRequest request, CancellationToken ct = default);
    Task<CustomerDetailDto> UpsertAsync(CustomerUpsertRequest request, CancellationToken ct = default);
    Task<CustomerDetailDto> CreateWalkInAsync(CustomerWalkInRequest request, CancellationToken ct = default);
    Task DeleteAsync(long customerId, CancellationToken ct = default);
}
