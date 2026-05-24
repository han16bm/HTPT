# 🏗️ Kiến Trúc Hệ Thống — FISH SHOP

> Theo phong cách dự án `byt_csdl_nkt` (NKT.Internal)

---

## Tổng Quan Kiến Trúc

```
┌─────────────────────────────────────────────────────────────────┐
│                          CLIENT TIER                            │
│                                                                 │
│   ┌──────────────────────┐   ┌──────────────────────────────┐  │
│   │     FE-Customer       │   │         FE-Admin              │  │
│   │  (React+Vite+TS)     │   │     (React+Vite+TS)           │  │
│   │  localhost:5173      │   │     localhost:5174             │  │
│   │                      │   │                               │  │
│   │ • Mua sắm online     │   │ • Dashboard & thống kê       │  │
│   │ • Tìm kiếm SP        │   │ • Quản lý SP/Danh mục       │  │
│   │ • Giỏ hàng/Checkout  │   │ • Quản lý đơn hàng          │  │
│   │ • Theo dõi đơn       │   │ • Bán hàng POS               │  │
│   │ • Blog & tư vấn      │   │ • Báo cáo doanh thu          │  │
│   └──────────┬───────────┘   └─────────────┬────────────────┘  │
└──────────────┼─────────────────────────────┼───────────────────┘
               │  HTTPS / REST API           │
               │    (qua Gateway)            │
               ▼                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      GATEWAY TIER (YARP)                        │
│                                                                 │
│              ┌────────────────────────────┐                     │
│              │     FishShop.Gateway        │                     │
│              │      localhost:5000          │                     │
│              │                             │                     │
│              │ • Reverse Proxy (YARP)      │                     │
│              │ • JWT Validation            │                     │
│              │ • Permission Check          │                     │
│              │ • Rate Limiting             │                     │
│              │ • Set-ApiKey Middleware     │                     │
│              │ • CORS (AllowNKT)           │                     │
│              │ • Response Compression      │                     │
│              └─────────────┬──────────────┘                     │
└────────────────────────────┼────────────────────────────────────┘
                             │ Proxy đến service phù hợp
        ┌────────────────────┼─────────────────────────┐
        ▼                    ▼           ▼              ▼           ▼
┌────────────┐  ┌────────────────┐  ┌────────────┐  ┌──────────┐  ┌───────────┐
│  API.Auth  │  │  API.Products  │  │ API.Orders │  │API.Admin │  │API.Content│
│  port:5001 │  │   port:5002    │  │  port:5003 │  │ port:5004│  │ port:5005 │
│            │  │                │  │            │  │          │  │           │
│ • Đăng nhập│  │ • Sản phẩm    │  │ • Giỏ hàng │  │• Dashboard│ │• Blog     │
│ • Đăng ký  │  │ • Danh mục    │  │ • Đặt hàng │  │• POS Sales│ │• Liên hệ  │
│ • JWT/Token│  │ • Tồn kho     │  │ • Thanh toán│  │• Báo cáo │  │           │
│ • Quyền    │  │               │  │ • Khuyến mãi│  │• Khách hàng│           │
└─────┬──────┘  └───────┬───────┘  └──────┬─────┘  └────┬─────┘  └─────┬─────┘
      │                 │                  │              │               │
      └─────────────────┴──────────────────┴──────────────┴───────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                     SHARED LIBRARIES                            │
│                                                                 │
│  ┌──────────────────────────┐   ┌─────────────────────────────┐│
│  │     netcore.Commons       │   │      netcore.Entities        ││
│  │                          │   │                             ││
│  │ • [ApiKey] Attribute     │   │ • Domain Entities (18 class)││
│  │ • [Audit] Attribute      │   │ • Enums                     ││
│  │ • MessageException       │   │ • AppDbContext (EF+Oracle)  ││
│  │ • KebabCase Transformer  │   │ • GenericRepository         ││
│  │ • ApiPrefixConvention    │   │ • UnitOfWork                ││
│  │ • TrimStrings Filter     │   │ • IEntityTypeConfiguration  ││
│  │ • GlobalExceptionFilter  │   │ • AddEntityServices()       ││
│  │ • ApiResponse<T>         │   │                             ││
│  │ • PagedResult<T>         │   │                             ││
│  │ • Serilog Extensions     │   │                             ││
│  │ • CORS Extensions        │   │                             ││
│  └──────────────────────────┘   └─────────────────────────────┘│
└─────────────────────────────────────────────────────────────────┘
                                    │
                          ┌─────────┴──────────┐
                          │                    │
                          ▼                    ▼
┌────────────────────────────────┐  ┌──────────────────────────────┐
│         DATA TIER              │  │      EXTERNAL SERVICES        │
│                                │  │                              │
│  ┌──────────────────────────┐  │  │  ┌──────────────────────┐   │
│  │     Oracle Database       │  │  │  │     Cloudinary        │   │
│  │         (19c+)            │  │  │  │   (Image Storage)    │   │
│  │  18 bảng, triggers,      │  │  │  │                      │   │
│  │  indexes, sequences      │  │  │  │ • 25GB free storage  │   │
│  └──────────────────────────┘  │  │  │ • CDN tự động        │   │
│                                │  │  │ • Transform URL       │   │
│  ┌──────────────────────────┐  │  │  └──────────────────────┘   │
│  │     RabbitMQ              │  │  │                              │
│  │   (Message Queue)        │  │  │  Dùng cho: upload ảnh SP,   │
│  │  queue: order.created    │  │  │  ảnh danh mục, blog thumb   │
│  │  queue: order.processed  │  │  │                              │
│  └──────────────────────────┘  │  └──────────────────────────────┘
│  Dùng cho: xử lý đơn hàng     │
│  bất đồng bộ, tránh race      │
│  condition tồn kho            │
└────────────────────────────────┘
```

---

## Pattern Kiến Trúc (Theo NKT Style)

### Cấu trúc trong mỗi API Service

```
Request từ Gateway (đã có ApiKey header)
    │
    ▼
[ApiKey] Attribute → Xác thực gateway key
    │
    ▼
[Audit] Attribute → Ghi nhật ký thao tác
    │
    ▼
Controller → Nhận request, gọi Service
    │
    ▼
I{Resource}Service (Interface)
    │
    ▼
{Resource}Service (Implementation)
    │ Gọi trực tiếp DbContext hoặc Repository
    ▼
UnitOfWork / GenericRepository / AppDbContext
    │
    ▼
Oracle Database
```

### Chuỗi DI Registration

```
Program.cs
  ├── AddApplicationServices()       ← DependencyInjection.cs
  │     ├── AddDependencyInjection() ← ServiceRegistrationExtensions.cs
  │     │     └── services.AddScoped<IXxx, Xxx>()
  │     ├── AddSerilogConfiguration()
  │     └── AddCorsConfiguration()
  │
  └── AddEntityServices()            ← EntityServicesExtensions.cs
        ├── services.AddDbContext<AppDbContext>(UseOracle(...))
        └── services.AddScoped<IUnitOfWork, UnitOfWork>()
```

---

## URL Pattern RESTful

```
Gateway URL: http://localhost:5000
    /api/{service}/{resource}[/{id-or-subresource}]

Ví dụ:
    POST   /api/user/auth/login
    GET    /api/product/products
    GET    /api/product/products/slug/{slug}
    POST   /api/order/orders
    GET    /api/order/orders/me
    PATCH  /api/order/orders/{orderCode}/status
    POST   /api/content/contacts
```

---

## Luồng Xác Thực (Auth Flow)

```
1. FE → POST /api/user/auth/login { username, password }
              │
              ▼ Gateway: route không cần JWT (UnauthenticatedRoutes)
              │
              ▼ API.Auth: AuthService.DangNhapAsync()
              │   - Check username/password (BCrypt)
              │   - Generate AccessToken (JWT 15m)
              │   - Generate RefreshToken (JWT 7d)
              │
              ▼ Return { accessToken, refreshToken, expiresIn, user }
              │
2. FE lưu token (memory / localStorage)
              │
3. FE → Request bảo vệ: Authorization: Bearer <accessToken>
    │
    ▼ Gateway kiểm tra JWT (TokenValidator)
    ▼ Gateway gọi /api/user/permissions/check (nếu cần quyền cụ thể)
    ▼ Gateway set header X-User-Id, X-User-Name, X-User-Info
    ▼ Gateway set header X-Api-Key cho service đích
    │
    ▼ Service nhận request: [ApiKey] validate key → [Audit] log → Controller
              │
4. Khi AccessToken hết hạn:
   FE → POST /api/user/auth/refresh-token { refreshToken }
              │
              ▼ API.Auth: tạo token mới
              ▼ Return new { accessToken, refreshToken }
```

---

## Luồng Đặt Hàng (Order Flow)

```
FE-Customer
  │
  ├─ 1. GET /api/product/products (duyệt SP)
  │
  ├─ 2. POST /api/order/cart/items (thêm vào giỏ)
  │
  ├─ 3. POST /api/order/promotions/validate { code, orderAmount }
  │
  ├─ 4. POST /api/order/orders (đặt hàng)
  │         └─ OrderService:
  │             - Tạo ORDER record
  │             - Tạo ORDER_ITEMS records
  │             - Publish OrderCreatedEvent vào RabbitMQ
  │             - Xóa SHOPPING_CART
  │             - API.Product consume event để trừ kho và ghi INVENTORY_TRANSACTIONS
  │
  ├─ 5. POST /api/order/payments { orderId, method }
  │
  └─ 6. GET /api/order/orders/me (theo dõi đơn)
```

---

## Port Map (Development)

| Service | URL Dev | Swagger |
|---------|---------|---------|
| FishShop.Gateway | http://localhost:5000 | — |
| API.Auth | http://localhost:5001 | http://localhost:5001/swagger |
| API.Products | http://localhost:5002 | http://localhost:5002/swagger |
| API.Orders | http://localhost:5003 | http://localhost:5003/swagger |
| API.Admin | http://localhost:5004 | http://localhost:5004/swagger |
| API.Content | http://localhost:5005 | http://localhost:5005/swagger |
| FE-Customer | http://localhost:5173 | — |
| FE-Admin | http://localhost:5174 | — |
| Oracle DB | localhost:1521/XE | — |

---

## Bảo Mật

| Lớp | Cơ chế | Mô tả |
|-----|--------|-------|
| Gateway → Service | `X-Api-Key` header | Service từ chối request không đến từ Gateway |
| Client → Gateway | JWT Bearer | Token validate tại Gateway |
| Gateway | `PermissionValidationMiddleware` | Gọi API.Auth để check quyền |
| Gateway | Rate Limiting | `GatewayRateLimitingMiddleware` |
| Mật khẩu | BCrypt | Hash/verify trong AuthService |
| CORS | Allowlist origin | Chỉ cho phép FE domains |
| Response | Gzip compression | Qua YARP middleware |
| Ảnh | Cloudinary API Key | Key ẩn trong appsettings, không lộ ra client |
| Queue | RabbitMQ credentials | Chỉ backend nội bộ kết nối |

---

## Hạ Tầng Bổ Sung

| Thành phần | Công nghệ | Mục đích | Tài liệu |
|-----------|-----------|----------|----------|
| **Image Storage** | Cloudinary (Free) | Lưu & phân phối ảnh sản phẩm, danh mục, blog | [11_INFRASTRUCTURE.md](./11_INFRASTRUCTURE.md) |
| **Message Queue** | RabbitMQ + MassTransit | Xử lý đơn hàng bất đồng bộ, tránh race condition | [11_INFRASTRUCTURE.md](./11_INFRASTRUCTURE.md) |

> **Cloudinary** cho phép transform ảnh qua URL (resize, crop, format) — không cần xử lý ảnh trên server.
>
> **RabbitMQ** tách biệt bước nhận & xử lý đơn: API trả về ngay trong 100ms, Worker xử lý trong nền.
