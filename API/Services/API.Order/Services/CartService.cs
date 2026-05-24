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
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public CartService(
        IUnitOfWork uow, 
        ILogger<CartService> logger, 
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _uow = uow;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _config = config;
    }

    // ─────────────────────────────────────────────
    // LẤY GIỎ HÀNG
    // ─────────────────────────────────────────────
    public async Task<CartDto> GetCartAsync(long userId, CancellationToken ct = default)
    {
        var cart = await GetOrCreateCartAsync(userId, ct);

        var items = await _uow.CartItems.Query()
            .Where(ci => ci.CartId == cart.Id)
            .ToListAsync(ct);

        return MapToDto(cart, items);
    }

    // ─────────────────────────────────────────────
    // THÊM SẢN PHẨM
    // ─────────────────────────────────────────────
    public async Task<CartDto> AddToCartAsync(long userId, AddToCartRequest request, CancellationToken ct = default)
    {
        if (request.Quantity <= 0)
            throw new MessageException("Số lượng không hợp lệ.");

        // Gọi RESTful API sang Product Service để lấy và xác thực thông tin sản phẩm
        var productApiUrl = _config["ProductServiceUrl"] ?? "http://localhost:5002";
        var apiKey = _config["ApiKey:Key"] ?? "fish-gateway-key-2026";
        var headerName = _config["ApiKey:HeaderName"] ?? "X-Api-Key";

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{productApiUrl}/api/product/products/{request.ProductId}");
        requestMessage.Headers.Add(headerName, apiKey);

        var response = await _httpClient.SendAsync(requestMessage, ct);
        if (!response.IsSuccessStatusCode)
            throw new MessageException($"Không thể kết nối đến Product Service (HTTP {response.StatusCode}).");

        var productResult = await response.Content.ReadFromJsonAsync<netcore.Commons.Models.ApiResponse<ProductDtoInfo>>(cancellationToken: ct);
        
        if (productResult is null || !productResult.Success || productResult.Data is null)
            throw new NotFoundException("Sản phẩm không tồn tại trên hệ thống", request.ProductId);

        var productData = productResult.Data;

        if (productData.StockQuantity < request.Quantity)
            throw new MessageException($"'{productData.Name}' không đủ hàng (còn {productData.StockQuantity}).");
        
        // Cập nhật lại request với dữ liệu chính xác từ Product Service
        request.UnitPrice = productData.SalePrice;
        request.ProductName = productData.Name ?? "Sản phẩm";
        request.ImageUrl = productData.ImageUrl;

        var cart = await GetOrCreateCartAsync(userId, ct);

        var existingItem = await _uow.CartItems.FirstOrDefaultAsync(
            ci => ci.CartId == cart.Id && ci.ProductId == (decimal)request.ProductId, ct);

        if (existingItem is not null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.UnitPrice = request.UnitPrice;
            existingItem.ProductName = request.ProductName;
            existingItem.ImageUrl = request.ImageUrl;
            _uow.CartItems.Update(existingItem);
        }
        else
        {
            var item = new CartItem
            {
                CartId = cart.Id,
                ProductId = (decimal)request.ProductId,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                ProductName = request.ProductName,
                ImageUrl = request.ImageUrl,
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
        var item = await _uow.CartItems.FirstOrDefaultAsync(
            ci => ci.Id == (decimal)request.CartItemId && ci.CartId == cart.Id, ct)
            ?? throw new NotFoundException("Mục giỏ hàng", request.CartItemId);

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
        var cart = await _uow.ShoppingCarts.FirstOrDefaultAsync(c => c.CustomerId == (decimal)userId, ct);
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
        var cart = await _uow.ShoppingCarts.FirstOrDefaultAsync(c => c.CustomerId == (decimal)userId, ct);
        if (cart is not null) return cart;

        cart = new ShoppingCart { CustomerId = (decimal)userId, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
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
            ProductName = ci.ProductName ?? string.Empty,
            ImageUrl = ci.ImageUrl,
            Quantity = (int)ci.Quantity,
            UnitPrice = ci.UnitPrice,
            SubTotal = ci.UnitPrice * ci.Quantity,
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

public class ProductDtoInfo
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
}
