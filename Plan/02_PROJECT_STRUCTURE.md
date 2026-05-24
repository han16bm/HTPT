# 📁 Cấu Trúc Thư Mục — FISH SHOP

> Theo phong cách dự án nguồn `byt_csdl_nkt` (NKT.Internal)

---

## Cấu Trúc Root

```
FISH_SHOP/
├── API/                              # Toàn bộ Backend .NET 8
│   ├── FishShop.sln                  # Solution file
│   │
│   ├── Gateway/                      # YARP Reverse Proxy
│   │   └── FishShop.Gateway/
│   │
│   ├── Services/                     # Các API service độc lập
│   │   ├── API.Auth/                 # Xác thực & phân quyền
│   │   ├── API.Products/             # Sản phẩm, danh mục, kho
│   │   ├── API.Orders/               # Giỏ hàng, đặt hàng, TT
│   │   ├── API.Admin/                # Dashboard, POS, báo cáo
│   │   └── API.Content/              # Blog, liên hệ
│   │
│   ├── netcore/                      # Shared libraries
│   │   ├── netcore.Commons/          # Hạ tầng & cross-cutting
│   │   └── netcore.Entities/         # Domain & Data Access
│   │
│   └── database/                     # SQL scripts
│       └── sqlserver_full_setup.sql  ✅ Tạo 4 database + seed dữ liệu mẫu
│
├── FE/                               # Admin Frontend
├── FE-Customer/                      # Customer Frontend
└── Plan/                             # Tài liệu kế hoạch
```

---

## Chi Tiết: `netcore.Commons`

```
netcore/netcore.Commons/
├── netcore.Commons.csproj
│
├── Attributes/
│   ├── ApiKeyAttribute.cs            ← Kiểm tra X-Api-Key header đến từ Gateway
│   └── AuditAttribute.cs             ← Ghi log thao tác (ai, lúc nào, làm gì)
│
├── Exceptions/
│   ├── MessageException.cs           ← Exception có message hiển thị cho client
│   ├── NotFoundException.cs          ← 404
│   └── UnauthorizedException.cs      ← 401
│
├── Extensions/
│   ├── KebabCaseRouteTokenTransformer.cs  ← ProductsController → /products
│   └── ApiPrefixRouteConvention.cs        ← Thêm prefix /api/{service-name}
│
├── Filters/
│   ├── TrimStringsActionFilter.cs         ← Trim string fields trong request
│   ├── FileSizeValidationFilter.cs        ← Validate file upload size
│   └── GlobalExceptionFilter.cs           ← Bắt exception → response chuẩn
│
├── Models/
│   ├── ApiResponse.cs                     ← { success, data, message, errors }
│   ├── PagedResult.cs                     ← { items, totalCount, page, totalPages }
│   └── ApiKeyOptions.cs                   ← Cấu hình [ApiKey]
│
└── Services/
    └── GatewayClient.cs                   ← HttpClient gọi cross-service
```

---

## Chi Tiết: `netcore.Entities`

```
netcore/netcore.Entities/
├── netcore.Entities.csproj              ← Refs: EF Core, SqlServer, BCrypt
│
├── Entities/
│   ├── BaseEntity.cs                    ← { Id, CreatedAt, UpdatedAt }
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
│   ├── OrderStatus.cs                   ← Pending/Confirmed/Shipping/Completed/Cancelled
│   ├── PaymentMethod.cs                 ← COD/BankTransfer/Cash
│   ├── PaymentStatus.cs                 ← Unpaid/Partial/Paid/Refunded
│   ├── DiscountType.cs                  ← Percent/Amount
│   ├── InventoryTransactionType.cs      ← Import/Export/Adjustment/Sale/Return
│   ├── BlogPostStatus.cs                ← Draft/Published/Hidden
│   └── ContactStatus.cs                 ← New/Read/Resolved/Closed
│
├── Persistence/
│   ├── AppDbContext.cs                  ← EF Core DbContext (SQL Server mapping)
│   └── Configurations/
│       ├── RoleConfiguration.cs
│       ├── UserConfiguration.cs
│       ├── CustomerProfileConfiguration.cs
│       ├── CustomerAddressConfiguration.cs
│       ├── CategoryConfiguration.cs
│       ├── ProductConfiguration.cs
│       ├── ProductImageConfiguration.cs
│       ├── InventoryTransactionConfiguration.cs
│       ├── PromotionConfiguration.cs
│       ├── ShoppingCartConfiguration.cs
│       ├── CartItemConfiguration.cs
│       ├── OrderConfiguration.cs
│       ├── OrderItemConfiguration.cs
│       ├── PaymentConfiguration.cs
│       ├── BlogCategoryConfiguration.cs
│       ├── BlogPostConfiguration.cs
│       └── ContactMessageConfiguration.cs
│
├── Interfaces/
│   ├── IRepository.cs                   ← Generic: GetById, GetAll, Add, Update, Delete
│   └── IUnitOfWork.cs                   ← SaveChangesAsync + repo properties
│
├── Repositories/
│   ├── GenericRepository.cs             ← Impl IRepository<T> dùng EF
│   └── UnitOfWork.cs                    ← Impl IUnitOfWork
│
└── Extensions/
    └── EntityServicesExtensions.cs      ← AddEntityServices(connString) — đk DbContext + UoW
```

---

## Template Cấu Trúc Mỗi `API.<ServiceName>`

> Áp dụng đồng nhất cho API.Auth, API.Products, API.Orders, API.Admin, API.Content

```
Services/API.<ServiceName>/
├── API.<ServiceName>.csproj
│
├── Program.cs                           ← Startup chuẩn NKT
├── DependencyInjection.cs               ← AddApplicationServices()
├── appsettings.json
├── appsettings.Development.json
│
├── Constants/
│   └── ServiceConstants.cs              ← Constants của service
│
├── Controllers/
│   ├── BaseApiController.cs             ← [Audit] protected string GetUserId()
│   └── {Resource}Controller.cs          ← [Audit][ApiKey][ApiController][Route("[controller]")]
│
├── Extensions/
│   ├── ServiceRegistrationExtensions.cs ← AddDependencyInjection() — IXxx → Xxx
│   ├── SerilogConfigurationExtensions.cs← AddSerilogConfiguration()
│   └── CorsConfigurationExtensions.cs   ← AddCorsConfiguration()
│
├── Filters/
│   └── NormalizeDateTimeFilter.cs       ← (nếu cần chuẩn hóa datetime)
│
├── Interfaces/
│   └── I{Resource}Service.cs
│
├── Models/
│   ├── DTOs/                            ← Response models (read)
│   │   ├── {Resource}Dto.cs
│   │   └── {Resource}ListDto.cs
│   ├── Commands/                        ← Request models (write)
│   │   └── {Action}{Resource}Request.cs
│   └── Queries/                         ← Filter/search parameters (read)
│       └── {Resource}Query.cs
│
├── Services/
│   └── {Resource}Service.cs             ← Impl IService, inject IUnitOfWork
│
├── Utils/
│   ├── ApiPrefixRouteConvention.cs      ← (sao chép từ netcore.Commons)
│   └── KebabCaseRouteTokenTransformer.cs
│
└── Properties/
    └── launchSettings.json
```

---

## Chi Tiết: `API.Auth`

```
Services/API.Auth/
├── API.Auth.csproj
├── Program.cs
├── DependencyInjection.cs
│
├── Constants/
│   └── AuthConstants.cs             ← Role codes: ADMIN, STAFF, CUSTOMER
│
├── Controllers/
│   ├── BaseApiController.cs
│   ├── AuthController.cs            ← [Route("[controller]")] → /api/user/auth/...
│   │   ├── login                POST
│   │   ├── register                  POST (customer only)
│   │   ├── refresh-token            POST
│   │   ├── logout                POST
│   │   ├── thong-tin-nguoi-dung     GET
│   │   └── healthcheck              GET
│   │
│   └── PermissionsController.cs     ← [Route("[controller]")] → /api/auth/permissions/...
│       └── check                    POST (Gateway gọi)
│
├── Extensions/
│   ├── ServiceRegistrationExtensions.cs
│   ├── SerilogConfigurationExtensions.cs
│   └── CorsConfigurationExtensions.cs
│
├── Interfaces/
│   ├── IAuthService.cs
│   ├── IJwtService.cs
│   └── IPasswordService.cs
│
├── Models/
│   ├── DTOs/
│   │   ├── AuthConfiguration.cs         ← Config model (connect string, JWT secret)
│   │   ├── NguoiDungDto.cs              ← User info response
│   │   ├── TokenDto.cs                  ← { accessToken, refreshToken, expiresIn }
│   │   └── PermissionValidationResponse.cs
│   ├── Commands/
│   │   ├── DangNhapRequest.cs
│   │   ├── DangKyRequest.cs
│   │   ├── LamMoiTokenRequest.cs
│   │   └── PermissionValidationRequest.cs
│   └── Queries/
│       └── (none — Auth không có query phức tạp)
│
├── Services/
│   ├── AuthService.cs               ← DangNhap, DangKy, LamMoiToken, DangXuat
│   ├── JwtService.cs                ← GenerateToken, ValidateToken
│   └── PasswordService.cs           ← HashPassword, VerifyPassword (BCrypt)
│
└── Utils/
    ├── ApiPrefixRouteConvention.cs
    └── KebabCaseRouteTokenTransformer.cs
```

---

## Chi Tiết: `API.Products`

```
Services/API.Products/
├── Controllers/
│   ├── ProductsController.cs
│   │   ├── list            GET  — filter: name, category, price range, isFeatured
│   │   ├── chi-tiet            GET  — by id
│   │   ├── theo-slug           GET  — by slug (SEO)
│   │   ├── san-pham-noi-bat    GET  — featured list
│   │   ├── them-moi-cap-nhat   POST — Admin/Staff
│   │   └── xoa                 POST — Admin
│   │
│   ├── CategoriesController.cs
│   │   ├── list            GET  — với cây con (parent/children)
│   │   ├── chi-tiet            GET
│   │   ├── theo-slug           GET
│   │   ├── them-moi-cap-nhat   POST — Admin
│   │   └── xoa                 POST — Admin
│   │
│   └── InventoryController.cs
│       ├── lich-su-giao-dich   GET  — filter: product, type, date range
│       ├── nhap-hang           POST — Admin
│       └── san-pham-sap-het    GET  — low stock alert
│
├── Interfaces/
│   ├── IProductService.cs
│   ├── ICategoryService.cs
│   └── IInventoryService.cs
│
├── Models/
│   ├── DTOs/
│   │   ├── ProductDto.cs              ← Full product (with images)
│   │   ├── ProductListDto.cs          ← Rút gọn (for list)
│   │   ├── CategoryDto.cs
│   │   └── InventoryTransactionDto.cs
│   ├── Commands/
│   │   ├── UpsertProductRequest.cs
│   │   ├── DeleteProductRequest.cs
│   │   ├── UpsertCategoryRequest.cs
│   │   └── NhapHangRequest.cs
│   └── Queries/
│       ├── ProductQuery.cs            ← page, pageSize, name, categoryId, minPrice...
│       ├── CategoryQuery.cs
│       └── InventoryQuery.cs
│
└── Services/
    ├── ProductService.cs
    ├── CategoryService.cs
    └── InventoryService.cs
```

---

## Chi Tiết: `API.Orders`

```
Services/API.Orders/
├── Controllers/
│   ├── CartController.cs
│   │   ├── gio-hang-hien-tai   GET  — lấy giỏ hàng hiện tại
│   │   ├── them-san-pham       POST — thêm vào giỏ
│   │   ├── cap-nhat-so-luong   POST — sửa qty
│   │   ├── xoa-san-pham        POST — xóa item
│   │   └── xoa-gio-hang        POST — clear cart
│   │
│   ├── OrdersController.cs
│   │   ├── create            POST — tạo đơn (public + auth)
│   │   ├── me    GET  — lịch sử đơn (auth)
│   │   ├── chi-tiet            GET  — xem chi tiết đơn
│   │   ├── huy-don             POST — hủy (auth + only PENDING)
│   │   ├── list            GET  — Admin: filter đơn
│   │   └── cap-nhat-trang-thai POST — Admin: cập nhật status
│   │
│   ├── PaymentsController.cs
│   │   ├── tao-giao-dich       POST — tạo payment record
│   │   └── trang-thai          GET  — kiểm tra trạng thái GD
│   │
│   └── PromotionsController.cs
│       ├── validate         POST — validate promo code (public)
│       ├── list            GET  — Admin: danh sách
│       ├── them-moi-cap-nhat   POST — Admin: CRUD
│       └── xoa                 POST — Admin
│
├── Interfaces/
│   ├── ICartService.cs
│   ├── IOrderService.cs
│   ├── IPaymentService.cs
│   └── IPromotionService.cs
│
├── Models/
│   ├── DTOs/
│   │   ├── CartDto.cs
│   │   ├── CartItemDto.cs
│   │   ├── OrderDto.cs
│   │   ├── OrderListDto.cs
│   │   ├── OrderItemDto.cs
│   │   ├── PaymentDto.cs
│   │   └── PromotionDto.cs
│   ├── Commands/
│   │   ├── ThemSanPhamVaoGioRequest.cs
│   │   ├── CapNhatSoLuongRequest.cs
│   │   ├── DatHangRequest.cs
│   │   ├── HuyDonRequest.cs
│   │   ├── CapNhatTrangThaiDonRequest.cs  ← Admin
│   │   ├── TaoGiaoDichRequest.cs
│   │   ├── KiemTraMaRequest.cs
│   │   └── UpsertPromotionRequest.cs
│   └── Queries/
│       ├── OrderQuery.cs                  ← Admin filter
│       ├── PromotionQuery.cs
│       └── CartQuery.cs
│
└── Services/
    ├── CartService.cs
    ├── OrderService.cs          ← Xử lý đặt hàng: order + trừ kho + inventory TX
    ├── PaymentService.cs
    └── PromotionService.cs
```

---

## Chi Tiết: `API.Admin`

```
Services/API.Admin/
├── Controllers/
│   ├── DashboardController.cs
│   │   ├── thong-ke-tong-quat  GET  — stats cards
│   │   └── bieu-do             GET  — chart data ?days=7
│   │
│   ├── SalesController.cs
│   │   ├── ban-hang-tai-quay   POST — POS create order (source=POS)
│   │   └── thong-ke            GET  — sales stats
│   │
│   ├── CustomersController.cs
│   │   ├── list            GET
│   │   ├── chi-tiet            GET
│   │   └── lich-su-don-hang    GET  — lịch sử mua của khách
│   │
│   └── ReportsController.cs
│       ├── doanh-thu           GET  — ?fromDate=&toDate=&groupBy=day/month
│       ├── san-pham-ban-chay   GET  — ?limit=10
│       └── tong-hop-don-hang   GET
│
├── Interfaces/
│   ├── IDashboardService.cs
│   ├── ISalesService.cs
│   ├── IAdminCustomerService.cs
│   └── IReportService.cs
│
├── Models/
│   ├── DTOs/
│   │   ├── DashboardStatsDto.cs
│   │   ├── ChartDataDto.cs
│   │   └── RevenueReportDto.cs
│   └── Queries/
│       ├── ReportQuery.cs          ← fromDate, toDate, groupBy
│       └── CustomerQuery.cs
│
└── Services/
    ├── DashboardService.cs
    ├── SalesService.cs             ← Tái dùng IOrderService từ API.Orders? Hoặc gọi trực tiếp
    ├── AdminCustomerService.cs
    └── ReportService.cs
```

---

## Chi Tiết: `API.Content`

```
Services/API.Content/
├── Controllers/
│   ├── BlogController.cs
│   │   ├── list            GET  — public: only PUBLISHED
│   │   ├── chi-tiet-theo-slug  GET  — public
│   │   ├── chi-tiet            GET  — Admin (any status)
│   │   ├── them-moi-cap-nhat   POST — Admin
│   │   └── xoa                 POST — Admin
│   │
│   ├── BlogCategoriesController.cs
│   │   ├── list            GET
│   │   ├── them-moi-cap-nhat   POST — Admin
│   │   └── xoa                 POST — Admin
│   │
│   └── ContactController.cs
│       ├── gui-lien-he         POST — public
│       ├── list            GET  — Admin
│       └── cap-nhat-trang-thai POST — Admin
│
├── Services/
│   ├── BlogService.cs
│   └── ContactService.cs
│
└── Models/
    ├── DTOs/
    │   ├── BlogPostDto.cs
    │   ├── BlogListDto.cs
    │   └── ContactMessageDto.cs
    ├── Commands/
    │   ├── UpsertBlogPostRequest.cs
    │   ├── GuiLienHeRequest.cs
    │   └── CapNhatTrangThaiLienHeRequest.cs
    └── Queries/
        ├── BlogQuery.cs
        └── ContactQuery.cs
```

---

## Chi Tiết: `FishShop.Gateway`

```
Gateway/FishShop.Gateway/
├── FishShop.Gateway.csproj          ← Yarp.ReverseProxy, Serilog
├── Program.cs                       ← YARP + Middleware + CORS
├── appsettings.json
├── appsettings.Development.json
│
├── Config/
│   ├── gateway.common.json          ← Routes, Clusters, Gateway config
│   └── Services/                    ← Config theo từng service (theo env)
│       ├── gateway.auth.Development.json
│       ├── gateway.products.Development.json
│       └── ...
│
├── Configuration/
│   └── GatewayOptions.cs            ← POCO mapping gateway config
│
├── Middleware/
│   ├── PermissionValidationMiddleware.cs   ← Gọi /permissions/check
│   ├── SetApiKeyMiddleware.cs              ← Gắn X-Api-Key cho service đích
│   └── SlowRequestLoggingMiddleware.cs     ← Log request > threshold
│
├── RateLimiting/
│   └── GatewayRateLimiter.cs
│
├── Services/
│   ├── ITokenValidator.cs
│   └── TokenValidator.cs            ← Xác thực JWT signature
│
└── Transforms/
    └── GatewayTransformProvider.cs  ← Thêm headers (X-User-Id, X-JWT-Payload...)
```

---

## Quy Ước Đặt Tên (Theo NKT)

| Loại | Quy tắc | Ví dụ |
|------|---------|-------|
| Controller | `{Resource}Controller` | `ProductsController` |
| Route Action | kebab-case tiếng Việt | `list`, `chi-tiet`, `them-moi-cap-nhat`, `xoa` |
| Interface Service | `I{Resource}Service` | `IProductService` |
| Service Impl | `{Resource}Service` | `ProductService` |
| Request (write) | `{Action}{Resource}Request` | `UpsertProductRequest`, `DatHangRequest` |
| Query (read) | `{Resource}Query` | `ProductQuery`, `OrderQuery` |
| DTO (response) | `{Resource}Dto` | `ProductDto`, `OrderDto` |
| List DTO | `{Resource}ListDto` | `ProductListDto` |
| Config POCO | `{Service}Configuration` | `ProductsConfiguration`, `AuthConfiguration` |
