using API.Orders.Models.Commands;
using API.Orders.Models.DTOs;
using netcore.Commons.Models;

namespace API.Orders.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken ct = default);
    Task<PaymentDto?> GetByOrderIdAsync(long orderId, CancellationToken ct = default);
}

public interface IPromotionService
{
    Task<PromotionValidationDto> ValidatePromotionAsync(ValidatePromotionRequest request, CancellationToken ct = default);
    Task<PagedResult<PromotionDto>> GetAllAsync(Models.Queries.PromotionQuery query, CancellationToken ct = default);
    Task<PromotionDto> UpsertAsync(UpsertPromotionRequest request, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}
