# 🔐 Module Quản Trị Hệ Thống — FISH SHOP

> Đơn giản hóa từ tài liệu tham khảo cho phù hợp stack: .NET 8 + Oracle + React
> 3 module: **User Management · System Config · Whitelist**

---

## Tổng Quan

| Module | Service | Độ ưu tiên | Ghi chú |
|--------|---------|-----------|---------|
| **User Management** | `API.Auth` (mở rộng) | ⭐⭐⭐ Cao | RBAC đơn giản: ADMIN / STAFF / CUSTOMER |
| **System Config** | `API.Admin` (bổ sung) | ⭐⭐ Trung bình | DB-driven config, memory cache |
| **Whitelist** | `FishShop.Gateway` (middleware) | ⭐ Thấp | IP + API Key whitelist |

---

## I. Quản Lý Người Dùng Internal (User Management)

### Scope cho FISH_SHOP

```
Đơn giản hóa: Không cần cây quyền phức tạp
→ Chỉ cần 3 role: ADMIN / STAFF / CUSTOMER
→ ADMIN: toàn quyền
→ STAFF: xem + sửa đơn hàng + bán POS (không xóa, không phân quyền)
→ CUSTOMER: tự đăng ký, tự quản lý profile

Quản lý internal = quản lý tài khoản ADMIN + STAFF
(Tài khoản CUSTOMER do khách tự đăng ký)
```

---

### Database Schema (Bổ Sung/Điều Chỉnh)

```sql
-- Đã có trong oracle_full_schema.sql:
-- ROLES (ID, ROLE_CODE, ROLE_NAME, DESCRIPTION)
-- USERS (ID, USERNAME, PASSWORD_HASH, FULL_NAME, EMAIL, PHONE, STATUS, ROLE_ID, ...)

-- Cần thêm:
CREATE TABLE PERMISSIONS (
    ID              NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    PERM_CODE       VARCHAR2(100)    NOT NULL UNIQUE,  -- 'USER.VIEW', 'USER.CREATE', ...
    PERM_NAME       VARCHAR2(200)    NOT NULL,
    MODULE          VARCHAR2(50)     NOT NULL,          -- 'USER', 'PRODUCT', 'ORDER'...
    DESCRIPTION     CLOB,
    CREATED_AT      TIMESTAMP        DEFAULT SYSTIMESTAMP
);

CREATE TABLE ROLE_PERMISSIONS (
    ID              NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ROLE_ID         NUMBER           NOT NULL REFERENCES ROLES(ID),
    PERM_CODE       VARCHAR2(100)    NOT NULL,
    CREATED_AT      TIMESTAMP        DEFAULT SYSTIMESTAMP,
    UNIQUE (ROLE_ID, PERM_CODE)
);

-- Bổ sung cột vào USERS (nếu chưa có):
ALTER TABLE USERS ADD (
    IS_ADMIN        NUMBER(1)        DEFAULT 0,    -- 1 = superadmin, bỏ qua mọi quyền
    LAST_LOGIN_AT   TIMESTAMP,
    RESET_TOKEN     VARCHAR2(200),
    RESET_TOKEN_EXP TIMESTAMP,
    CREATED_BY      NUMBER,
    UPDATED_BY      NUMBER,
    UPDATED_AT      TIMESTAMP
);
```

---

### Permission Codes Cho FISH_SHOP

```
Module USER:
  USER.VIEW          → Xem danh sách người dùng
  USER.CREATE        → Thêm người dùng mới
  USER.EDIT          → Sửa thông tin
  USER.DELETE        → Xóa người dùng
  USER.RESET_PWD     → Reset mật khẩu
  USER.MANAGE_ROLE   → Phân quyền

Module PRODUCT:
  PRODUCT.VIEW       → Xem sản phẩm
  PRODUCT.CREATE     → Thêm sản phẩm
  PRODUCT.EDIT       → Sửa sản phẩm
  PRODUCT.DELETE     → Xóa sản phẩm

Module ORDER:
  ORDER.VIEW         → Xem đơn hàng
  ORDER.PROCESS      → Cập nhật trạng thái đơn
  ORDER.CANCEL       → Hủy đơn hàng
  ORDER.EXPORT       → Xuất Excel

Module INVENTORY:
  INVENTORY.VIEW     → Xem tồn kho
  INVENTORY.IMPORT   → Nhập hàng

Module REPORT:
  REPORT.VIEW        → Xem báo cáo

-- Seed data mặc định:
ADMIN role → ALL permissions
STAFF role → *.VIEW + ORDER.PROCESS + INVENTORY.VIEW + PRODUCT.EDIT
```

---

### API Endpoints (Mở Rộng API.Auth)

Thêm vào `API.Auth/Controllers/UsersController.cs`:

```
Controller: UsersController → /api/auth/users/...

GET  tim-kiem                    → Danh sách users (filter: keyword, role, status)
GET  chi-tiet                    → Chi tiết user (by id)
POST them-moi-cap-nhat           → Thêm/sửa user (upsert)
POST xoa                         → Xóa user
POST khoa-mo-khoa                → Khóa/mở khóa (toggle status)
POST reset-mat-khau              → Reset mật khẩu (generate random + hash)
GET  lay-quyen-nguoi-dung        → Lấy danh sách quyền của user
POST cap-nhat-quyen              → Gán nhóm quyền (roleId) cho user

Controller: RolesController → /api/auth/roles/...

GET  tim-kiem                    → Danh sách roles
GET  chi-tiet                    → Chi tiết role + permissions
POST them-moi-cap-nhat           → Thêm/sửa role
POST xoa                         → Xóa role
POST cap-nhat-quyen-role         → Cập nhật permission list cho role
```

---

### DTOs (API.Auth/Models)

```csharp
// Models/DTOs/NguoiDungListDto.cs
public record NguoiDungListDto
{
    public long Id { get; init; }
    public string Username { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string RoleCode { get; init; } = default!;
    public string RoleName { get; init; } = default!;
    public int Status { get; init; }          // 1=Active, 0=Locked
    public bool IsAdmin { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

// Models/DTOs/NguoiDungChiTietDto.cs — đầy đủ hơn cho màn sửa
public record NguoiDungChiTietDto : NguoiDungListDto
{
    public long RoleId { get; init; }
    public List<string> PermCodes { get; init; } = [];   // Quyền của role
}

// Models/Commands/ThemMoiCapNhatNguoiDungRequest.cs
public record ThemMoiCapNhatNguoiDungRequest
{
    public long? Id { get; init; }                // null = thêm mới
    public string Username { get; init; } = default!;
    public string? Password { get; init; }        // null khi sửa (giữ nguyên)
    public string FullName { get; init; } = default!;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public long RoleId { get; init; }
    public int Status { get; init; } = 1;
    public bool IsAdmin { get; init; } = false;
}

// Models/Commands/ResetMatKhauRequest.cs
public record ResetMatKhauRequest
{
    public long UserId { get; init; }
    public string? NewPassword { get; init; }     // null = tự generate
}

// Models/Queries/NguoiDungQuery.cs
public record NguoiDungQuery
{
    public string? Keyword { get; init; }         // Tìm username, fullname, email
    public long? RoleId { get; init; }
    public int? Status { get; init; }             // 1=Active, 0=Locked
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
```

---

### Service Implementation (API.Auth/Services/UserManagementService.cs)

```csharp
public class UserManagementService : IUserManagementService
{
    private readonly AppDbContext _db;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<UserManagementService> _logger;

    // GET — Danh sách người dùng
    public async Task<ApiResponse<PagedResult<NguoiDungListDto>>> GetAllAsync(
        NguoiDungQuery query, CancellationToken ct)
    {
        var q = _db.Users
            .Include(u => u.Role)
            .Where(u => u.Role!.RoleCode != "CUSTOMER"); // Chỉ admin + staff

        if (!string.IsNullOrEmpty(query.Keyword))
        {
            var kw = query.Keyword.ToLower();
            q = q.Where(u => u.Username.ToLower().Contains(kw)
                          || u.FullName.ToLower().Contains(kw)
                          || (u.Email != null && u.Email.ToLower().Contains(kw)));
        }
        if (query.RoleId.HasValue) q = q.Where(u => u.RoleId == query.RoleId);
        if (query.Status.HasValue)  q = q.Where(u => u.Status == query.Status);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new NguoiDungListDto { /* map */ })
            .ToListAsync(ct);

        return ApiResponse<PagedResult<NguoiDungListDto>>.Ok(
            new PagedResult<NguoiDungListDto>(items, total, query.Page, query.PageSize));
    }

    // POST — Thêm mới hoặc cập nhật
    public async Task<ApiResponse<object>> UpsertAsync(
        ThemMoiCapNhatNguoiDungRequest request, CancellationToken ct)
    {
        // Validate username unique
        var exists = await _db.Users
            .AnyAsync(u => u.Username == request.Username && u.Id != request.Id, ct);
        if (exists)
            return ApiResponse<object>.Fail("Username đã tồn tại");

        if (request.Id.HasValue)
        {
            // UPDATE
            var user = await _db.Users.FindAsync(request.Id.Value)
                ?? throw new NotFoundException("Người dùng không tồn tại");
            user.FullName = request.FullName;
            user.Email = request.Email;
            user.Phone = request.Phone;
            user.RoleId = request.RoleId;
            user.Status = request.Status;
            user.IsAdmin = request.IsAdmin;
            if (!string.IsNullOrEmpty(request.Password))
                user.PasswordHash = _passwordService.Hash(request.Password);
        }
        else
        {
            // INSERT
            if (string.IsNullOrEmpty(request.Password))
                return ApiResponse<object>.Fail("Mật khẩu không được để trống khi tạo mới");

            _db.Users.Add(new User
            {
                Username     = request.Username,
                PasswordHash = _passwordService.Hash(request.Password),
                FullName     = request.FullName,
                Email        = request.Email,
                Phone        = request.Phone,
                RoleId       = request.RoleId,
                Status       = 1,
                IsAdmin      = request.IsAdmin
            });
        }

        await _db.SaveChangesAsync(ct);
        return ApiResponse<object>.Ok(null, "Lưu thành công");
    }

    // POST — Reset mật khẩu
    public async Task<ApiResponse<object>> ResetPasswordAsync(
        ResetMatKhauRequest request, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(request.UserId)
            ?? throw new NotFoundException("Người dùng không tồn tại");

        var newPwd = string.IsNullOrEmpty(request.NewPassword)
            ? GenerateRandomPassword()  // Tự gen nếu không truyền
            : request.NewPassword;

        user.PasswordHash = _passwordService.Hash(newPwd);
        await _db.SaveChangesAsync(ct);

        // TODO: Gửi email thông báo (Phase 6)
        _logger.LogInformation("Reset mật khẩu user {Username}", user.Username);

        return ApiResponse<object>.Ok(new { temporaryPassword = newPwd },
            "Reset mật khẩu thành công");
    }

    private static string GenerateRandomPassword()
        => $"Fish@{Random.Shared.Next(100000, 999999)}";
}
```

---

### FE Admin — Màn Hình Users (3 lớp)

```
Pages/Users/
├── UserList.tsx              ← Màn danh sách chính
│   ├── Thanh tìm kiếm (keyword, role filter, status filter)
│   ├── Bảng dữ liệu (Ant Design Table)
│   │   Columns: Avatar, Username, Họ tên, Role, Trạng thái, Đăng nhập cuối, Actions
│   ├── Phân trang
│   └── [+ Thêm mới] Button
│
├── UserFormModal.tsx         ← Modal thêm/sửa
│   Fields:
│   ├── Username (disabled khi sửa)
│   ├── Mật khẩu (optional khi sửa)
│   ├── Họ tên
│   ├── Email
│   ├── Số điện thoại
│   ├── Role (Select: Admin/Staff)
│   ├── Trạng thái (Active/Locked)
│   └── Is Admin (Switch)
│
├── ResetPasswordModal.tsx    ← Modal reset mật khẩu
│   ├── Input mật khẩu mới (optional — để trống = auto-generate)
│   └── Confirm reset
│
└── UserPermissionModal.tsx   ← Modal xem quyền của user (read-only)
    └── Danh sách quyền theo module
```

---

## II. Cấu Hình Hệ Thống (System Config)

### Scope cho FISH_SHOP

```
Đơn giản hóa: Không cần config per-environment phức tạp
→ Lưu các config động (thay đổi không cần redeploy):
   • Phí vận chuyển mặc định
   • Số sản phẩm low-stock cảnh báo
   • Thời gian tự hủy đơn PENDING (giờ)
   • Banner/thông báo trang chủ
   • Thông tin shop (tên, địa chỉ, SĐT, email)
   • Cấu hình Cloudinary (cloud-name, upload-preset)
   
→ Không dùng cho: JWT secret, connection string (giữ trong appsettings.json)
```

---

### Database

```sql
CREATE TABLE SYSTEM_SETTINGS (
    ID              NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    SETTING_KEY     VARCHAR2(100)    NOT NULL UNIQUE,  -- 'SHIPPING_FEE_DEFAULT'
    SETTING_GROUP   VARCHAR2(50)     NOT NULL,          -- 'SHIPPING', 'SHOP_INFO', 'ALERT'
    VALUE           CLOB,                               -- Giá trị (JSON hoặc string)
    DATA_TYPE       VARCHAR2(20)     DEFAULT 'STRING',  -- STRING/NUMBER/BOOLEAN/JSON
    DESCRIPTION     VARCHAR2(500),
    IS_ACTIVE       NUMBER(1)        DEFAULT 1,
    UPDATED_BY      VARCHAR2(100),
    UPDATED_AT      TIMESTAMP        DEFAULT SYSTIMESTAMP
);

-- Seed data mẫu
INSERT INTO SYSTEM_SETTINGS VALUES (DEFAULT, 'SHIPPING_FEE_DEFAULT', 'SHIPPING', '30000', 'NUMBER', 'Phí vận chuyển mặc định (VNĐ)', 1, 'system', SYSTIMESTAMP);
INSERT INTO SYSTEM_SETTINGS VALUES (DEFAULT, 'LOW_STOCK_THRESHOLD',  'ALERT',    '10',    'NUMBER', 'Ngưỡng cảnh báo hàng sắp hết', 1, 'system', SYSTIMESTAMP);
INSERT INTO SYSTEM_SETTINGS VALUES (DEFAULT, 'AUTO_CANCEL_HOURS',    'ORDER',    '48',    'NUMBER', 'Tự hủy đơn PENDING sau N giờ', 1, 'system', SYSTIMESTAMP);
INSERT INTO SYSTEM_SETTINGS VALUES (DEFAULT, 'SHOP_NAME',            'SHOP_INFO','Cá Cảnh Shop', 'STRING', 'Tên cửa hàng', 1, 'system', SYSTIMESTAMP);
INSERT INTO SYSTEM_SETTINGS VALUES (DEFAULT, 'SHOP_PHONE',           'SHOP_INFO','0901234567',   'STRING', 'SĐT cửa hàng', 1, 'system', SYSTIMESTAMP);
INSERT INTO SYSTEM_SETTINGS VALUES (DEFAULT, 'HOME_BANNER_TEXT',     'CONTENT',  'Chào mừng đến Cá Cảnh Shop!', 'STRING', 'Text banner trang chủ', 1, 'system', SYSTIMESTAMP);
```

---

### Service với Memory Cache (API.Admin)

```csharp
// API.Admin/Services/SystemConfigService.cs
public class SystemConfigService : ISystemConfigService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private const string CACHE_KEY = "system_settings";
    private readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(30);

    // Lấy tất cả config (có cache)
    public async Task<Dictionary<string, string?>> GetAllAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(CACHE_KEY, out Dictionary<string, string?> cached))
            return cached!;

        var settings = await _db.SystemSettings
            .Where(s => s.IsActive == 1)
            .ToDictionaryAsync(s => s.SettingKey, s => s.Value, ct);

        _cache.Set(CACHE_KEY, settings, CACHE_DURATION);
        return settings;
    }

    // Lấy 1 giá trị theo key (tiện lợi)
    public async Task<T?> GetAsync<T>(string key, T? defaultValue = default, CancellationToken ct = default)
    {
        var all = await GetAllAsync(ct);
        if (!all.TryGetValue(key, out var raw) || raw is null)
            return defaultValue;

        if (typeof(T) == typeof(string)) return (T)(object)raw;
        if (typeof(T) == typeof(int))    return (T)(object)int.Parse(raw);
        if (typeof(T) == typeof(bool))   return (T)(object)bool.Parse(raw);
        if (typeof(T) == typeof(decimal))return (T)(object)decimal.Parse(raw);

        return defaultValue;
    }

    // Cập nhật config → invalidate cache
    public async Task<ApiResponse<object>> UpdateAsync(
        CapNhatCauHinhRequest request, string updatedBy, CancellationToken ct)
    {
        var setting = await _db.SystemSettings
            .FirstOrDefaultAsync(s => s.SettingKey == request.SettingKey, ct)
            ?? throw new NotFoundException($"Key '{request.SettingKey}' không tồn tại");

        // Validate data type
        ValidateDataType(request.Value, setting.DataType);

        setting.Value     = request.Value;
        setting.UpdatedBy = updatedBy;
        setting.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // Invalidate cache → lần đọc tiếp sẽ tải lại từ DB
        _cache.Remove(CACHE_KEY);

        return ApiResponse<object>.Ok(null, "Cập nhật cấu hình thành công");
    }

    private static void ValidateDataType(string? value, string dataType)
    {
        if (value is null) return;
        var valid = dataType switch
        {
            "NUMBER"  => decimal.TryParse(value, out _),
            "BOOLEAN" => bool.TryParse(value, out _),
            "JSON"    => IsValidJson(value),
            _         => true // STRING → luôn hợp lệ
        };
        if (!valid) throw new MessageException($"Giá trị không hợp lệ với kiểu {dataType}");
    }

    private static bool IsValidJson(string str)
    {
        try { System.Text.Json.JsonDocument.Parse(str); return true; }
        catch { return false; }
    }
}
```

---

### API Endpoints (trong API.Admin)

```
Controller: SystemConfigController → /api/admin/system-config/...

GET  tim-kiem           → Danh sách config (filter: group, keyword)
GET  chi-tiet           → Chi tiết 1 config
POST cap-nhat           → Cập nhật giá trị config
POST bat-tat            → Toggle IsActive
POST them-moi           → Thêm config mới (Admin only)
POST xoa                → Xóa config
GET  refresh-cache      → Xóa cache thủ công
```

---

### Sử Dụng Config Trong Services Khác

```csharp
// Inject ISystemConfigService ở bất kỳ service nào
// Ví dụ: OrderService tính phí ship

public class OrderService : IOrderService
{
    private readonly ISystemConfigService _config;

    public async Task ProcessOrderAsync(OrderCreatedMessage msg, CancellationToken ct)
    {
        // Đọc config động từ DB (có cache)
        var shippingFee = await _config.GetAsync<decimal>("SHIPPING_FEE_DEFAULT", 30000, ct);
        var autoCancelHours = await _config.GetAsync<int>("AUTO_CANCEL_HOURS", 48, ct);

        var order = new Order { ShippingFee = shippingFee, /* ... */ };
        // ...
    }
}
```

---

### FE Admin — Màn Cấu Hình

```
Pages/SystemConfig/
├── ConfigList.tsx              ← Danh sách config theo Group
│   ├── Tabs theo group (SHIPPING / SHOP_INFO / ALERT / ORDER / CONTENT)
│   ├── Mỗi tab: bảng config key-value
│   └── Inline edit hoặc Modal edit
│
└── ConfigEditModal.tsx         ← Modal sửa 1 config
    ├── Key (readonly)
    ├── Mô tả (readonly)
    ├── Data Type (readonly)
    ├── Giá trị (input phù hợp với DataType)
    │   STRING  → Input
    │   NUMBER  → InputNumber
    │   BOOLEAN → Switch
    │   JSON    → Textarea + Validate JSON
    └── [Lưu]
```

---

## III. Quản Lý Whitelist

### Scope cho FISH_SHOP

```
Đơn giản hóa:
→ Whitelist IP: chặn truy cập API Admin từ IP lạ (optional)
→ Whitelist API Key: danh sách API keys hợp lệ cho service-to-service
→ Không cần CIDR phức tạp trong MVP
→ Cache trong memory (không cần Redis cho MVP)
```

---

### Database

```sql
CREATE TABLE WHITELIST_ENTRIES (
    ID              NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    RECORD_TYPE     VARCHAR2(20)     NOT NULL,  -- 'IP', 'API_KEY', 'DOMAIN'
    VALUE           VARCHAR2(500)    NOT NULL,  -- IP address, key, domain
    DESCRIPTION     VARCHAR2(500),
    IS_ENABLED      NUMBER(1)        DEFAULT 1,
    CREATED_BY      VARCHAR2(100),
    CREATED_AT      TIMESTAMP        DEFAULT SYSTIMESTAMP,
    UPDATED_AT      TIMESTAMP
);

-- API Keys cho service-to-service (thay thế config cứng)
INSERT INTO WHITELIST_ENTRIES VALUES (DEFAULT, 'API_KEY', 'fish-gateway-key-prod', 'Key của Gateway', 1, 'system', SYSTIMESTAMP, NULL);

-- IP Admin (nếu muốn restrict)
-- INSERT INTO WHITELIST_ENTRIES VALUES (DEFAULT, 'IP', '192.168.1.0', 'Mạng nội bộ', 1, 'system', SYSTIMESTAMP, NULL);
```

---

### Whitelist Service (trong `netcore.Commons`)

```csharp
// netcore.Commons/Services/WhitelistService.cs
public class WhitelistService : IWhitelistService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private const string CACHE_KEY = "whitelist_entries";
    private readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(10);

    public async Task<bool> IsApiKeyAllowedAsync(string apiKey, CancellationToken ct = default)
    {
        var entries = await GetCachedAsync(ct);
        return entries.Any(e => e.RecordType == "API_KEY"
                             && e.Value == apiKey
                             && e.IsEnabled);
    }

    public async Task<bool> IsIpAllowedAsync(string ipAddress, CancellationToken ct = default)
    {
        var entries = await GetCachedAsync(ct);
        var ipEntries = entries.Where(e => e.RecordType == "IP" && e.IsEnabled).ToList();

        // Nếu không có rule IP nào → cho tất cả qua
        if (!ipEntries.Any()) return true;

        return ipEntries.Any(e => e.Value == ipAddress);
    }

    public void InvalidateCache() => _cache.Remove(CACHE_KEY);

    private async Task<List<WhitelistEntry>> GetCachedAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(CACHE_KEY, out List<WhitelistEntry> cached))
            return cached!;

        var entries = await _db.WhitelistEntries.ToListAsync(ct);
        _cache.Set(CACHE_KEY, entries, CACHE_DURATION);
        return entries;
    }
}
```

---

### Tích Hợp Vào Gateway (SetApiKeyMiddleware)

```csharp
// FishShop.Gateway/Middleware/SetApiKeyMiddleware.cs
// (Upgrade từ config cứng → lấy từ DB qua WhitelistService)

public class SetApiKeyMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Lấy api-key tương ứng với cluster đích
        var cluster = context.GetRouteValue("ClusterId")?.ToString();
        var apiKey = cluster switch
        {
            "auth-cluster"     => _config["ApiKeys:Auth"],
            "products-cluster" => _config["ApiKeys:Products"],
            "orders-cluster"   => _config["ApiKeys:Orders"],
            "admin-cluster"    => _config["ApiKeys:Admin"],
            "content-cluster"  => _config["ApiKeys:Content"],
            _                  => null
        };

        if (!string.IsNullOrEmpty(apiKey))
            context.Request.Headers["X-Api-Key"] = apiKey;

        await next(context);
    }
}
```

---

### API Endpoints (trong API.Admin)

```
Controller: WhitelistController → /api/admin/whitelist/...

GET  tim-kiem           → Danh sách (filter: type, keyword, isEnabled)
GET  chi-tiet           → Chi tiết 1 entry
POST them-moi-cap-nhat  → Thêm/sửa entry
POST xoa                → Xóa entry
POST bat-tat            → Toggle IsEnabled (không xóa hẳn)
POST refresh-cache      → Invalidate cache whitelist
```

---

### FE Admin — Màn Whitelist

```
Pages/Whitelist/
├── WhitelistList.tsx           ← Danh sách với filter
│   ├── Filter: Record Type (IP/API_KEY/DOMAIN), keyword, trạng thái
│   ├── Bảng: Type, Giá trị, Mô tả, Trạng thái, Actions
│   └── [+ Thêm mới]
│
└── WhitelistFormModal.tsx      ← Modal thêm/sửa
    ├── Record Type (Select: IP/API_KEY/DOMAIN)
    ├── Giá trị (Input)
    ├── Mô tả
    └── Is Enabled (Switch)
```

---

## IV. Tích Hợp Vào API.Auth & API.Admin

### Bổ Sung Vào `API.Auth`

```
API.Auth/Controllers/
├── (đã có) AuthController.cs
├── (đã có) PermissionsController.cs
├── [MỚI]   UsersController.cs           ← User Management
└── [MỚI]   RolesController.cs           ← Role Management

API.Auth/Services/
├── (đã có) AuthService.cs, JwtService.cs, PasswordService.cs
├── [MỚI]   UserManagementService.cs
└── [MỚI]   RoleService.cs

API.Auth/Interfaces/
├── [MỚI]   IUserManagementService.cs
└── [MỚI]   IRoleService.cs
```

### Bổ Sung Vào `API.Admin`

```
API.Admin/Controllers/
├── (đã có) DashboardController.cs, SalesController.cs, ...
├── [MỚI]   SystemConfigController.cs
└── [MỚI]   WhitelistController.cs

API.Admin/Services/
├── [MỚI]   SystemConfigService.cs
└── [MỚI]   WhitelistService.cs

API.Admin/Interfaces/
├── [MỚI]   ISystemConfigService.cs
└── [MỚI]   IWhitelistService.cs
```

---

## V. Audit Log (Dùng Chung)

Ghi nhận mọi thao tác quản trị — lưu vào bảng hoặc file Serilog:

```sql
-- Dùng Serilog ghi ra file (đơn giản nhất, không cần bảng riêng)
-- Hoặc tạo bảng:
CREATE TABLE AUDIT_LOGS (
    ID              NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ACTION          VARCHAR2(100),   -- 'USER.CREATE', 'USER.RESET_PWD', 'CONFIG.UPDATE'
    TARGET_TYPE     VARCHAR2(50),    -- 'User', 'SystemConfig', 'Whitelist'
    TARGET_ID       VARCHAR2(100),
    OLD_VALUE       CLOB,            -- JSON snapshot trước khi thay đổi
    NEW_VALUE       CLOB,            -- JSON snapshot sau khi thay đổi
    PERFORMED_BY    VARCHAR2(100),   -- Username
    PERFORMED_AT    TIMESTAMP        DEFAULT SYSTIMESTAMP,
    IP_ADDRESS      VARCHAR2(50),
    USER_AGENT      VARCHAR2(500)
);
```

```csharp
// [Audit] attribute trong NKT tự log → tận dụng luôn
// Hoặc thêm IAuditLogService.cs vào netcore.Commons:

public interface IAuditLogService
{
    Task LogAsync(string action, string targetType, string targetId,
                  object? oldValue = null, object? newValue = null,
                  CancellationToken ct = default);
}
```

---

## VI. Cập Nhật Sidebar FE Admin

```typescript
// Bổ sung vào menuItems (FE/src/layout/MasterLayout.tsx)
{
  key: 'system',
  icon: <SettingOutlined />,
  label: 'Quản trị hệ thống',
  roles: ['ADMIN'],            // Chỉ ADMIN thấy menu này
  children: [
    { key: '/system/users',    label: 'Quản lý người dùng' },
    { key: '/system/config',   label: 'Cấu hình hệ thống' },
    { key: '/system/whitelist',label: 'Whitelist' },
  ]
}
```

---

## VII. Thứ Tự Implement

```
Phase 1 — User Management (ưu tiên cao, cần sớm)
  [ ] Thêm bảng PERMISSIONS + ROLE_PERMISSIONS vào Oracle
  [ ] Seed data: permission codes + role-permission mapping
  [ ] Bổ sung cột USERS (IS_ADMIN, LAST_LOGIN_AT, ...)
  [ ] UserManagementService (getAll, upsert, delete, toggle, resetPwd)
  [ ] UsersController + RolesController trong API.Auth
  [ ] FE: UserList + UserFormModal + ResetPasswordModal

Phase 2 — System Config (sau khi có đủ tính năng MVP)
  [ ] Tạo bảng SYSTEM_SETTINGS + seed data
  [ ] SystemConfigService với IMemoryCache
  [ ] SystemConfigController trong API.Admin
  [ ] Inject ISystemConfigService vào OrderService (tính phí ship động)
  [ ] FE: ConfigList theo Group tabs

Phase 3 — Whitelist (khi cần bảo mật cao hơn)
  [ ] Tạo bảng WHITELIST_ENTRIES
  [ ] WhitelistService với cache
  [ ] WhitelistController trong API.Admin
  [ ] Tích hợp vào [ApiKey] attribute hoặc Gateway middleware
  [ ] FE: WhitelistList + WhitelistFormModal
```
