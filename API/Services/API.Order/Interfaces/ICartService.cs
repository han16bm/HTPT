using API.Order.Models.Commands;
using API.Order.Models.DTOs;

namespace API.Order.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(long userId, CancellationToken ct = default);
    Task<CartDto> AddToCartAsync(long userId, AddToCartRequest request, CancellationToken ct = default);
    Task<CartDto> UpdateQuantityAsync(long userId, UpdateCartItemRequest request, CancellationToken ct = default);
    Task<CartDto> RemoveItemAsync(long userId, RemoveCartItemRequest request, CancellationToken ct = default);
    Task ClearCartAsync(long userId, CancellationToken ct = default);
}
