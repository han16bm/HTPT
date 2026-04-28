using API.Orders.Interfaces;
using API.Orders.Models.Commands;
using API.Orders.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;
using netcore.Commons.Controllers;
using netcore.Commons.Models;

namespace API.Orders.Controllers;

[Audit]
[ApiKey]
[Route("[controller]")]
public class CartController : BaseApiController
{
    private readonly ICartService _service;

    public CartController(ICartService service) => _service = service;

    // GET /api/orders/cart/gio-hang-hien-tai
    [HttpGet("gio-hang-hien-tai")]
    public async Task<ApiResponse<CartDto>> GioHangHienTai(CancellationToken ct)
    {
        var result = await _service.GetCartAsync(GetUserId(), ct);
        return ApiResponse.Ok(result, "Lấy giỏ hàng thành công");
    }

    // POST /api/orders/cart/them-san-pham
    [HttpPost("them-san-pham")]
    public async Task<ApiResponse<CartDto>> ThemSanPham([FromBody] AddToCartRequest request, CancellationToken ct)
    {
        var result = await _service.AddToCartAsync(GetUserId(), request, ct);
        return ApiResponse.Ok(result, "Đã thêm vào giỏ hàng");
    }

    // POST /api/orders/cart/cap-nhat-so-luong
    [HttpPost("cap-nhat-so-luong")]
    public async Task<ApiResponse<CartDto>> CapNhatSoLuong([FromBody] UpdateCartItemRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateQuantityAsync(GetUserId(), request, ct);
        return ApiResponse.Ok(result, "Cập nhật số lượng thành công");
    }

    // POST /api/orders/cart/xoa-san-pham
    [HttpPost("xoa-san-pham")]
    public async Task<ApiResponse<CartDto>> XoaSanPham([FromBody] RemoveCartItemRequest request, CancellationToken ct)
    {
        var result = await _service.RemoveItemAsync(GetUserId(), request, ct);
        return ApiResponse.Ok(result, "Đã xóa khỏi giỏ hàng");
    }

    // POST /api/orders/cart/xoa-gio-hang
    [HttpPost("xoa-gio-hang")]
    public async Task<ApiResponse> XoaGioHang(CancellationToken ct)
    {
        await _service.ClearCartAsync(GetUserId(), ct);
        return ApiResponse.OkEmpty("Đã xóa giỏ hàng");
    }
}
