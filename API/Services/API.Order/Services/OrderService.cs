using API.Order.Interfaces;
using API.Order.Models.Commands;
using API.Order.Models.DTOs;
using API.Order.Models.Queries;
using Microsoft.EntityFrameworkCore;
using netcore.Commons.Exceptions;
using netcore.Commons.Models;
using netcore.Entities.Entities;
using netcore.Entities.Interfaces;
using OrderEntity = netcore.Entities.Entities.Order;
using MassTransit;
using MessageException = netcore.Commons.Exceptions.MessageException;
using System.Net.Http.Json;

namespace API.Order.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<OrderService> _logger;
    private readonly MassTransit.IPublishEndpoint _publishEndpoint;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public OrderService(
        IUnitOfWork uow,
        ILogger<OrderService> logger,
        MassTransit.IPublishEndpoint publishEndpoint,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _uow = uow;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _httpClient = httpClientFactory.CreateClient();
        _config = config;
    }

    // ─────────────────────────────────────────────
    // TẠO ĐƠN TỪ GIỎ HÀNG
    // ─────────────────────────────────────────────
    public async Task<OrderDto> CreateOrderFromCartAsync(long userId, CreateOrderRequest request, CancellationToken ct = default)
    {
        var cart = await _uow.ShoppingCarts.FirstOrDefaultAsync(c => c.CustomerId == (decimal)userId, ct)
            ?? throw new MessageException("Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi đặt hàng.");

        var cartItems = await _uow.CartItems.Query()
            .Where(ci => ci.CartId == cart.Id)
            .ToListAsync(ct);

        if (!cartItems.Any())
            throw new MessageException("Giỏ hàng trống.");

        var productInfos = new Dictionary<decimal, ProductDtoInfo>();
        foreach (var item in cartItems)
        {
            var product = await GetProductInfoAsync((long)item.ProductId, ct);
            if (product.StockQuantity < item.Quantity)
            {
                throw new MessageException($"'{product.Name}' không đủ hàng (còn {product.StockQuantity}).");
            }

            productInfos[item.ProductId] = product;
        }

        // Validate và áp promotion
        decimal discountAmount = 0;
        Promotion? promotion = null;

        if (!string.IsNullOrWhiteSpace(request.PromotionCode))
        {
            promotion = await _uow.Promotions.FirstOrDefaultAsync(
                p => p.PromoCode == request.PromotionCode && p.Status == true, ct);

            if (promotion is not null)
            {
                var subtotalForPromo = cartItems.Sum(ci => ci.UnitPrice * ci.Quantity);
                discountAmount = CalculateDiscount(promotion, subtotalForPromo);
            }
        }

        var userInfo = await GetUserInfoAsync(userId, ct);
        var shippingAddress = FirstNonEmpty(
            request.ShippingAddress,
            userInfo?.Address,
            BuildAddress(userInfo?.AddressLine, userInfo?.Ward, userInfo?.District, userInfo?.Province));

        if (string.IsNullOrWhiteSpace(shippingAddress))
        {
            throw new MessageException("Vui long cap nhat dia chi giao hang.");
        }

        var customerName = FirstNonEmpty(
            request.CustomerName,
            userInfo?.FullName,
            TryGetShippingAddressPart(shippingAddress, 0),
            userInfo?.Username,
            "Khach hang")!;
        var customerPhone = FirstNonEmpty(
            request.CustomerPhone,
            userInfo?.Phone,
            TryGetShippingAddressPart(shippingAddress, 1),
            string.Empty)!;
        var customerEmail = FirstNonEmpty(request.CustomerEmail, userInfo?.Email);

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var subtotal = cartItems.Sum(ci => ci.UnitPrice * ci.Quantity);
            var orderCode = $"DH{DateTime.UtcNow:yyyyMMddHHmmss}";

            var order = new OrderEntity
            {
                OrderCode = orderCode,
                CustomerId = (decimal)userId,
                PromotionId = promotion?.Id,
                OrderSource = "ONLINE",
                OrderStatus = "PENDING",
                PaymentStatus = "PENDING",
                PaymentMethod = request.PaymentMethod,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                CustomerEmail = customerEmail,
                CustomerAddress = shippingAddress,
                Note = request.Note,
                SubtotalAmount = subtotal,
                DiscountAmount = discountAmount,
                ShippingFee = request.ShippingFee,
                TotalAmount = subtotal - discountAmount + request.ShippingFee,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _uow.Orders.AddAsync(order, ct);
            await _uow.SaveChangesAsync(ct);

            foreach (var ci in cartItems)
            {
                var product = productInfos[ci.ProductId];
                await _uow.OrderItems.AddAsync(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = ci.ProductId,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl ?? ci.ImageUrl,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    DiscountAmount = 0,
                    LineTotal = ci.UnitPrice * ci.Quantity,
                    CreatedAt = DateTime.UtcNow,
                }, ct);
            }

            if (promotion is not null)
            {
                promotion.UsedCount += 1;
                _uow.Promotions.Update(promotion);
            }

            foreach (var ci in cartItems)
                _uow.CartItems.Remove(ci);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            _logger.LogInformation("Order created: {OrderCode} for user {UserId}", orderCode, userId);

            var eventMsg = new netcore.Commons.Messages.Events.OrderCreatedEvent
            {
                OrderCode = order.OrderCode,
                CustomerId = (long?)order.CustomerId,
                CreatedAt = order.CreatedAt,
                Items = cartItems.Select(ci => new netcore.Commons.Messages.Events.OrderItemEventDto
                {
                    ProductId = (long)ci.ProductId,
                    Quantity = (int)ci.Quantity
                }).ToList()
            };
            await _publishEndpoint.Publish(eventMsg, ct);

            return await GetByIdAsync((long)order.Id, ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    // ─────────────────────────────────────────────
    // TẠO ĐƠN TRỰC TIẾP (POS / Admin)
    // ─────────────────────────────────────────────
    public async Task<OrderDto> CreateDirectOrderAsync(DirectOrderRequest request, CancellationToken ct = default)
    {
        decimal discountAmount = 0;
        Promotion? promotion = null;

        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            promotion = await _uow.Promotions.FirstOrDefaultAsync(
                p => p.PromoCode == request.PromoCode && p.Status == true, ct);
        }

        await _uow.BeginTransactionAsync(ct);
        try
        {
            decimal subtotal = 0;
            var lines = new List<(OrderLineRequest Line, ProductDtoInfo Product, decimal Price)>();

            foreach (var line in request.Items)
            {
                var product = await GetProductInfoAsync(line.ProductId, ct);

                if (product.StockQuantity < line.Quantity)
                    throw new MessageException($"'{product.Name}' không đủ hàng (còn {product.StockQuantity}).");

                var price = line.UnitPrice ?? product.SalePrice;
                subtotal += price * line.Quantity;
                lines.Add((line, product, price));
            }

            if (promotion is not null)
                discountAmount = CalculateDiscount(promotion, subtotal);

            var orderCode = $"DH{DateTime.UtcNow:yyyyMMddHHmmss}";
            var order = new OrderEntity
            {
                OrderCode = orderCode,
                CustomerId = request.CustomerId.HasValue ? (decimal)request.CustomerId.Value : null,
                PromotionId = promotion?.Id,
                OrderSource = request.Source,
                OrderStatus = request.Source == "POS" ? "COMPLETED" : "PENDING",
                PaymentStatus = request.Source == "POS" ? "PAID" : "PENDING",
                PaymentMethod = request.PaymentMethod,
                CustomerName = request.CustomerName ?? "Khách lẻ",
                CustomerPhone = request.CustomerPhone ?? string.Empty,
                CustomerAddress = request.CustomerAddress ?? "Tại quầy",
                Note = request.Note,
                SubtotalAmount = subtotal,
                DiscountAmount = discountAmount,
                ShippingFee = request.ShippingFee,
                TotalAmount = subtotal - discountAmount + request.ShippingFee,
                DeliveredAt = request.Source == "POS" ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _uow.Orders.AddAsync(order, ct);
            await _uow.SaveChangesAsync(ct);

            foreach (var (line, product, price) in lines)
            {
                await _uow.OrderItems.AddAsync(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = (decimal)line.ProductId,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Quantity = line.Quantity,
                    UnitPrice = price,
                    DiscountAmount = 0,
                    LineTotal = price * line.Quantity,
                    CreatedAt = DateTime.UtcNow,
                }, ct);
            }

            if (promotion is not null)
            {
                promotion.UsedCount += 1;
                _uow.Promotions.Update(promotion);
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            _logger.LogInformation("Direct order created: {OrderCode} source={Source}", orderCode, request.Source);

            var eventMsg = new netcore.Commons.Messages.Events.OrderCreatedEvent
            {
                OrderCode = order.OrderCode,
                CustomerId = (long?)order.CustomerId,
                CreatedAt = order.CreatedAt,
                Items = lines.Select(l => new netcore.Commons.Messages.Events.OrderItemEventDto
                {
                    ProductId = (long)l.Line.ProductId,
                    Quantity = (int)l.Line.Quantity
                }).ToList()
            };
            await _publishEndpoint.Publish(eventMsg, ct);

            return await GetByIdAsync((long)order.Id, ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    // ─────────────────────────────────────────────
    // ĐƠN HÀNG CỦA TÔI
    // ─────────────────────────────────────────────
    public async Task<PagedResult<OrderListDto>> GetMyOrdersAsync(long userId, OrderQuery query, CancellationToken ct = default)
    {
        query.CustomerId = userId;
        return await GetAllOrdersAsync(query, ct);
    }

    // ─────────────────────────────────────────────
    // TẤT CẢ ĐƠN (Admin)
    // ─────────────────────────────────────────────
    public async Task<PagedResult<OrderListDto>> GetAllOrdersAsync(OrderQuery query, CancellationToken ct = default)
    {
        var q = _uow.Orders.Query()
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            q = q.Where(o => o.OrderCode.Contains(query.Keyword) ||
                o.CustomerName.Contains(query.Keyword) ||
                (o.CustomerPhone != null && o.CustomerPhone.Contains(query.Keyword)));

        if (query.CustomerId.HasValue)
            q = q.Where(o => o.CustomerId == (decimal)query.CustomerId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(o => o.OrderStatus == query.Status);

        if (!string.IsNullOrWhiteSpace(query.PaymentMethod))
            q = q.Where(o => o.PaymentMethod == query.PaymentMethod);

        if (!string.IsNullOrWhiteSpace(query.PaymentStatus))
            q = q.Where(o => o.PaymentStatus == query.PaymentStatus);

        if (!string.IsNullOrWhiteSpace(query.OrderCode))
            q = q.Where(o => o.OrderCode.Contains(query.OrderCode));

        if (!string.IsNullOrWhiteSpace(query.Source))
            q = q.Where(o => o.OrderSource == query.Source);

        if (query.FromDate.HasValue)
            q = q.Where(o => o.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            q = q.Where(o => o.CreatedAt <= query.ToDate.Value);

        q = q.OrderByDescending(o => o.CreatedAt);

        var totalCount = await q.CountAsync(ct);
        var pageSize = Math.Min(query.PageSize, 100);
        var page = Math.Max(query.Page, 1);

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderListDto
            {
                Id = (long)o.Id,
                OrderCode = o.OrderCode,
                CustomerName = o.CustomerName,
                CustomerPhone = o.CustomerPhone,
                OrderStatus = o.OrderStatus,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                TotalAmount = o.TotalAmount,
                ItemCount = o.OrderItems.Count,
                Source = o.OrderSource,
                CreatedAt = o.CreatedAt,
            })
            .ToListAsync(ct);

        return new PagedResult<OrderListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    // ─────────────────────────────────────────────
    // CHI TIẾT THEO ORDER CODE
    // ─────────────────────────────────────────────
    public async Task<OrderDto> GetByOrderCodeAsync(string orderCode, CancellationToken ct = default)
    {
        var order = await _uow.Orders.Query()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode, ct)
            ?? throw new NotFoundException($"Không tìm thấy đơn hàng: {orderCode}");

        return MapToDto(order);
    }

    // ─────────────────────────────────────────────
    // CHI TIẾT THEO ID
    // ─────────────────────────────────────────────
    public async Task<OrderDto> GetByIdAsync(long orderId, CancellationToken ct = default)
    {
        var order = await _uow.Orders.Query()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == (decimal)orderId, ct)
            ?? throw new NotFoundException("Đơn hàng", orderId);

        return MapToDto(order);
    }

    // ─────────────────────────────────────────────
    // CẬP NHẬT TRẠNG THÁI (Admin)
    // ─────────────────────────────────────────────
    public async Task<OrderDto> UpdateStatusAsync(UpdateOrderStatusRequest request, CancellationToken ct = default)
    {
        var order = await _uow.Orders.FirstOrDefaultAsync(o => o.OrderCode == request.OrderCode, ct)
            ?? throw new NotFoundException($"Không tìm thấy đơn hàng: {request.OrderCode}");

        var prevStatus = order.OrderStatus;
        order.OrderStatus = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        if (request.Status == "CONFIRMED") order.ConfirmedAt = DateTime.UtcNow;
        if (request.Status == "SHIPPING") order.ShippedAt = DateTime.UtcNow;
        if (request.Status == "COMPLETED")
        {
            order.DeliveredAt = DateTime.UtcNow;
            order.PaymentStatus = "PAID";
        }
        if (request.Status == "CANCELLED") order.CancelledAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Note))
            order.Note = request.Note;

        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Order {OrderCode} status: {Old} → {New}", request.OrderCode, prevStatus, request.Status);
        return await GetByOrderCodeAsync(request.OrderCode, ct);
    }

    // ─────────────────────────────────────────────
    // HỦY ĐƠN (Customer)
    // ─────────────────────────────────────────────
    public async Task<OrderDto> CancelOrderAsync(long userId, CancelOrderRequest request, CancellationToken ct = default)
    {
        var order = await _uow.Orders.FirstOrDefaultAsync(
            o => o.OrderCode == request.OrderCode && o.CustomerId == (decimal)userId, ct)
            ?? throw new NotFoundException($"Không tìm thấy đơn hàng: {request.OrderCode}");

        if (order.OrderStatus != "PENDING")
            throw new MessageException($"Không thể hủy đơn hàng ở trạng thái '{order.OrderStatus}'.");

        order.OrderStatus = "CANCELLED";
        order.CancelledAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Reason))
            order.Note = request.Reason;

        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Order {OrderCode} cancelled by user {UserId}", request.OrderCode, userId);
        return await GetByOrderCodeAsync(request.OrderCode, ct);
    }

    // ─────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────
    private async Task<ProductDtoInfo> GetProductInfoAsync(long productId, CancellationToken ct)
    {
        var productApiUrl = _config["ProductServiceUrl"] ?? "http://localhost:5002";
        var apiKey = _config["ApiKey:Key"] ?? "fish-gateway-key-2026";
        var headerName = _config["ApiKey:HeaderName"] ?? "X-Api-Key";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{productApiUrl}/api/product/products/{productId}");
        requestMessage.Headers.Add(headerName, apiKey);

        var response = await _httpClient.SendAsync(requestMessage, ct);
        if (!response.IsSuccessStatusCode)
            throw new MessageException($"Không thể kết nối đến Product Service (HTTP {response.StatusCode}).");

        var productResult = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDtoInfo>>(cancellationToken: ct);
        if (productResult is null || !productResult.Success || productResult.Data is null)
            throw new NotFoundException("Sản phẩm", productId);

        return productResult.Data;
    }

    private async Task<UserDtoInfo?> GetUserInfoAsync(long userId, CancellationToken ct)
    {
        var userApiUrl = _config["UserServiceUrl"] ?? "http://localhost:5001";
        var apiKey = _config["ApiKey:Key"] ?? "fish-gateway-key-2026";
        var headerName = _config["ApiKey:HeaderName"] ?? "X-Api-Key";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{userApiUrl}/api/user/auth/me");
        requestMessage.Headers.Add(headerName, apiKey);
        requestMessage.Headers.Add("X-User-Id", userId.ToString());

        try
        {
            var response = await _httpClient.SendAsync(requestMessage, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Cannot get user profile from UserService. UserId={UserId}, Status={StatusCode}", userId, response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDtoInfo>>(cancellationToken: ct);
            return result is { Success: true, Data: not null } ? result.Data : null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Cannot connect to UserService when creating order for user {UserId}", userId);
            return null;
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "UserService request timed out when creating order for user {UserId}", userId);
            return null;
        }
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim();

    private static string? TryGetShippingAddressPart(string? shippingAddress, int index)
    {
        if (string.IsNullOrWhiteSpace(shippingAddress))
        {
            return null;
        }

        var parts = shippingAddress.Split('|', 3, StringSplitOptions.TrimEntries);
        return parts.Length > index ? parts[index] : null;
    }

    private static string? BuildAddress(params string?[] parts)
    {
        var address = string.Join(", ", parts
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p!.Trim()));

        return string.IsNullOrWhiteSpace(address) ? null : address;
    }

    private static decimal CalculateDiscount(Promotion promo, decimal subtotal)
    {
        if (subtotal < promo.MinOrderValue)
            return 0;

        decimal discount = promo.DiscountType == "PERCENT"
            ? subtotal * promo.DiscountValue / 100
            : promo.DiscountValue;

        if (promo.MaxDiscountValue.HasValue)
            discount = Math.Min(discount, promo.MaxDiscountValue.Value);

        return Math.Min(discount, subtotal);
    }

    private static OrderDto MapToDto(OrderEntity o) => new()
    {
        Id = (long)o.Id,
        OrderCode = o.OrderCode,
        CustomerId = (long)(o.CustomerId ?? 0),
        CustomerName = o.CustomerName,
        CustomerPhone = o.CustomerPhone,
        OrderStatus = o.OrderStatus,
        PaymentMethod = o.PaymentMethod,
        PaymentStatus = o.PaymentStatus,
        Subtotal = o.SubtotalAmount,
        DiscountAmount = o.DiscountAmount,
        ShippingFee = o.ShippingFee,
        TotalAmount = o.TotalAmount,
        ShippingAddress = o.CustomerAddress,
        Notes = o.Note,
        Source = o.OrderSource,
        CreatedAt = o.CreatedAt,
        UpdatedAt = o.UpdatedAt,
        Items = o.OrderItems.Select(oi => new OrderItemDto
        {
            Id = (long)oi.Id,
            ProductId = (long)oi.ProductId,
            ProductName = oi.ProductName,
            ImageUrl = oi.ImageUrl,
            Quantity = (int)oi.Quantity,
            UnitPrice = oi.UnitPrice,
            SubTotal = oi.LineTotal,
        }).ToList(),
    };

    private sealed class UserDtoInfo
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? AddressLine { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
    }
}
