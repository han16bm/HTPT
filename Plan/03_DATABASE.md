# 🗄️ Thiết Kế Cơ Sở Dữ Liệu — FISH SHOP

> Schema Oracle 19c+ · File nguồn: `/API/database/oracle_full_schema.sql`

---

## Sơ Đồ Quan Hệ (ERD Tóm Tắt)

```
ROLES ──────────── USERS ─────────────────── CUSTOMER_PROFILES
                     │                               │
                     │                    CUSTOMER_ADDRESSES
                     │
              BLOG_POSTS ◄──── BLOG_CATEGORIES
              (author_id)

CATEGORIES ◄────── PRODUCTS ─────── PRODUCT_IMAGES
(parent_id              │
self-ref)               ├── INVENTORY_TRANSACTIONS
                         │
                         ├── PROMOTION_PRODUCTS ─── PROMOTIONS
                         │
                         └── CART_ITEMS ──── SHOPPING_CARTS ── CUSTOMER_PROFILES
                         │
                         └── ORDER_ITEMS ─── ORDERS ──── CUSTOMER_PROFILES
                                                   │           │
                                                   ├── CUSTOMER_ADDRESSES
                                                   ├── PROMOTIONS
                                                   └── PAYMENTS

CONTACT_MESSAGES (độc lập)
```

---

## Chi Tiết Các Bảng

### 1. ROLES — Vai trò hệ thống
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|---------|
| ID | NUMBER | NOT NULL | PK, Identity |
| CODE | VARCHAR2(30) | NOT NULL | UNIQUE: ADMIN, STAFF, CUSTOMER |
| NAME | VARCHAR2(100) | NOT NULL | Tên hiển thị |
| DESCRIPTION | VARCHAR2(500) | NULL | Mô tả |
| STATUS | NUMBER(1) | NOT NULL | 1=Active, 0=Inactive |
| CREATED_AT | DATE | NOT NULL | Mặc định SYSDATE |
| UPDATED_AT | DATE | NOT NULL | Auto-update trigger |

Seed data:
```sql
('ADMIN', 'Administrator', 'Quản trị hệ thống')
('STAFF',  'Staff',        'Nhân viên bán hàng')
('CUSTOMER','Customer',    'Khách hàng')
```

---

### 2. USERS — Tài khoản hệ thống
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|-------|
| ID | NUMBER | NOT NULL | PK |
| ROLE_ID | NUMBER | NOT NULL | FK → ROLES |
| USERNAME | VARCHAR2(50) | NOT NULL | UNIQUE |
| EMAIL | VARCHAR2(150) | NULL | UNIQUE |
| PASSWORD_HASH | VARCHAR2(255) | NOT NULL | Bcrypt |
| FULL_NAME | VARCHAR2(150) | NULL | |
| PHONE | VARCHAR2(20) | NULL | UNIQUE |
| AVATAR_URL | VARCHAR2(500) | NULL | |
| STATUS | NUMBER(1) | NOT NULL | 1=Active, 0=Locked |
| IS_ADMIN | NUMBER(1) | NOT NULL | 1=Superadmin, bỏ qua RBAC |
| LAST_LOGIN_AT | DATE | NULL | |
| RESET_TOKEN | VARCHAR2(200) | NULL | Token đặt lại mật khẩu |
| RESET_TOKEN_EXP | DATE | NULL | Hết hạn token |
| CREATED_BY | NUMBER | NULL | FK → USERS |
| UPDATED_BY | NUMBER | NULL | FK → USERS |
| UPDATED_AT | DATE | NULL | |

---

### 2b. PERMISSIONS — Danh sách quyền [MỚI]
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|-------|
| ID | NUMBER | NOT NULL | PK, Identity |
| PERM_CODE | VARCHAR2(100) | NOT NULL | UNIQUE: 'USER.VIEW', 'ORDER.PROCESS' |
| PERM_NAME | VARCHAR2(200) | NOT NULL | Tên hiển thị |
| MODULE | VARCHAR2(50) | NOT NULL | USER/PRODUCT/ORDER/INVENTORY/REPORT |
| DESCRIPTION | CLOB | NULL | Mô tả chi tiết |
| CREATED_AT | TIMESTAMP | NOT NULL | DEFAULT SYSTIMESTAMP |

```sql
-- Seed data PERMISSIONS
('USER.VIEW',       'Xem người dùng',     'USER')
('USER.CREATE',     'Thêm người dùng',    'USER')
('USER.EDIT',       'Sửa người dùng',     'USER')
('USER.DELETE',     'Xóa người dùng',     'USER')
('USER.RESET_PWD',  'Reset mật khẩu',     'USER')
('USER.MANAGE_ROLE','Phân quyền',         'USER')
('PRODUCT.VIEW',    'Xem sản phẩm',       'PRODUCT')
('PRODUCT.CREATE',  'Thêm sản phẩm',      'PRODUCT')
('PRODUCT.EDIT',    'Sửa sản phẩm',       'PRODUCT')
('PRODUCT.DELETE',  'Xóa sản phẩm',       'PRODUCT')
('ORDER.VIEW',      'Xem đơn hàng',       'ORDER')
('ORDER.PROCESS',   'Cập nhật trạng thái','ORDER')
('ORDER.CANCEL',    'Hủy đơn hàng',       'ORDER')
('INVENTORY.VIEW',  'Xem tồn kho',        'INVENTORY')
('INVENTORY.IMPORT','ĂNhập hàng',          'INVENTORY')
('REPORT.VIEW',     'Xem báo cáo',        'REPORT')
```

---

### 2c. ROLE_PERMISSIONS — Phân quyền theo Role [MỚI]
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|-------|
| ID | NUMBER | NOT NULL | PK, Identity |
| ROLE_ID | NUMBER | NOT NULL | FK → ROLES |
| PERM_CODE | VARCHAR2(100) | NOT NULL | Permission code |
| CREATED_AT | TIMESTAMP | NOT NULL | DEFAULT SYSTIMESTAMP |
| | UNIQUE(ROLE_ID, PERM_CODE) | | Không trùng |

```sql
-- Seed STAFF permissions (ROLE_ID=2):
INSERT INTO ROLE_PERMISSIONS (ROLE_ID, PERM_CODE) VALUES (2, 'PRODUCT.VIEW');
INSERT INTO ROLE_PERMISSIONS (ROLE_ID, PERM_CODE) VALUES (2, 'PRODUCT.EDIT');
INSERT INTO ROLE_PERMISSIONS (ROLE_ID, PERM_CODE) VALUES (2, 'ORDER.VIEW');
INSERT INTO ROLE_PERMISSIONS (ROLE_ID, PERM_CODE) VALUES (2, 'ORDER.PROCESS');
INSERT INTO ROLE_PERMISSIONS (ROLE_ID, PERM_CODE) VALUES (2, 'INVENTORY.VIEW');
INSERT INTO ROLE_PERMISSIONS (ROLE_ID, PERM_CODE) VALUES (2, 'REPORT.VIEW');
```

---

### 3. CUSTOMER_PROFILES — Hồ sơ khách hàng
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|---------|
| ID | NUMBER | NOT NULL | PK |
| USER_ID | NUMBER | NULL | FK → USERS (tài khoản liên kết) |
| CUSTOMER_CODE | VARCHAR2(30) | NOT NULL | UNIQUE, tự gen |
| FULL_NAME | VARCHAR2(150) | NOT NULL | |
| PHONE | VARCHAR2(20) | NOT NULL | UNIQUE |
| EMAIL | VARCHAR2(150) | NULL | |
| DATE_OF_BIRTH | DATE | NULL | |
| GENDER | VARCHAR2(10) | NULL | MALE/FEMALE/OTHER |
| STATUS | NUMBER(1) | NOT NULL | |

---

### 4. CUSTOMER_ADDRESSES — Địa chỉ giao hàng
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|---------|
| ID | NUMBER | NOT NULL | PK |
| CUSTOMER_ID | NUMBER | NOT NULL | FK → CUSTOMER_PROFILES |
| RECEIVER_NAME | VARCHAR2(150) | NOT NULL | Tên người nhận |
| RECEIVER_PHONE | VARCHAR2(20) | NOT NULL | |
| PROVINCE | VARCHAR2(100) | NOT NULL | Tỉnh/TP |
| DISTRICT | VARCHAR2(100) | NOT NULL | Quận/Huyện |
| WARD | VARCHAR2(100) | NULL | Phường/Xã |
| ADDRESS_LINE | VARCHAR2(255) | NOT NULL | Số nhà, đường |
| IS_DEFAULT | NUMBER(1) | NOT NULL | 1=Địa chỉ mặc định |

---

### 5. CATEGORIES — Danh mục sản phẩm
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|---------|
| ID | NUMBER | NOT NULL | PK |
| PARENT_ID | NUMBER | NULL | FK → CATEGORIES (self-ref, danh mục cha) |
| CATEGORY_CODE | VARCHAR2(30) | NOT NULL | UNIQUE |
| NAME | VARCHAR2(150) | NOT NULL | |
| SLUG | VARCHAR2(200) | NOT NULL | UNIQUE, SEO-friendly |
| DESCRIPTION | VARCHAR2(1000) | NULL | |
| IMAGE_URL | VARCHAR2(500) | NULL | |
| DISPLAY_ORDER | NUMBER | NOT NULL | Thứ tự hiển thị |
| STATUS | NUMBER(1) | NOT NULL | |

Seed data:
```sql
('FISH',      'Cá cảnh', 'ca-canh',   order=1)
('ACCESSORY', 'Phụ kiện','phu-kien',  order=2)
('FOOD',      'Thức ăn', 'thuc-an',   order=3)
```

---

### 6. PRODUCTS — Sản phẩm
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|---------|
| ID | NUMBER | NOT NULL | PK |
| CATEGORY_ID | NUMBER | NOT NULL | FK → CATEGORIES |
| PRODUCT_CODE | VARCHAR2(30) | NOT NULL | UNIQUE |
| SKU | VARCHAR2(50) | NULL | UNIQUE |
| NAME | VARCHAR2(200) | NOT NULL | |
| SLUG | VARCHAR2(250) | NOT NULL | UNIQUE, SEO |
| SHORT_DESCRIPTION | VARCHAR2(500) | NULL | |
| DESCRIPTION | CLOB | NULL | Rich text |
| IMAGE_URL | VARCHAR2(500) | NULL | Ảnh chính (Cloudinary URL) |
| CLOUDINARY_PUBLIC_ID | VARCHAR2(200) | NULL | Để xóa ảnh trên Cloudinary |
| COST_PRICE | NUMBER(14,2) | NOT NULL | Giá nhập |
| SALE_PRICE | NUMBER(14,2) | NOT NULL | Giá bán |
| STOCK_QUANTITY | NUMBER | NOT NULL | Tồn kho |
| SOLD_QUANTITY | NUMBER | NOT NULL | Đã bán |
| WEIGHT_GRAMS | NUMBER(10,2) | NULL | Trọng lượng |
| STATUS | NUMBER(1) | NOT NULL | 1=Đang bán |
| IS_FEATURED | NUMBER(1) | NOT NULL | 1=Nổi bật |
| CREATED_BY | NUMBER | NULL | FK → USERS |
| UPDATED_BY | NUMBER | NULL | FK → USERS |

---

### 7. PRODUCT_IMAGES — Nhiều ảnh cho sản phẩm
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|---------|
| ID | NUMBER | NOT NULL | PK |
| PRODUCT_ID | NUMBER | NOT NULL | FK → PRODUCTS |
| IMAGE_URL | VARCHAR2(500) | NOT NULL | Cloudinary URL |
| CLOUDINARY_PUBLIC_ID | VARCHAR2(200) | NULL | Để xóa trên Cloudinary |
| ALT_TEXT | VARCHAR2(255) | NULL | |
| IS_PRIMARY | NUMBER(1) | NOT NULL | 1=Ảnh chính |
| DISPLAY_ORDER | NUMBER | NOT NULL | |

---

### 8. INVENTORY_TRANSACTIONS — Giao dịch nhập/xuất kho
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|---------|
| ID | NUMBER | NOT NULL | PK |
| PRODUCT_ID | NUMBER | NOT NULL | FK → PRODUCTS |
| TRANSACTION_TYPE | VARCHAR2(20) | NOT NULL | IMPORT/EXPORT/ADJUSTMENT/SALE/RETURN |
| QUANTITY | NUMBER | NOT NULL | Số lượng (dương/âm) |
| UNIT_COST | NUMBER(14,2) | NULL | Giá nhập từng đơn (nếu là IMPORT) |
| REFERENCE_TYPE | VARCHAR2(30) | NULL | ORDER/PURCHASE/MANUAL |
| REFERENCE_ID | NUMBER | NULL | ID của order/purchase |
| NOTE | VARCHAR2(500) | NULL | Ghi chú |
| CREATED_BY | NUMBER | NULL | FK → USERS |

---

### 9. PROMOTIONS — Mã khuyến mãi
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|---------|
| ID | NUMBER | NOT NULL | PK |
| PROMO_CODE | VARCHAR2(30) | NOT NULL | UNIQUE |
| TITLE | VARCHAR2(200) | NOT NULL | |
| DESCRIPTION | VARCHAR2(1000) | NULL | |
| DISCOUNT_TYPE | VARCHAR2(20) | NOT NULL | PERCENT/AMOUNT |
| DISCOUNT_VALUE | NUMBER(14,2) | NOT NULL | Giá trị giảm |
| MAX_DISCOUNT_VALUE | NUMBER(14,2) | NULL | Giảm tối đa (cho PERCENT) |
| MIN_ORDER_VALUE | NUMBER(14,2) | NOT NULL | Giá trị đơn tối thiểu |
| START_AT | DATE | NOT NULL | |
| END_AT | DATE | NOT NULL | |
| USAGE_LIMIT | NUMBER | NULL | NULL = không giới hạn |
| USED_COUNT | NUMBER | NOT NULL | Đã sử dụng |
| STATUS | NUMBER(1) | NOT NULL | |

---

### 10. ORDERS — Đơn hàng
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|---------|
| ID | NUMBER | NOT NULL | PK |
| ORDER_CODE | VARCHAR2(30) | NOT NULL | UNIQUE, tự gen |
| CUSTOMER_ID | NUMBER | NULL | FK → CUSTOMER_PROFILES |
| ADDRESS_ID | NUMBER | NULL | FK → CUSTOMER_ADDRESSES |
| PROMOTION_ID | NUMBER | NULL | FK → PROMOTIONS |
| ORDER_SOURCE | VARCHAR2(20) | NOT NULL | ONLINE/POS |
| ORDER_STATUS | VARCHAR2(20) | NOT NULL | PENDING/CONFIRMED/SHIPPING/COMPLETED/CANCELLED |
| PAYMENT_STATUS | VARCHAR2(20) | NOT NULL | UNPAID/PARTIAL/PAID/REFUNDED |
| PAYMENT_METHOD | VARCHAR2(20) | NOT NULL | COD/BANK_TRANSFER/CASH |
| CUSTOMER_NAME | VARCHAR2(150) | NOT NULL | Snapshot tại thời điểm đặt |
| CUSTOMER_PHONE | VARCHAR2(20) | NOT NULL | Snapshot |
| CUSTOMER_EMAIL | VARCHAR2(150) | NULL | Snapshot |
| CUSTOMER_ADDRESS | VARCHAR2(500) | NOT NULL | Full address snapshot |
| NOTE | VARCHAR2(1000) | NULL | Ghi chú của khách |
| SUBTOTAL_AMOUNT | NUMBER(14,2) | NOT NULL | Tổng trước giảm |
| DISCOUNT_AMOUNT | NUMBER(14,2) | NOT NULL | Số tiền giảm |
| SHIPPING_FEE | NUMBER(14,2) | NOT NULL | Phí ship |
| TOTAL_AMOUNT | NUMBER(14,2) | NOT NULL | Tổng thanh toán |
| CONFIRMED_AT | DATE | NULL | |
| SHIPPED_AT | DATE | NULL | |
| DELIVERED_AT | DATE | NULL | |
| CANCELLED_AT | DATE | NULL | |

---

### 11. PAYMENTS — Giao dịch thanh toán
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|---------|
| ID | NUMBER | NOT NULL | PK |
| ORDER_ID | NUMBER | NOT NULL | FK → ORDERS |
| PAYMENT_CODE | VARCHAR2(30) | NOT NULL | UNIQUE |
| METHOD | VARCHAR2(20) | NOT NULL | COD/BANK_TRANSFER/CASH |
| STATUS | VARCHAR2(20) | NOT NULL | PENDING/SUCCESS/FAILED/REFUNDED |
| AMOUNT | NUMBER(14,2) | NOT NULL | |
| TRANSACTION_REF | VARCHAR2(100) | NULL | Mã GD ngân hàng |
| PAID_AT | DATE | NULL | |
| NOTE | VARCHAR2(500) | NULL | |

---

### 12. BLOG_POSTS — Bài viết blog
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|---------|
| ID | NUMBER | NOT NULL | PK |
| CATEGORY_ID | NUMBER | NULL | FK → BLOG_CATEGORIES |
| AUTHOR_ID | NUMBER | NULL | FK → USERS |
| TITLE | VARCHAR2(250) | NOT NULL | |
| SLUG | VARCHAR2(300) | NOT NULL | UNIQUE |
| SUMMARY | VARCHAR2(1000) | NULL | Tóm tắt |
| CONTENT | CLOB | NOT NULL | Nội dung rich text |
| THUMBNAIL_URL | VARCHAR2(500) | NULL | |
| STATUS | VARCHAR2(20) | NOT NULL | DRAFT/PUBLISHED/HIDDEN |
| PUBLISHED_AT | DATE | NULL | |

---

### 13. CONTACT_MESSAGES — Liên hệ từ khách
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|---------|
| FULL_NAME | VARCHAR2(150) | NOT NULL | |
| EMAIL | VARCHAR2(150) | NULL | |
| PHONE | VARCHAR2(20) | NULL | |
| SUBJECT | VARCHAR2(200) | NULL | |
| MESSAGE | CLOB | NOT NULL | |
| STATUS | VARCHAR2(20) | NOT NULL | NEW/READ/RESOLVED/CLOSED |

---

### 14. SYSTEM_SETTINGS — Cấu hình động [MỚI]
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|-------|
| ID | NUMBER | NOT NULL | PK, Identity |
| SETTING_KEY | VARCHAR2(100) | NOT NULL | UNIQUE: 'SHIPPING_FEE_DEFAULT' |
| SETTING_GROUP | VARCHAR2(50) | NOT NULL | SHIPPING/SHOP_INFO/ALERT/ORDER/CONTENT |
| VALUE | CLOB | NULL | Giá trị (string/number/JSON) |
| DATA_TYPE | VARCHAR2(20) | NOT NULL | STRING/NUMBER/BOOLEAN/JSON |
| DESCRIPTION | VARCHAR2(500) | NULL | |
| IS_ACTIVE | NUMBER(1) | NOT NULL | DEFAULT 1 |
| UPDATED_BY | VARCHAR2(100) | NULL | Username người sửa |
| UPDATED_AT | TIMESTAMP | NOT NULL | DEFAULT SYSTIMESTAMP |

```sql
-- Seed data
INSERT INTO SYSTEM_SETTINGS VALUES (DEFAULT,'SHIPPING_FEE_DEFAULT','SHIPPING','30000','NUMBER','Phí vận chuyển mặc định (VNĐ)',1,'system',SYSTIMESTAMP);
INSERT INTO SYSTEM_SETTINGS VALUES (DEFAULT,'LOW_STOCK_THRESHOLD','ALERT','10','NUMBER','Ngưỡng cảnh báo hàng sắp hết',1,'system',SYSTIMESTAMP);
INSERT INTO SYSTEM_SETTINGS VALUES (DEFAULT,'AUTO_CANCEL_HOURS','ORDER','48','NUMBER','Tự hủy đơn PENDING sau N giờ',1,'system',SYSTIMESTAMP);
INSERT INTO SYSTEM_SETTINGS VALUES (DEFAULT,'SHOP_NAME','SHOP_INFO','Cá Cảnh Shop','STRING','Tên cửa hàng',1,'system',SYSTIMESTAMP);
INSERT INTO SYSTEM_SETTINGS VALUES (DEFAULT,'SHOP_PHONE','SHOP_INFO','0901234567','STRING','SĐT cửa hàng',1,'system',SYSTIMESTAMP);
```

---

### 15. WHITELIST_ENTRIES — Danh sách trắng [MỚI]
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|-------|
| ID | NUMBER | NOT NULL | PK, Identity |
| RECORD_TYPE | VARCHAR2(20) | NOT NULL | IP/API_KEY/DOMAIN |
| VALUE | VARCHAR2(500) | NOT NULL | IP, key, domain |
| DESCRIPTION | VARCHAR2(500) | NULL | |
| IS_ENABLED | NUMBER(1) | NOT NULL | DEFAULT 1 |
| CREATED_BY | VARCHAR2(100) | NULL | |
| CREATED_AT | TIMESTAMP | NOT NULL | DEFAULT SYSTIMESTAMP |
| UPDATED_AT | TIMESTAMP | NULL | |

```sql
INSERT INTO WHITELIST_ENTRIES VALUES (DEFAULT,'API_KEY','fish-gateway-key-2026','Key của Gateway',1,'system',SYSTIMESTAMP,NULL);
```

---

### 16. AUDIT_LOGS — Nhật ký thao tác [MỚI]
| Cột | Kiểu | Null | Ghi chú |
|-----|------|------|-------|
| ID | NUMBER | NOT NULL | PK, Identity |
| ACTION | VARCHAR2(100) | NOT NULL | 'USER.CREATE', 'CONFIG.UPDATE' |
| TARGET_TYPE | VARCHAR2(50) | NOT NULL | 'User', 'SystemConfig', 'Order' |
| TARGET_ID | VARCHAR2(100) | NULL | ID của đối tượng |
| OLD_VALUE | CLOB | NULL | JSON snapshot trước |
| NEW_VALUE | CLOB | NULL | JSON snapshot sau |
| PERFORMED_BY | VARCHAR2(100) | NOT NULL | Username |
| PERFORMED_AT | TIMESTAMP | NOT NULL | DEFAULT SYSTIMESTAMP |
| IP_ADDRESS | VARCHAR2(50) | NULL | |
| USER_AGENT | VARCHAR2(500) | NULL | |

---

## EF Core — Ánh Xạ Entity

### AppDbContext.cs (tóm tắt)
```csharp
public class AppDbContext : DbContext
{
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<CustomerProfile> CustomerProfiles { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<PromotionProduct> PromotionProducts { get; set; }
    public DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<BlogCategory> BlogCategories { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }

    // Bổ sung cho Admin Modules:
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<WhitelistEntry> WhitelistEntries { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        // Oracle cần upper-case table names:
        // modelBuilder.HasDefaultSchema("FISHSCHEMA");
    }
}
```

### Connection String (Oracle)
```json
// appsettings.json
{
  "ConnectionStrings": {
    "Oracle": "User Id=fishuser;Password=fish123;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=FISHDB)));"
  }
}
```

```csharp
// ServiceRegistrationExtensions.cs
services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(configuration.GetConnectionString("Oracle")));
```

---

## Indexes hiện có
```sql
-- Tất cả FK đều có index
IDX_USERS_ROLE_ID
IDX_CUSTOMER_USER_ID
IDX_CUSTOMER_ADDRESSES_CUSTOMER
IDX_PRODUCTS_CATEGORY_ID
IDX_PRODUCT_IMAGES_PRODUCT_ID
IDX_INV_TX_PRODUCT_ID
IDX_CART_ITEMS_CART_ID
IDX_ORDERS_CUSTOMER_ID
IDX_ORDERS_STATUS          -- Quan trọng cho filter đơn hàng
IDX_ORDER_ITEMS_ORDER_ID
IDX_PAYMENTS_ORDER_ID
IDX_BLOG_POSTS_CATEGORY_ID
IDX_BLOG_POSTS_AUTHOR_ID
```

## Bảng Cần Bổ Sung (Tương Lai)

| Bảng | Mục đích |
|------|----------|
| PRODUCT_REVIEWS | Đánh giá sao + nhận xét của khách |
| WISHLISTS | Danh sách yêu thích |
| NOTIFICATION_LOGS | Lịch sử email/SMS gửi |
| AUDIT_LOGS | Lịch sử thao tác của admin |
| REFRESH_TOKENS | Lưu refresh token (thay localStorage) |
