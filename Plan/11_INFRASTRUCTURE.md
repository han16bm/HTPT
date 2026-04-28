# 🏗️ Hạ Tầng Bổ Sung — FISH SHOP

> Cloudinary (lưu ảnh) · RabbitMQ (xử lý đơn hàng bất đồng bộ)

---

## I. Lưu Trữ Ảnh — Cloudinary

### Tại Sao Chọn Cloudinary?

| Tiêu chí | Cloudinary | AWS S3 | Self-hosted |
|----------|-----------|--------|-------------|
| **Free tier** | ✅ 25GB storage + 25GB bandwidth/tháng | ❌ Tính phí từ đầu | ✅ Miễn phí nhưng tốn server |
| **Dễ tích hợp** | ✅ SDK .NET + React sẵn | ⚠️ Cần config IAM phức tạp | ❌ Phải tự code |
| **CDN toàn cầu** | ✅ Tích hợp sẵn | ✅ (thêm CloudFront) | ❌ Không |
| **Transform ảnh** | ✅ Resize/crop/format qua URL | ❌ Cần Lambda | ❌ Không |
| **Phù hợp dự án** | ✅ **Recommended** | ⚠️ Overkill | ❌ |

---

### Cloudinary Free Tier

```
Storage  : 25 GB
Bandwidth: 25 GB / tháng
Requests : Unlimited
Transform: 25 credits / tháng
→ Đủ cho giai đoạn development + MVP
```

---

### Cách Tích Hợp Backend (.NET 8)

#### Cài Package
```xml
<!-- Trong API.Products.csproj hoặc netcore.Commons.csproj -->
<PackageReference Include="CloudinaryDotNet" Version="1.*" />
```

#### Config
```json
// appsettings.json
{
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret",
    "Folder": "fish-shop"
  }
}
```

#### Service Interface (trong `netcore.Commons`)
```csharp
// netcore/netcore.Commons/Services/IFileStorageService.cs
public interface IFileStorageService
{
    /// <summary>Upload ảnh, trả về URL công khai</summary>
    Task<FileUploadResult> UploadImageAsync(IFormFile file, string folder, CancellationToken ct = default);

    /// <summary>Xóa ảnh theo publicId</summary>
    Task<bool> DeleteImageAsync(string publicId, CancellationToken ct = default);
}

public record FileUploadResult(
    bool Success,
    string? Url,          // URL đầy đủ
    string? PublicId,     // Cloudinary public_id (để xóa sau)
    string? Message = null
);
```

#### Service Implementation
```csharp
// netcore/netcore.Commons/Services/CloudinaryFileStorageService.cs
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly string _defaultFolder;
    private readonly ILogger<CloudinaryFileStorageService> _logger;

    public CloudinaryFileStorageService(
        IConfiguration configuration,
        ILogger<CloudinaryFileStorageService> logger)
    {
        _logger = logger;
        var section = configuration.GetSection("Cloudinary");
        var account = new Account(
            section["CloudName"],
            section["ApiKey"],
            section["ApiSecret"]);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
        _defaultFolder = section["Folder"] ?? "fish-shop";
    }

    public async Task<FileUploadResult> UploadImageAsync(
        IFormFile file, string folder, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return new FileUploadResult(false, null, null, "File rỗng");

        // Validate extension
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return new FileUploadResult(false, null, null, $"Định dạng {ext} không được hỗ trợ");

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = $"{_defaultFolder}/{folder}",
            // Tự động tối ưu chất lượng
            Transformation = new Transformation()
                .Quality("auto")
                .FetchFormat("auto"),
            // Tên file = timestamp để tránh trùng
            PublicId = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Path.GetFileNameWithoutExtension(file.FileName)}"
        };

        try
        {
            var result = await _cloudinary.UploadAsync(uploadParams, ct);

            if (result.Error != null)
            {
                _logger.LogError("Cloudinary upload error: {Error}", result.Error.Message);
                return new FileUploadResult(false, null, null, result.Error.Message);
            }

            return new FileUploadResult(true, result.SecureUrl.ToString(), result.PublicId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload ảnh thất bại: {FileName}", file.FileName);
            return new FileUploadResult(false, null, null, ex.Message);
        }
    }

    public async Task<bool> DeleteImageAsync(string publicId, CancellationToken ct = default)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }
}
```

#### Đăng Ký DI (trong `EntityServicesExtensions` hoặc `ServiceRegistrationExtensions`)
```csharp
// netcore.Commons: thêm AddFileStorageService()
public static IServiceCollection AddFileStorageService(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddSingleton<IFileStorageService, CloudinaryFileStorageService>();
    return services;
}
```

---

### Sử Dụng Trong Controller (API.Products)

```csharp
// API.Products/Controllers/ProductsController.cs

// POST /api/products/products/upload-anh
[HttpPost("upload-anh")]
[Consumes("multipart/form-data")]
public async Task<object> UploadAnhSanPham(
    [FromForm] long productId,
    IFormFile file,
    CancellationToken ct)
{
    var result = await _fileService.UploadImageAsync(file, "products", ct);
    if (!result.Success)
        return BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });

    // Lưu URL vào PRODUCT_IMAGES
    await _productService.AddImageAsync(productId, result.Url!, result.PublicId!, ct);

    return Ok(new ApiResponse<object>
    {
        Success = true,
        Data = new { url = result.Url, publicId = result.PublicId }
    });
}

// POST /api/products/products/xoa-anh
[HttpPost("xoa-anh")]
public async Task<object> XoaAnhSanPham([FromBody] XoaAnhRequest request, CancellationToken ct)
{
    // Xóa trên Cloudinary
    await _fileService.DeleteImageAsync(request.PublicId, ct);
    // Xóa record trong PRODUCT_IMAGES
    await _productService.RemoveImageAsync(request.ImageId, ct);
    return Ok(new ApiResponse<object> { Success = true });
}
```

---

### URL Transform (Cloudinary URL Magic)

```typescript
// FE: Tự resize ảnh qua URL parameter — KHÔNG cần backend
const getOptimizedUrl = (cloudinaryUrl: string, width: number, height?: number) => {
  // Chèn transform vào URL trước /upload/
  return cloudinaryUrl.replace(
    '/upload/',
    `/upload/w_${width}${height ? `,h_${height}` : ''},c_fill,q_auto,f_auto/`
  );
};

// Sử dụng
<img
  src={getOptimizedUrl(product.imageUrl, 400, 400)}
  srcSet={`
    ${getOptimizedUrl(product.imageUrl, 400)} 400w,
    ${getOptimizedUrl(product.imageUrl, 800)} 800w
  `}
  alt={product.name}
  loading="lazy"
/>
```

---

### Cấu Trúc Folder Cloudinary

```
fish-shop/
├── products/          ← Ảnh sản phẩm
├── categories/        ← Ảnh danh mục
├── blog/              ← Ảnh thumbnail blog
└── avatars/           ← Ảnh đại diện user
```

---

## II. Xử Lý Đơn Hàng Bất Đồng Bộ — RabbitMQ

### Vấn Đề Cần Giải Quyết

```
❌ Vấn đề: Nhiều khách đặt hàng cùng lúc (Flash Sale, sự kiện)
   - N request đồng thời → API.Orders → Oracle → race condition tồn kho
   - Database bị quá tải
   - Timeout → khách bức xúc

✅ Giải pháp: Message Queue (RabbitMQ)
   - API nhận đơn → validate cơ bản → push vào Queue → trả về "Đang xử lý"
   - Worker kéo từng đơn → xử lý tuần tự → cập nhật trạng thái
   - No race condition, no timeout
```

---

### Luồng Xử Lý Với RabbitMQ

```
FE: POST /api/orders/orders/dat-hang
             │
             ▼
    API.Orders (OrdersController)
             │ 1. Validate request cơ bản (thiếu trường, ...)
             │ 2. Validate sơ bộ tồn kho (đọc nhanh)
             │ 3. Validate mã giảm giá (nếu có)
             │
             ▼
    RabbitMQ Exchange: "fish-shop.orders"
    Queue: "order.created"
             │
             │ Return ngay: { orderCode: "FS...", status: "PENDING", message: "Đang xử lý đơn hàng" }
             │
             ↓ (Async - background)
    ┌─────────────────────────────────┐
    │    OrderWorkerService           │
    │    (Background Service / IHostedService) │
    │                                 │
    │  Consume từ queue:              │
    │  1. Lock tồn kho (Oracle FOR UPDATE NOWAIT) │
    │  2. Trừ STOCK_QUANTITY          │
    │  3. Tạo ORDER + ORDER_ITEMS     │
    │  4. Ghi INVENTORY_TRANSACTIONS  │
    │  5. Xóa SHOPPING_CART           │
    │  6. Publish "order.processed"   │
    └─────────────────────────────────┘
             │
             ▼
    Queue: "order.processed"
             │
             ↓ (Tương lai)
    EmailNotificationWorker → Gửi email xác nhận
```

---

### Cài Package

```xml
<!-- API.Orders.csproj hoặc Worker project -->
<PackageReference Include="RabbitMQ.Client" Version="6.*" />
<!-- Hoặc dùng MassTransit (abstraction layer cao hơn, dễ hơn) -->
<PackageReference Include="MassTransit" Version="8.*" />
<PackageReference Include="MassTransit.RabbitMQ" Version="8.*" />
```

> **Gợi ý:** Dùng **MassTransit** thay vì RabbitMQ.Client trực tiếp — API đơn giản hơn nhiều, ít boilerplate, dễ test.

---

### Cấu Hình (Theo NKT Style)

```json
// appsettings.json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  },
  "OrderQueue": {
    "Name": "order.created",
    "PrefetchCount": 5
  }
}
```

---

### Message Classes (trong `netcore.Entities` hoặc `API.Orders/Messages/`)

```csharp
// API.Orders/Messages/OrderCreatedMessage.cs
namespace API.Orders.Messages
{
    /// <summary>Message đẩy vào RabbitMQ khi nhận đơn hàng</summary>
    public record OrderCreatedMessage
    {
        public string OrderCode { get; init; } = default!;
        public long? CustomerId { get; init; }
        public string CustomerName { get; init; } = default!;
        public string CustomerPhone { get; init; } = default!;
        public string CustomerAddress { get; init; } = default!;
        public string? PromoCode { get; init; }
        public string PaymentMethod { get; init; } = default!;
        public string OrderSource { get; init; } = "ONLINE";
        public string? Note { get; init; }
        public List<OrderItemMessage> Items { get; init; } = [];
        public DateTime ReceivedAt { get; init; } = DateTime.UtcNow;
    }

    public record OrderItemMessage
    {
        public long ProductId { get; init; }
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
    }
}
```

---

### API.Orders — OrdersController (Publish Message)

```csharp
// API.Orders/Controllers/OrdersController.cs
using MassTransit;
using API.Orders.Messages;

[Audit]
[ApiKey]
[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderValidationService _validationService;
    private readonly IBus _bus;                           // MassTransit
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderValidationService validationService,
        IBus bus,
        ILogger<OrdersController> logger)
    {
        _validationService = validationService;
        _bus = bus;
        _logger = logger;
    }

    // POST /api/orders/orders/dat-hang
    [HttpPost("dat-hang")]
    public async Task<object> DatHang([FromBody] DatHangRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Nhận đơn hàng từ {Source}", request.OrderSource);

        // 1. Validate request
        var validation = await _validationService.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Thông tin đơn hàng không hợp lệ",
                Errors = validation.Errors
            });

        // 2. Generate order code
        var orderCode = GenerateOrderCode();

        // 3. Publish message → RabbitMQ (KHÔNG chờ xử lý xong)
        var message = new OrderCreatedMessage
        {
            OrderCode    = orderCode,
            CustomerId   = request.CustomerId,
            CustomerName = request.CustomerName,
            CustomerPhone= request.CustomerPhone,
            CustomerAddress = request.CustomerAddress,
            PromoCode    = request.PromoCode,
            PaymentMethod= request.PaymentMethod,
            OrderSource  = request.OrderSource,
            Note         = request.Note,
            Items        = request.Items.Select(i => new OrderItemMessage
            {
                ProductId = i.ProductId,
                Quantity  = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        await _bus.Publish(message, ct);

        _logger.LogInformation("Đẩy đơn {OrderCode} vào queue thành công", orderCode);

        // 4. Trả về ngay — không chờ xử lý
        return Accepted(new ApiResponse<object>
        {
            Success = true,
            Data = new
            {
                orderCode,
                status    = "PENDING",
                message   = "Đơn hàng đang được xử lý, vui lòng chờ trong giây lát."
            }
        });
    }

    private static string GenerateOrderCode()
        => $"FS{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
}
```

---

### OrderWorkerService — Xử Lý Trong Background

```csharp
// API.Orders/Workers/OrderCreatedConsumer.cs
using MassTransit;
using API.Orders.Messages;

/// <summary>Worker tiêu thụ message từ RabbitMQ và xử lý đơn hàng thực sự</summary>
public class OrderCreatedConsumer : IConsumer<OrderCreatedMessage>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(IOrderService orderService, ILogger<OrderCreatedConsumer> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Bắt đầu xử lý đơn {OrderCode}", msg.OrderCode);

        try
        {
            await _orderService.ProcessOrderAsync(msg, context.CancellationToken);
            _logger.LogInformation("Xử lý đơn {OrderCode} thành công", msg.OrderCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xử lý đơn {OrderCode}", msg.OrderCode);
            // MassTransit tự retry (có thể config retry policy)
            throw; // re-throw để MassTransit xử lý retry/dead-letter
        }
    }
}
```

---

### OrderService.ProcessOrderAsync — Xử Lý Thực Tế

```csharp
// API.Orders/Services/OrderService.cs (phần xử lý thực tế)
public async Task ProcessOrderAsync(OrderCreatedMessage msg, CancellationToken ct)
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
    try
    {
        // 1. Kiểm tra & khóa tồn kho (Oracle: SELECT FOR UPDATE NOWAIT)
        foreach (var item in msg.Items)
        {
            var product = await _dbContext.Products
                .FromSqlRaw("SELECT * FROM PRODUCTS WHERE ID = :id FOR UPDATE NOWAIT", item.ProductId)
                .FirstOrDefaultAsync(ct)
                ?? throw new MessageException($"Sản phẩm ID={item.ProductId} không tồn tại");

            if (product.StockQuantity < item.Quantity)
                throw new MessageException($"Sản phẩm '{product.Name}' không đủ số lượng. Còn: {product.StockQuantity}");

            // 2. Trừ tồn kho
            product.StockQuantity -= item.Quantity;
            product.SoldQuantity  += item.Quantity;
        }

        // 3. Tính promotion (nếu có)
        decimal discountAmount = 0;
        if (!string.IsNullOrEmpty(msg.PromoCode))
        {
            var promo = await _dbContext.Promotions
                .FirstOrDefaultAsync(p => p.PromoCode == msg.PromoCode && p.Status == 1, ct);
            if (promo != null)
            {
                discountAmount = promo.DiscountType == "PERCENT"
                    ? Math.Min(msg.SubtotalAmount * promo.DiscountValue / 100, promo.MaxDiscountValue ?? decimal.MaxValue)
                    : promo.DiscountValue;

                promo.UsedCount++;
            }
        }

        // 4. Tạo bản ghi ORDER
        var order = new Order
        {
            OrderCode       = msg.OrderCode,
            CustomerId      = msg.CustomerId,
            OrderStatus     = OrderStatus.Pending,
            PaymentStatus   = PaymentStatus.Unpaid,
            PaymentMethod   = Enum.Parse<PaymentMethod>(msg.PaymentMethod),
            CustomerName    = msg.CustomerName,
            CustomerPhone   = msg.CustomerPhone,
            CustomerAddress = msg.CustomerAddress,
            Note            = msg.Note,
            SubtotalAmount  = msg.Items.Sum(i => i.Quantity * i.UnitPrice),
            DiscountAmount  = discountAmount,
            ShippingFee     = 30000, // TODO: tính phí ship
            OrderSource     = msg.OrderSource == "POS" ? "POS" : "ONLINE"
        };
        order.TotalAmount = order.SubtotalAmount - order.DiscountAmount + order.ShippingFee;

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(ct); // Lấy order.Id

        // 5. Tạo ORDER_ITEMS
        foreach (var item in msg.Items)
        {
            var product = await _dbContext.Products.FindAsync(item.ProductId);
            _dbContext.OrderItems.Add(new OrderItem
            {
                OrderId     = order.Id,
                ProductId   = item.ProductId,
                ProductName = product!.Name,
                Quantity    = item.Quantity,
                UnitPrice   = item.UnitPrice,
                LineTotal   = item.Quantity * item.UnitPrice
            });
        }

        // 6. Ghi INVENTORY_TRANSACTIONS
        foreach (var item in msg.Items)
        {
            _dbContext.InventoryTransactions.Add(new InventoryTransaction
            {
                ProductId       = item.ProductId,
                TransactionType = InventoryTransactionType.Sale,
                Quantity        = -item.Quantity, // âm = xuất kho
                ReferenceType   = "ORDER",
                ReferenceId     = order.Id
            });
        }

        // 7. Xóa giỏ hàng (nếu có customer đăng nhập)
        if (msg.CustomerId.HasValue)
        {
            var cart = await _dbContext.ShoppingCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == msg.CustomerId && c.Status == "ACTIVE", ct);

            if (cart != null)
            {
                _dbContext.CartItems.RemoveRange(cart.Items);
                cart.Status = "CHECKED_OUT";
            }
        }

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
    catch
    {
        await transaction.RollbackAsync(ct);
        // Cập nhật order status = FAILED (nếu order đã được tạo)
        throw;
    }
}
```

---

### Đăng Ký MassTransit + RabbitMQ trong Program.cs

```csharp
// API.Orders/Program.cs — thêm vào sau AddApplicationServices()
using MassTransit;

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Đăng ký consumer
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        // Cấu hình queue và retry
        cfg.ReceiveEndpoint("order.created", e =>
        {
            e.PrefetchCount = 5; // Xử lý tối đa 5 đơn cùng lúc

            // Retry: 3 lần, mỗi lần cách 5 giây
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

            // Sau 3 lần retry thất bại → vào Dead Letter Queue
            e.ConfigureConsumer<OrderCreatedConsumer>(ctx);
        });

        cfg.ConfigureEndpoints(ctx);
    });
});
```

---

### Trạng Thái Đơn Hàng — Polling từ FE

Vì `dat-hang` trả về ngay (không chờ xử lý), FE cần polling để biết kết quả:

```typescript
// FE-Customer: sau khi nhận { orderCode, status: "PENDING" }
const pollOrderStatus = async (orderCode: string) => {
  const MAX_POLLS = 10;
  const INTERVAL_MS = 2000; // 2 giây / lần

  for (let i = 0; i < MAX_POLLS; i++) {
    await sleep(INTERVAL_MS);

    const res = await orderService.getByCode(orderCode);
    if (res.success && res.data) {
      const status = res.data.orderStatus;

      if (status === 'PENDING') continue; // Chờ tiếp

      if (status === 'CONFIRMED') {
        router.push(`/my-orders/${orderCode}?success=true`);
        return;
      }

      if (status === 'CANCELLED') {
        showError('Đơn hàng bị hủy: ' + res.data.cancelReason);
        return;
      }
    }
  }

  // Vẫn PENDING sau 20s → thông báo chờ thêm
  showInfo('Đơn hàng đang được xử lý. Bạn có thể kiểm tra trong Lịch sử đơn hàng.');
};
```

---

### Cài RabbitMQ (Dev Local)

```bash
# Option A: Docker (khuyến nghị)
docker run -d \
  --name fish-rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=guest \
  -e RABBITMQ_DEFAULT_PASS=guest \
  rabbitmq:3-management

# Management UI: http://localhost:15672 (guest/guest)

# Option B: Windows installer
# Download: https://www.rabbitmq.com/install-windows.html
# Yêu cầu: Erlang OTP cài trước
```

---

## III. Cập Nhật Cấu Trúc Backend

### Thêm Vào `API.Orders`

```
Services/API.Orders/
├── ...
├── Messages/                    ← [MỚI] Message classes cho RabbitMQ
│   ├── OrderCreatedMessage.cs
│   └── OrderProcessedMessage.cs ← Publish sau khi xử lý xong
│
└── Workers/                     ← [MỚI] Background consumers
    └── OrderCreatedConsumer.cs
```

### Thêm Vào `netcore.Commons`

```
netcore/netcore.Commons/
├── ...
└── Services/
    ├── IFileStorageService.cs         ← [MỚI] Interface upload ảnh
    └── CloudinaryFileStorageService.cs← [MỚI] Cloudinary implementation
```

---

## IV. Tóm Tắt Packages Cần Cài Thêm

### `netcore.Commons`
```xml
<PackageReference Include="CloudinaryDotNet" Version="1.*" />
```

### `API.Orders`
```xml
<PackageReference Include="MassTransit" Version="8.*" />
<PackageReference Include="MassTransit.RabbitMQ" Version="8.*" />
```

---

## V. Biến Môi Trường Bổ Sung

### Backend
```json
// appsettings.Development.json — thêm vào
{
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "123456789012345",
    "ApiSecret": "your-api-secret",
    "Folder": "fish-shop-dev"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest"
  }
}
```

### Frontend
```env
# .env.development — thêm vào
VITE_CLOUDINARY_CLOUD_NAME=your-cloud-name
VITE_CLOUDINARY_UPLOAD_PRESET=fish-shop-preset  # unsigned preset (optional)
```

---

## VI. Thứ Tự Khởi Động Dev (Cập Nhật)

```
1. 🗄️  Start Oracle Database        (localhost:1521)
2. 🐰  Start RabbitMQ               (localhost:5672, UI: 15672)
3. ⚙️  Start API.Auth               (port 5001)
4. ⚙️  Start API.Products           (port 5002)
5. ⚙️  Start API.Orders             (port 5003 — có OrderCreatedConsumer)
6. ⚙️  Start API.Admin              (port 5004)
7. ⚙️  Start API.Content            (port 5005)
8. 🌐  Start FishShop.Gateway       (port 8080)
9. 🌐  Start FE-Customer            (port 5173)
10. 🌐 Start FE-Admin               (port 5174)
```
