# ⚙️ Kế Hoạch Tái Cấu Trúc Backend — FISH SHOP

> Theo phong cách dự án nguồn `byt_csdl_nkt` (NKT.Internal)
> .NET 8 · EF Core + Oracle · Multi-service architecture

---

## So Sánh: Cũ vs Mới

| | Kiến trúc cũ (Clean Arch + CQRS) | Kiến trúc mới (theo NKT style) |
|-|-----------------------------------|-------------------------------|
| Tổ chức | `Domain / CQRS / Infrastructure / API` | `netcore.Commons` + `netcore.Entities` + N × `API.<Service>` |
| Handler | MediatR CQRS | Service pattern trực tiếp (`IService / Service`) |
| DI | `AddInfrastructureServices()` tập trung | `AddApplicationServices()` + `AddEntityServices()` tách biệt |
| Config | `appsettings.json` thuần | Config lấy dynamically từ `API.Configuration` (hoặc appsettings) |
| Controller | Generic base + specific | `[Audit][ApiKey][ApiController][Route("[controller]")]` + `BaseApiController` |
| Routing | Mặc định | KebabCase + `ApiPrefixRouteConvention` |
| Logging | Serilog extension method | Serilog qua `SerilogConfigurationExtensions` |
| CORS | Inline | Qua `CorsConfigurationExtensions` |
| Models | Commands + Queries (MediatR) | `Models/Commands/`, `Models/Queries/`, `Models/DTOs/` (plain class) |
| Gateway | Ocelot/YARP đơn giản | YARP + `PermissionValidationMiddleware` + `SetApiKeyMiddleware` |

---

## Cấu Trúc Solution Mới

```
FISH_SHOP/API/
│
├── FishShop.sln                         ← Solution chính
│
├── Gateway/
│   └── FishShop.Gateway/                ← YARP Reverse Proxy (port 8080)
│
├── Services/                            ← Các API service độc lập
│   ├── API.Auth/                        ← Xác thực, JWT (port 5001)
│   ├── API.Products/                    ← Sản phẩm & Danh mục (port 5002)
│   ├── API.Orders/                      ← Giỏ hàng, Đặt hàng (port 5003)
│   ├── API.Admin/                       ← Dashboard, Kho, Báo cáo (port 5004)
│   └── API.Content/                     ← Blog, Liên hệ (port 5005)
│
└── netcore/                             ← Thư viện dùng chung
    ├── netcore.Commons/                 ← Attributes, Exceptions, Extensions, Middleware
    └── netcore.Entities/                ← Entities, EF DbContext, Repositories
```

---

## Chi Tiết: `netcore.Commons` — Thư viện hạ tầng dùng chung

> Không có business logic — chỉ là infrastructure & cross-cutting concerns

```
netcore/netcore.Commons/
├── netcore.Commons.csproj
│
├── Attributes/
│   ├── ApiKeyAttribute.cs               ← [ApiKey] — xác thực API key từ Gateway
│   └── AuditAttribute.cs                ← [Audit] — ghi log thao tác
│
├── Exceptions/
│   ├── MessageException.cs              ← Exception có message hiển thị được
│   ├── NotFoundException.cs             ← 404 exception
│   └── UnauthorizedException.cs         ← 401 exception
│
├── Extensions/
│   ├── KebabCaseRouteTokenTransformer.cs   ← Chuyển route sang kebab-case
│   └── ApiPrefixRouteConvention.cs         ← Thêm prefix /api/{service-name}
│
├── Filters/
│   ├── TrimStringsActionFilter.cs       ← Trim whitespace cho string inputs
│   ├── FileSizeValidationFilter.cs      ← Validate kích thước file upload
│   └── GlobalExceptionFilter.cs         ← Xử lý exception thành response chuẩn
│
├── Middleware/
│   └── RequestLoggingMiddleware.cs      ← Log request/response (dùng Serilog)
│
├── Models/
│   ├── ApiResponse.cs                   ← { Success, Data, Message, Errors }
│   ├── PagedResult.cs                   ← { Items, TotalCount, Page, PageSize }
│   └── ApiKeyOptions.cs                 ← Options cho [ApiKey] attribute
│
└── Services/
    └── GatewayClient.cs                 ← HTTP client gọi sang các service khác
```

---

## Chi Tiết: `netcore.Entities` — Domain & Data Access

> Entities + EF Core DbContext + Repositories — mapping với Oracle schema có sẵn

```
netcore/netcore.Entities/
├── netcore.Entities.csproj
│
├── Entities/
│   ├── BaseEntity.cs                    ← Id, CreatedAt, UpdatedAt
│   ├── Role.cs
│   ├── User.cs
│   ├── CustomerProfile.cs
│   ├── CustomerAddress.cs
│   ├── Category.cs
│   ├── Product.cs
│   ├── ProductImage.cs
│   ├── InventoryTransaction.cs
│   ├── Promotion.cs
│   ├── PromotionProduct.cs
│   ├── ShoppingCart.cs
│   ├── CartItem.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── Payment.cs
│   ├── BlogCategory.cs
│   ├── BlogPost.cs
│   └── ContactMessage.cs
│
├── Enums/
│   ├── OrderStatus.cs
│   ├── PaymentMethod.cs
│   ├── PaymentStatus.cs
│   ├── DiscountType.cs
│   ├── InventoryTransactionType.cs
│   ├── BlogPostStatus.cs
│   └── ContactStatus.cs
│
├── Persistence/
│   ├── AppDbContext.cs                  ← EF Core DbContext (Oracle)
│   └── Configurations/                  ← IEntityTypeConfiguration<T> cho từng entity
│       ├── RoleConfiguration.cs
│       ├── UserConfiguration.cs
│       ├── CustomerProfileConfiguration.cs
│       ├── CategoryConfiguration.cs
│       ├── ProductConfiguration.cs
│       ├── OrderConfiguration.cs
│       └── ... (1 file / entity)
│
├── Interfaces/
│   ├── IRepository.cs                   ← Generic CRUD interface
│   └── IUnitOfWork.cs
│
├── Repositories/
│   ├── GenericRepository.cs             ← Base impl dùng EF
│   └── UnitOfWork.cs
│
└── Extensions/
    └── EntityServicesExtensions.cs      ← AddEntityServices() — đăng ký DbContext + Repos
```

---

## Chi Tiết: Mỗi API Service (chuẩn NKT)

> Template áp dụng đồng nhất cho `API.Auth`, `API.Products`, `API.Orders`, `API.Admin`, `API.Content`

```
Services/API.<ServiceName>/
├── API.<ServiceName>.csproj
│
├── Program.cs                           ← Startup theo chuẩn NKT
├── DependencyInjection.cs               ← AddApplicationServices()
├── appsettings.json
├── appsettings.Development.json
│
├── Constants/
│   └── ServiceConstants.cs              ← Constants nội bộ của service
│
├── Controllers/
│   ├── BaseApiController.cs             ← [Audit] base class
│   └── {Resource}Controller.cs          ← [Audit][ApiKey][ApiController][Route("[controller]")]
│
├── Extensions/
│   ├── ServiceRegistrationExtensions.cs ← AddDependencyInjection() — đăng ký IService → Service
│   ├── SerilogConfigurationExtensions.cs← AddSerilogConfiguration()
│   └── CorsConfigurationExtensions.cs   ← AddCorsConfiguration()
│
├── Filters/
│   └── NormalizeDateTimeFilter.cs       ← (nếu cần)
│
├── Interfaces/
│   └── I{Resource}Service.cs            ← Interface của service
│
├── Models/
│   ├── DTOs/                            ← Response DTOs (đọc/hiển thị)
│   │   └── {Resource}Dto.cs
│   ├── Commands/                        ← Request models (ghi/thay đổi)
│   │   └── {Action}{Resource}Request.cs
│   └── Queries/                         ← Filter/search models (đọc với tham số)
│       └── {Resource}Query.cs
│
├── Services/
│   └── {Resource}Service.cs             ← Impl của IService, gọi DbContext trực tiếp
│
└── Utils/
    ├── ApiPrefixRouteConvention.cs       ← (copy từ netcore.Commons nếu chưa tách lib)
    └── KebabCaseRouteTokenTransformer.cs ← (copy từ netcore.Commons nếu chưa tách lib)
```

---

## Program.cs Chuẩn (Theo NKT Pattern)

```csharp
// API.Products/Program.cs
using API.Products;
using API.Products.Filters;
using API.Products.Models.DTOs;
using API.Products.Utils;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using netcore.Commons.Attributes;
using netcore.Commons.Extensions;
using netcore.Entities;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Cấu hình Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});
builder.Services.AddHttpClient();

// Đọc config cho service này (giống NKT: đọc từ DB hoặc appsettings)
// Option A: appsettings thông thường (đơn giản)
var productConfig = configuration.GetSection("ProductsService").Get<ProductsConfiguration>()
    ?? throw new InvalidOperationException("ProductsService config missing");
services.AddSingleton(productConfig);

// Option B: giống hệt NKT (đọc config từ API.Configuration qua gateway)
// builder.Services.AddConfigurationLoader<ProductsConfiguration>();
// using var tempProvider = services.BuildServiceProvider();
// var configService = tempProvider.GetRequiredService<ConfigurationLoader<ProductsConfiguration>>();
// var fetchedConfig = await configService.GetAsync();
// var connectionString = fetchedConfig.CONNECTION_STRING;
// services.AddSingleton(fetchedConfig);

// Đăng ký services
services.AddApplicationServices(configuration);             // Services, Serilog, CORS
services.AddEntityServices(configuration, connectionString); // DbContext, Repos

// Controllers với routing chuẩn NKT
builder.Services.AddControllers();
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseRouteTokenTransformer()));
    options.Conventions.Add(new ApiPrefixRouteConvention());
    options.Filters.Add<TrimStringsActionFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ApiKey (từ Gateway)
builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection("ApiKey"));

var app = builder.Build();

// Pipeline — theo đúng thứ tự NKT
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "Swagger UI - API.Products";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API.Products V1");
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseCors("AllowOrigin");
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.Run();
```

---

## DependencyInjection.cs Chuẩn

```csharp
// API.Products/DependencyInjection.cs
namespace API.Products
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Đăng ký tất cả services
            services.AddDependencyInjection(configuration);

            // Cấu hình logging (Serilog)
            services.AddSerilogConfiguration(configuration);

            // Cấu hình CORS
            services.AddCorsConfiguration();

            return services;
        }
    }
}
```

---

## ServiceRegistrationExtensions.cs Chuẩn

```csharp
// API.Products/Extensions/ServiceRegistrationExtensions.cs
namespace API.Products.Extensions
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddDependencyInjection(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton(TimeProvider.System);
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddAuthentication();
            services.AddAuthorization();

            // Đăng ký services theo interface
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductImageService, ProductImageService>();

            return services;
        }
    }
}
```

---

## Controller Chuẩn (Theo NKT)

```csharp
// API.Products/Controllers/ProductsController.cs
using API.Products.Interfaces;
using API.Products.Models.Commands;
using API.Products.Models.Queries;
using Microsoft.AspNetCore.Mvc;
using netcore.Commons.Attributes;

namespace API.Products.Controllers
{
    [Audit]
    [ApiKey]
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService service, ILogger<ProductsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET /api/product/products?name=guppy&page=1&pageSize=10
        [HttpGet]
        public async Task<object> TimKiemSanPham([FromQuery] ProductQuery query, CancellationToken ct)
        {
            return await _service.GetAllAsync(query, ct);
        }

        // GET /api/product/products/1
        [HttpGet("chi-tiet")]
        public async Task<object> ChiTietSanPham([FromQuery] long id, CancellationToken ct)
        {
            return await _service.GetByIdAsync(id, ct);
        }

        // GET /api/product/products/slug/ca-betta
        [HttpGet("theo-slug")]
        public async Task<object> SanPhamTheoSlug([FromQuery] string slug, CancellationToken ct)
        {
            return await _service.GetBySlugAsync(slug, ct);
        }

        // POST /api/product/products
        [HttpPost("them-moi-cap-nhat")]
        public async Task<object> ThemMoiCapNhatSanPham([FromBody] UpsertProductRequest request, CancellationToken ct)
        {
            return await _service.UpsertAsync(request, ct);
        }

        // POST /api/product/products/{id}
        [HttpPost("xoa")]
        public async Task<object> XoaSanPham([FromBody] DeleteProductRequest request, CancellationToken ct)
        {
            return await _service.DeleteAsync(request, ct);
        }

        // GET /api/product/products/health
        [HttpGet("healthcheck")]
        public Task<object> HealthCheck()
        {
            return Task.FromResult<object>(new { Status = "Healthy", Service = "API.Products" });
        }
    }
}
```

---

## Mapping Các API Service → Chức Năng

### `API.Auth` (port 5001)
Phục vụ cả Customer lẫn Admin login
```
Controllers/
├── AuthController.cs         → /api/user/auth/...
│   ├── login             → POST (Customer + Staff + Admin)
│   ├── register               → POST (Customer)
│   ├── refresh-token         → POST (refresh JWT)
│   ├── logout             → POST
│   ├── thong-tin-nguoi-dung  → GET
│   └── healthcheck           → GET
│
└── PermissionsController.cs  → /api/auth/permissions/...
    └── check                 → POST (Gateway gọi validate quyền)
```

### `API.Products` (port 5002)
Sản phẩm + Danh mục + Tồn kho
```
Controllers/
├── ProductsController.cs     → /api/product/products/...
│   ├── list              → GET (filter, paging)
│   ├── chi-tiet              → GET (by id)
│   ├── theo-slug             → GET (by slug)
│   ├── them-moi-cap-nhat     → POST (upsert)
│   ├── xoa                   → POST
│   └── san-pham-noi-bat      → GET
│
├── CategoriesController.cs   → /api/product/categories/...
│   ├── list              → GET
│   ├── chi-tiet              → GET
│   ├── them-moi-cap-nhat     → POST
│   └── xoa                   → POST
│
└── InventoryController.cs    → /api/products/inventory/...
    ├── lich-su-giao-dich     → GET
    ├── nhap-hang             → POST
    └── san-pham-sap-het-hang → GET
```

### `API.Orders` (port 5003)
Giỏ hàng + Đặt hàng + Thanh toán + Khuyến mãi
```
Controllers/
├── CartController.cs         → /api/order/cart/...
│   ├── gio-hang-hien-tai     → GET
│   ├── them-san-pham         → POST
│   ├── cap-nhat-so-luong     → POST
│   ├── xoa-san-pham          → POST
│   └── xoa-gio-hang          → POST (clear)
│
├── OrdersController.cs       → /api/order/orders/...
│   ├── create              → POST (create order)
│   ├── me      → GET (customer's orders)
│   ├── chi-tiet              → GET (by order code)
│   ├── huy-don               → POST
│   └── cap-nhat-trang-thai   → POST (Admin)
│
├── PaymentsController.cs     → /api/orders/payments/...
│   ├── tao-giao-dich         → POST
│   └── trang-thai            → GET
│
└── PromotionsController.cs   → /api/order/promotions/...
    ├── validate            → POST (validate promo code)
    ├── list               → GET (Admin)
    └── them-moi-cap-nhat      → POST (Admin)
```

### `API.Admin` (port 5004)
Dashboard + Báo cáo + Quản lý POS
```
Controllers/
├── DashboardController.cs    → /api/admin/dashboard/...
│   ├── thong-ke-tong-quat    → GET
│   └── bieu-do               → GET
│
├── SalesController.cs        → /api/admin/sales/...
│   ├── ban-hang-tai-quay     → POST (POS)
│   └── thong-ke              → GET
│
├── CustomersController.cs    → /api/admin/customers/...
│   ├── list              → GET
│   ├── chi-tiet              → GET
│   └── don-hang              → GET (customer's order history)
│
└── ReportsController.cs      → /api/admin/reports/...
    ├── doanh-thu             → GET
    ├── san-pham-ban-chay     → GET
    └── tong-hop-don-hang     → GET
```

### `API.Content` (port 5005)
Blog + Liên hệ
```
Controllers/
├── BlogController.cs         → /api/content/blogs/...
│   ├── list              → GET
│   ├── chi-tiet              → GET (by slug)
│   ├── them-moi-cap-nhat     → POST (Admin)
│   └── xoa                   → POST (Admin)
│
├── BlogCategoriesController.cs → /api/content/blog-categories/...
│
└── ContactController.cs      → /api/content/contacts/...
    ├── gui-lien-he           → POST (public)
    ├── list              → GET (Admin)
    └── cap-nhat-trang-thai   → POST (Admin)
```

---

## Gateway (`FishShop.Gateway`) — YARP Config

### Config/gateway.common.json (mẫu)
```json
{
  "Gateway": {
    "SlowRequestWarningThresholdMs": 1000,
    "EnableResponseCompression": true,
    "JwtForwarding": {
      "Enabled": true,
      "HeaderName": "X-JWT-Payload",
      "userIdHeaderName": "X-User-Id",
      "userIdClaim": "sub",
      "userRoleClaim": "role"
    },
    "UnauthenticatedRoutes": [
      { "pathPrefix": "/api/user/auth/login", "methods": ["POST", "OPTIONS"] },
      { "pathPrefix": "/api/user/auth/register", "methods": ["POST", "OPTIONS"] },
      { "pathPrefix": "/api/user/auth/refresh-token", "methods": ["POST"] },
      { "pathPrefix": "/api/product/products" },
      { "pathPrefix": "/api/product/products/{id}" },
      { "pathPrefix": "/api/product/products/slug/{slug}" },
      { "pathPrefix": "/api/product/categories" },
      { "pathPrefix": "/api/content/blogs" },
      { "pathPrefix": "/api/order/promotions/validate" },
      { "pathPrefix": "/api/order/orders" },
      { "pathPrefix": "/api/content/contacts" },
      { "pathPrefix": "/api/user/auth/health" },
      { "pathPrefix": "/api/product/products/health" },
      { "pathPrefix": "/api/order/orders/health" },
      { "pathPrefix": "/api/admin/dashboard/healthcheck" },
      { "pathPrefix": "/api/content/blogs/health" }
    ],
    "validatePermissionEndpoint": "http://localhost:5001/api/user/permissions/check",
    "tokenValidation": {
      "validateLifetime": true,
      "signingKey": "{{JWT_SECRET_KEY}}",
      "allowedIssuers": ["FishShop"],
      "allowedAudiences": ["FishShop"]
    }
  },
  "ReverseProxy": {
    "Routes": {
      "auth-route": {
        "ClusterId": "auth-cluster",
        "Match": { "Path": "/api/auth/{**catch-all}" }
      },
      "products-route": {
        "ClusterId": "products-cluster",
        "Match": { "Path": "/api/products/{**catch-all}" }
      },
      "orders-route": {
        "ClusterId": "orders-cluster",
        "Match": { "Path": "/api/orders/{**catch-all}" }
      },
      "admin-route": {
        "ClusterId": "admin-cluster",
        "Match": { "Path": "/api/admin/{**catch-all}" }
      },
      "content-route": {
        "ClusterId": "content-cluster",
        "Match": { "Path": "/api/content/{**catch-all}" }
      }
    },
    "Clusters": {
      "auth-cluster": {
        "Destinations": {
          "primary": { "Address": "http://localhost:5001" }
        }
      },
      "products-cluster": {
        "Destinations": {
          "primary": { "Address": "http://localhost:5002" }
        }
      },
      "orders-cluster": {
        "Destinations": {
          "primary": { "Address": "http://localhost:5003" }
        }
      },
      "admin-cluster": {
        "Destinations": {
          "primary": { "Address": "http://localhost:5004" }
        }
      },
      "content-cluster": {
        "Destinations": {
          "primary": { "Address": "http://localhost:5005" }
        }
      }
    }
  }
}
```

---

## URL Pattern Sau Khi Áp KebabCase + ApiPrefix

Cú pháp: `/api/{service-name}/{controller-name}/{action}`

| Gọi từ FE | Service | Controller | Action |
|-----------|---------|------------|--------|
| `POST /api/user/auth/login` | API.Auth | AuthController | DangNhap |
| `GET /api/product/products` | API.Products | ProductsController | TimKiem |
| `GET /api/product/products/slug/{slug}` | API.Products | ProductsController | TheoSlug |
| `GET /api/product/categories` | API.Products | CategoriesController | TimKiem |
| `POST /api/order/cart/items` | API.Orders | CartController | ThemSanPham |
| `POST /api/order/orders` | API.Orders | OrdersController | DatHang |
| `GET /api/order/orders/me` | API.Orders | OrdersController | DonHangCuaToi |
| `GET /api/admin/dashboard/thong-ke-tong-quat` | API.Admin | DashboardController | ThongKeTongQuat |
| `POST /api/admin/sales/ban-hang-tai-quay` | API.Admin | SalesController | BanHangTaiQuay |
| `GET /api/content/blogs` | API.Content | BlogController | TimKiem |
| `POST /api/content/contacts` | API.Content | ContactController | GuiLienHe |

---

## Packages cần cài cho từng project

### netcore.Commons
```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.*" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.*" />
<PackageReference Include="Serilog.Sinks.File" Version="6.*" />
<PackageReference Include="Microsoft.AspNetCore.App" />
```

### netcore.Entities
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.*" />
<PackageReference Include="Oracle.EntityFrameworkCore" Version="8.21.*" />
<PackageReference Include="BCrypt.Net-Next" Version="4.*" />
```

### Mỗi API.{Service}
```xml
<ProjectReference Include="../../netcore/netcore.Commons/netcore.Commons.csproj" />
<ProjectReference Include="../../netcore/netcore.Entities/netcore.Entities.csproj" />
<PackageReference Include="Serilog.AspNetCore" Version="8.*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.*" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.*" /> <!-- chỉ API.Auth -->
```

### FishShop.Gateway
```xml
<PackageReference Include="Yarp.ReverseProxy" Version="2.*" />
<PackageReference Include="Serilog.AspNetCore" Version="8.*" />
```

---

## Thứ Tự Tái Cấu Trúc BE

```
Bước 1 — Tạo solution mới và cấu trúc thư mục
  [ ] Tạo FishShop.sln
  [ ] Tạo thư mục Gateway/, Services/, netcore/
  [ ] Tạo các .csproj cho netcore.Commons và netcore.Entities

Bước 2 — Implement netcore.Entities (từ schema Oracle có sẵn)
  [ ] Tất cả Entity classes (mapper từ SQL schema)
  [ ] EF Core IEntityTypeConfiguration cho từng bảng (Oracle uppercase)
  [ ] AppDbContext với Oracle connection
  [ ] GenericRepository + UnitOfWork
  [ ] EntityServicesExtensions.cs — AddEntityServices()

Bước 3 — Implement netcore.Commons
  [ ] ApiResponse, PagedResult
  [ ] ApiKeyAttribute, AuditAttribute
  [ ] MessageException, NotFoundException
  [ ] KebabCaseRouteTokenTransformer, ApiPrefixRouteConvention
  [ ] TrimStringsActionFilter, GlobalExceptionFilter
  [ ] SerilogConfigurationExtensions, CorsConfigurationExtensions

Bước 4 — API.Auth (service đầu tiên)
  [ ] Program.cs — theo chuẩn NKT
  [ ] DependencyInjection.cs
  [ ] AuthConfiguration.cs (model config)
  [ ] Models: DangNhapRequest, TokenDTO, NguoiDungDTO
  [ ] IAuthService + AuthService (login, register, refresh, logout)
  [ ] IPasswordService + PasswordService (BCrypt)
  [ ] IJwtService + JwtService
  [ ] AuthController (login, register, refresh-token, logout)
  [ ] PermissionsController (check endpoint cho Gateway)
  [ ] ServiceRegistrationExtensions.cs

Bước 5 — API.Products
  [ ] ProductsConfiguration.cs
  [ ] Models: ProductDto, ProductListDto, UpsertProductRequest, ProductQuery, CategoryDto, ...
  [ ] IProductService + ProductService (gọi DbContext qua UnitOfWork)
  [ ] ICategoryService + CategoryService
  [ ] IInventoryService + InventoryService
  [ ] Controllers: ProductsController, CategoriesController, InventoryController
  [ ] ServiceRegistrationExtensions.cs

Bước 6 — API.Orders
  [ ] Models: CartDto, OrderDto, CreateOrderRequest, PaymentDto, ...
  [ ] ICartService + CartService
  [ ] IOrderService + OrderService (tạo đơn, trừ kho, ghi inventory TX)
  [ ] IPaymentService + PaymentService
  [ ] IPromotionService + PromotionService
  [ ] Controllers: CartController, OrdersController, PaymentsController, PromotionsController

Bước 7 — API.Admin
  [ ] IDashboardService + DashboardService
  [ ] ISalesService + SalesService (POS)
  [ ] IReportService + ReportService
  [ ] Controllers: DashboardController, SalesController, CustomersController, ReportsController

Bước 8 — API.Content
  [ ] IBlogService + BlogService
  [ ] IContactService + ContactService
  [ ] Controllers: BlogController, BlogCategoriesController, ContactController

Bước 9 — FishShop.Gateway
  [ ] Program.cs (YARP + Serilog + Permission middleware)
  [ ] Config/gateway.common.json (routes + clusters)
  [ ] Middleware: PermissionValidationMiddleware, SetApiKeyMiddleware
  [ ] Services: TokenValidator
```

---

## Điều Chỉnh FE Khi Đổi API URL

Sau tái cấu trúc, FE cần cập nhật base URL:

```typescript
// FE/src/api/constants.ts — cập nhật prefix
export const API_ENDPOINTS = {
  // Auth → /api/user/auth/...
  LOGIN:         '/auth/auth/login',
  REGISTER:      '/auth/auth/register',
  REFRESH_TOKEN: '/auth/auth/refresh-token',
  LOGOUT:        '/auth/auth/logout',

  // Products → /api/product/products/...
  PRODUCTS:      '/products/products/list',
  PRODUCT_DETAIL: (id: number) => `/products/products/chi-tiet?id=${id}`,
  PRODUCT_SLUG:  (slug: string) => `/products/products/theo-slug?slug=${slug}`,
  CATEGORIES:    '/products/categories/list',

  // Orders → /api/orders/...
  CART:          '/orders/cart/gio-hang-hien-tai',
  ADD_TO_CART:   '/orders/cart/them-san-pham',
  CREATE_ORDER:  '/orders/orders/create',
  MY_ORDERS:     '/orders/orders/me',

  // Admin → /api/admin/...
  DASHBOARD:     '/admin/dashboard/thong-ke-tong-quat',
  POS_SALE:      '/admin/sales/ban-hang-tai-quay',

  // Content → /api/content/...
  BLOG:          '/content/blog/list',
  CONTACT_SEND:  '/content/contact/gui-lien-he',
} as const;
```
