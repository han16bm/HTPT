using API.Order.Models.Commands;
using API.Order.Models.DTOs;
using API.Order.Models.Queries;
using netcore.Commons.Models;

namespace API.Order.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderFromCartAsync(long userId, CreateOrderRequest request, CancellationToken ct = default);
    Task<OrderDto> CreateDirectOrderAsync(DirectOrderRequest request, CancellationToken ct = default);
    Task<PagedResult<OrderListDto>> GetMyOrdersAsync(long userId, OrderQuery query, CancellationToken ct = default);
    Task<PagedResult<OrderListDto>> GetAllOrdersAsync(OrderQuery query, CancellationToken ct = default);
    Task<OrderDto> GetByOrderCodeAsync(string orderCode, CancellationToken ct = default);
    Task<OrderDto> GetByIdAsync(long orderId, CancellationToken ct = default);
    Task<OrderDto> UpdateStatusAsync(UpdateOrderStatusRequest request, CancellationToken ct = default);
    Task<OrderDto> CancelOrderAsync(long userId, CancelOrderRequest request, CancellationToken ct = default);
}
