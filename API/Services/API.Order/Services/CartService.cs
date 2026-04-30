using API.Order.Interfaces;
using API.Order.Models.Commands;
using API.Order.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Exceptions;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;

namespace API.Order.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CartService> _logger;

    public CartService(IUnitOfWork uow, ILogger<CartService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    // ─────────────────────────────────────────────
    // LẤY GIỎ HÀNG
    // ─────────────────────────────────────────────
    public async Task<CartDto> GetCartAsync(long userId, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, ct);

        var items = await _uow.CartItems.Query()
            .Include(ci => ci.Product)
            .Where(ci => ci.CartId == cart.Id)
            .ToListAsync(ct);

        return MapToDto(cart, items);
    }

    // ─────────────────────────────────────────────
    // THÊM SẢN PHẨM
    // ─────────────────────────────────────────────
    public async Task<CartDto> AddToCartAsync(long userId, AddToCartRequest request, CancellationToken ct = default)
    {
        var product = await _uow.Products.FirstOrDefaultAsync(
            p => p.Id == (decimal)request.ProductId && p.Status == true, ct)
            ?? throw new NotFoundException("Sản phẩm", request.ProductId);

        if (product.StockQuantity < request.Quantity)
            throw new MessageException($"Sản phẩm '{product.Name}' không đủ hàng. Còn lại: {product.StockQuantity}");

        var cart = await GetOrCreateCartAsync(userId, ct);

        // Kiểm tra xem SP đã có trong giỏ chưa
        var existingItem = await _uow.CartItems.FirstOrDefaultAsync(
            ci => ci.CartId == cart.Id && ci.ProductId == (decimal)request.ProductId, ct);

        if (existingItem is not null)
        {
            var newQty = existingItem.Quantity + request.Quantity;
            if (product.StockQuantity < newQty)
                throw new MessageException($"Không thể thêm. Tổng số lượng vượt quá tồn kho ({product.StockQuantity}).");

            existingItem.Quantity = newQty;
            existingItem.UnitPrice = product.SalePrice;
            _uow.CartItems.Update(existingItem);
        }
        else
        {
            var item = new CartItem
            {
                CartId = cart.Id,
                ProductId = (decimal)request.ProductId,
                Quantity = request.Quantity,
                UnitPrice = product.SalePrice,
            };
            await _uow.CartItems.AddAsync(item, ct);
        }

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} added product {ProductId} x{Qty} to cart", userId, request.ProductId, request.Quantity);

        return await GetCartAsync(userId, ct);
    }

    // ─────────────────────────────────────────────
    // CẬP NHẬT SỐ LƯỢNG
    // ─────────────────────────────────────────────
    public async Task<CartDto> UpdateQuantityAsync(long userId, UpdateCartItemRequest request, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, ct);
        var item = await _uow.CartItems.Query()
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.Id == (decimal)request.CartItemId && ci.CartId == cart.Id, ct)
            ?? throw new NotFoundException("Mục giỏ hàng", request.CartItemId);

        if (item.Product!.StockQuantity < request.Quantity)
            throw new MessageException($"Sản phẩm '{item.Product.Name}' không đủ hàng. Còn lại: {item.Product.StockQuantity}");

        item.Quantity = request.Quantity;
        _uow.CartItems.Update(item);
        await _uow.SaveChangesAsync(ct);

        return await GetCartAsync(userId, ct);
    }

    // ─────────────────────────────────────────────
    // XÓA MỤC
    // ─────────────────────────────────────────────
    public async Task<CartDto> RemoveItemAsync(long userId, RemoveCartItemRequest request, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, ct);
        var item = await _uow.CartItems.FirstOrDefaultAsync(
            ci => ci.Id == (decimal)request.CartItemId && ci.CartId == cart.Id, ct)
            ?? throw new NotFoundException("Mục giỏ hàng", request.CartItemId);

        _uow.CartItems.Remove(item);
        await _uow.SaveChangesAsync(ct);

        return await GetCartAsync(userId, ct);
    }

    // ─────────────────────────────────────────────
    // XÓA TOÀN BỘ GIỎ
    // ─────────────────────────────────────────────
    public async Task ClearCartAsync(long userId, CancellationToken ct = default)
    {
        var customer = await _uow.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == (decimal)userId, ct);
        if (customer is null) return;

        var cart = await _uow.ShoppingCarts.FirstOrDefaultAsync(c => c.CustomerId == customer.Id, ct);
        if (cart is null) return;

        var items = await _uow.CartItems.Query()
            .Where(ci => ci.CartId == cart.Id)
            .ToListAsync(ct);

        foreach (var item in items)
            _uow.CartItems.Remove(item);

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} cleared cart", userId);
    }

    // ─────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────
    private async Task<ShoppingCart> GetOrCreateCartAsync(long userId, CancellationToken ct)
    {
        var customer = await _uow.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == (decimal)userId, ct)
            ?? throw new MessageException("Không tìm thấy thông tin khách hàng.");

        var cart = await _uow.ShoppingCarts.FirstOrDefaultAsync(c => c.CustomerId == customer.Id, ct);
        if (cart is not null) return cart;

        cart = new ShoppingCart { CustomerId = customer.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await _uow.ShoppingCarts.AddAsync(cart, ct);
        await _uow.SaveChangesAsync(ct);
        return cart;
    }

    private static CartDto MapToDto(ShoppingCart cart, List<CartItem> items)
    {
        var itemDtos = items.Select(ci => new CartItemDto
        {
            Id = (long)ci.Id,
            ProductId = (long)ci.ProductId,
            ProductName = ci.Product?.Name ?? string.Empty,
            ProductSlug = ci.Product?.Slug,
            ImageUrl = ci.Product?.ImageUrl,
            Quantity = (int)ci.Quantity,
            UnitPrice = ci.UnitPrice,
            SubTotal = ci.UnitPrice * ci.Quantity,
            StockQuantity = (int)(ci.Product?.StockQuantity ?? 0),
        }).ToList();

        return new CartDto
        {
            Id = (long)cart.Id,
            UserId = (long)cart.CustomerId,
            Items = itemDtos,
            TotalPrice = itemDtos.Sum(i => i.SubTotal),
            TotalItems = itemDtos.Sum(i => i.Quantity),
        };
    }
}
