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

namespace API.Order.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<OrderService> _logger;
    private readonly MassTransit.IPublishEndpoint _publishEndpoint;

    public OrderService(IUnitOfWork uow, ILogger<OrderService> logger, MassTransit.IPublishEndpoint publishEndpoint)
    {
        _uow = uow;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    // ─────────────────────────────────────────────
    // TẠO ĐƠN TỪ GIỎ HÀNG
    // ─────────────────────────────────────────────
    public async Task<OrderDto> CreateOrderFromCartAsync(long userId, CreateOrderRequest request, CancellationToken ct = default)
    {
        // Lấy customer profile
        var customer = await _uow.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == (decimal)userId, ct)
            ?? throw new MessageException("Không tìm thấy thông tin khách hàng. Vui lòng cập nhật hồ sơ.");

        // Lấy giỏ hàng
        var cart = await _uow.ShoppingCarts.FirstOrDefaultAsync(c => c.CustomerId == customer.Id, ct)
            ?? throw new MessageException("Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi đặt hàng.");

        var cartItems = await _uow.CartItems.Query()
            .Include(ci => ci.Product)
            .Where(ci => ci.CartId == cart.Id)
            .ToListAsync(ct);

        if (!cartItems.Any())
            throw new MessageException("Giỏ hàng trống.");

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

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var subtotal = cartItems.Sum(ci => ci.UnitPrice * ci.Quantity);
            var orderCode = $"DH{DateTime.UtcNow:yyyyMMddHHmmss}";

            var order = new OrderEntity
            {
                OrderCode = orderCode,
                CustomerId = customer.Id,
                PromotionId = promotion?.Id,
                OrderSource = "ONLINE",
                OrderStatus = "PENDING",
                PaymentStatus = "PENDING",
                PaymentMethod = request.PaymentMethod,
                CustomerName = customer.FullName,
                CustomerPhone = customer.Phone,
                CustomerEmail = customer.Email,
                CustomerAddress = request.ShippingAddress,
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

            // Tạo order items + trừ kho + ghi inventory TX
            foreach (var ci in cartItems)
            {
                if (ci.Product!.StockQuantity < ci.Quantity)
                    throw new MessageException($"Sản phẩm '{ci.Product.Name}' không đủ hàng (còn {ci.Product.StockQuantity}).");

                await _uow.OrderItems.AddAsync(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product!.Name,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    DiscountAmount = 0,
                    LineTotal = ci.UnitPrice * ci.Quantity,
                    CreatedAt = DateTime.UtcNow,
                }, ct);

                // Trừ kho
                ci.Product.StockQuantity -= ci.Quantity;
                ci.Product.SoldQuantity += ci.Quantity;
                _uow.Products.Update(ci.Product);

                // Ghi inventory transaction
                await _uow.InventoryTransactions.AddAsync(new InventoryTransaction
                {
                    ProductId = ci.ProductId,
                    TransactionType = "SALE",
                    Quantity = -ci.Quantity,
                    UnitCost = ci.UnitPrice,
                    ReferenceType = "ORDER",
                    ReferenceId = order.Id,
                    Note = $"Đơn hàng {orderCode}",
                    CreatedAt = DateTime.UtcNow,
                }, ct);
            }

            // Cập nhật lần dùng promotion
            if (promotion is not null)
            {
                promotion.UsedCount += 1;
                _uow.Promotions.Update(promotion);
            }

            // Xóa giỏ hàng
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
        // Nếu có CustomerId → lấy profile, nếu không → dùng thông tin truyền vào
        CustomerProfile? customer = null;
        if (request.CustomerId.HasValue)
        {
            customer = await _uow.CustomerProfiles.FirstOrDefaultAsync(p => p.Id == (decimal)request.CustomerId.Value, ct)
                ?? throw new NotFoundException("Khách hàng", request.CustomerId.Value);
        }

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
            var lines = new List<(OrderLineRequest line, Product product)>();

            foreach (var line in request.Items)
            {
                var product = await _uow.Products.FirstOrDefaultAsync(p => p.Id == (decimal)line.ProductId && p.Status == true, ct)
                    ?? throw new NotFoundException("Sản phẩm", line.ProductId);

                if (product.StockQuantity < line.Quantity)
                    throw new MessageException($"'{product.Name}' không đủ hàng (còn {product.StockQuantity}).");

                var price = line.UnitPrice ?? product.SalePrice;
                subtotal += price * line.Quantity;
                lines.Add((line, product));
            }

            if (promotion is not null)
                discountAmount = CalculateDiscount(promotion, subtotal);

            var orderCode = $"DH{DateTime.UtcNow:yyyyMMddHHmmss}";
            var order = new OrderEntity
            {
                OrderCode = orderCode,
                CustomerId = customer?.Id,
                PromotionId = promotion?.Id,
                OrderSource = request.Source,
                OrderStatus = request.Source == "POS" ? "COMPLETED" : "PENDING",
                PaymentStatus = request.Source == "POS" ? "PAID" : "PENDING",
                PaymentMethod = request.PaymentMethod,
                CustomerName = customer?.FullName ?? request.CustomerName ?? "Khách lẻ",
                CustomerPhone = customer?.Phone ?? request.CustomerPhone ?? string.Empty,
                CustomerEmail = customer?.Email,
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

            foreach (var (line, product) in lines)
            {
                var price = line.UnitPrice ?? product.SalePrice;
                await _uow.OrderItems.AddAsync(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = (decimal)line.ProductId,
                    ProductName = product.Name,
                    Quantity = line.Quantity,
                    UnitPrice = price,
                    DiscountAmount = 0,
                    LineTotal = price * line.Quantity,
                    CreatedAt = DateTime.UtcNow,
                }, ct);

                product.StockQuantity -= line.Quantity;
                product.SoldQuantity += line.Quantity;
                _uow.Products.Update(product);

                await _uow.InventoryTransactions.AddAsync(new InventoryTransaction
                {
                    ProductId = (decimal)line.ProductId,
                    TransactionType = "SALE",
                    Quantity = -line.Quantity,
                    UnitCost = price,
                    ReferenceType = "ORDER",
                    ReferenceId = order.Id,
                    Note = $"Đơn {orderCode} ({request.Source})",
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
                    ProductId = (long)l.line.ProductId,
                    Quantity = (int)l.line.Quantity
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
        var customer = await _uow.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == (decimal)userId, ct)
            ?? throw new MessageException("Khong tim thay thong tin khach hang. Vui long cap nhat ho so.");

        query.CustomerId = (long)customer.Id;
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
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
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
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
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
        var customer = await _uow.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == (decimal)userId, ct)
            ?? throw new UnauthorizedException("Không tìm thấy hồ sơ khách hàng");

        var order = await _uow.Orders.FirstOrDefaultAsync(
            o => o.OrderCode == request.OrderCode && o.CustomerId == customer.Id, ct)
            ?? throw new NotFoundException($"Không tìm thấy đơn hàng: {request.OrderCode}");

        if (order.OrderStatus != "PENDING")
            throw new MessageException($"Không thể hủy đơn hàng ở trạng thái '{order.OrderStatus}'.");

        order.OrderStatus = "CANCELLED";
        order.CancelledAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Reason))
            order.Note = request.Reason;

        _uow.Orders.Update(order);

        // Hoàn lại kho
        var items = await _uow.OrderItems.Query()
            .Include(oi => oi.Product)
            .Where(oi => oi.OrderId == order.Id)
            .ToListAsync(ct);

        foreach (var item in items)
        {
            if (item.Product is not null)
            {
                item.Product.StockQuantity += item.Quantity;
                item.Product.SoldQuantity = Math.Max(0, item.Product.SoldQuantity - item.Quantity);
                _uow.Products.Update(item.Product);

                await _uow.InventoryTransactions.AddAsync(new InventoryTransaction
                {
                    ProductId = item.ProductId,
                    TransactionType = "RETURN",
                    Quantity = item.Quantity,
                    ReferenceType = "ORDER",
                    ReferenceId = order.Id,
                    Note = $"Hoàn hàng do hủy đơn {order.OrderCode}",
                    CreatedAt = DateTime.UtcNow,
                }, ct);
            }
        }

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Order {OrderCode} cancelled by user {UserId}", request.OrderCode, userId);

        return await GetByOrderCodeAsync(request.OrderCode, ct);
    }

    // ─────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────
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
            ProductName = oi.Product?.Name ?? oi.ProductName,
            ImageUrl = oi.Product?.ImageUrl,
            Quantity = (int)oi.Quantity,
            UnitPrice = oi.UnitPrice,
            SubTotal = oi.LineTotal,
        }).ToList(),
    };
}
