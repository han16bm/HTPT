using API.Order.Interfaces;
using API.Order.Models.Commands;
using API.Order.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Order.Controllers;

[Audit]
[ApiKey]
[Route("cart")]
public class CartController : BaseApiController
{
    private readonly ICartService _service;

    public CartController(ICartService service)
    {
        _service = service;
    }

    // GET /api/order/cart
    [HttpGet]
    public async Task<ApiResponse<CartDto>> GetCurrent(CancellationToken ct)
    {
        var result = await _service.GetCartAsync(GetUserId(), ct);
        return ApiResponse.Ok(result, "Lấy giỏ hàng thành công");
    }

    // POST /api/order/cart/items
    [HttpPost("items")]
    public async Task<ApiResponse<CartDto>> AddItem([FromBody] AddToCartRequest request, CancellationToken ct)
    {
        var result = await _service.AddToCartAsync(GetUserId(), request, ct);
        return ApiResponse.Ok(result, "Đã thêm vào giỏ hàng");
    }

    // PUT /api/order/cart/items/1
    [HttpPut("items/{cartItemId:long}")]
    public async Task<ApiResponse<CartDto>> UpdateItemQuantity(
        [FromRoute] long cartItemId,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken ct)
    {
        request.CartItemId = cartItemId;
        var result = await _service.UpdateQuantityAsync(GetUserId(), request, ct);
        return ApiResponse.Ok(result, "Cập nhật số lượng thành công");
    }

    // DELETE /api/order/cart/items/1
    [HttpDelete("items/{cartItemId:long}")]
    public async Task<ApiResponse<CartDto>> RemoveItem([FromRoute] long cartItemId, CancellationToken ct)
    {
        var result = await _service.RemoveItemAsync(GetUserId(), new RemoveCartItemRequest { CartItemId = cartItemId }, ct);
        return ApiResponse.Ok(result, "Đã xóa khỏi giỏ hàng");
    }

    // DELETE /api/order/cart
    [HttpDelete]
    public async Task<ApiResponse> Clear(CancellationToken ct)
    {
        await _service.ClearCartAsync(GetUserId(), ct);
        return ApiResponse.OkEmpty("Đã xóa giỏ hàng");
    }
}
