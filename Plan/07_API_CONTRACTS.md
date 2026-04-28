# 📋 Hợp Đồng API — FISH SHOP

> Base URL (qua Gateway): `http://localhost:8080/api`
> URL Pattern: `/api/{service}/{controller}/{action}` (KebabCase theo NKT)
> Format response chuẩn: `{ success, data, message, errors }`

---

## URL Pattern (Theo NKT Style)

| Service | Prefix | Ví dụ |
|---------|--------|-------|
| API.Auth | `/api/auth` | `/api/auth/auth/dang-nhap` |
| API.Products | `/api/products` | `/api/products/products/tim-kiem` |
| API.Orders | `/api/orders` | `/api/orders/orders/dat-hang` |
| API.Admin | `/api/admin` | `/api/admin/dashboard/thong-ke-tong-quat` |
| API.Content | `/api/content` | `/api/content/blog/tim-kiem` |

---

## Response Format Chuẩn

```json
// Thành công — single object
{
  "success": true,
  "data": { ... },
  "message": "Thao tác thành công"
}

// Thành công — paged list
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 100,
    "page": 1,
    "pageSize": 10,
    "totalPages": 10
  }
}

// Thất bại
{
  "success": false,
  "data": null,
  "message": "Không tìm thấy sản phẩm",
  "errors": ["Product with id 999 not found"]
}
```

---

## 🔐 Authentication

### POST `/api/auth/auth/dang-nhap`
**Public** — tương ứng `AuthController.DangNhap()`
```json
// Request
{ "username": "admin", "password": "Admin@123" }

// Response 200
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "eyJ...",
    "expiresIn": 900,
    "user": {
      "id": 1,
      "username": "admin",
      "fullName": "Administrator",
      "email": "admin@fishshop.vn",
      "role": "ADMIN",
      "avatarUrl": null
    }
  }
}
```

### POST `/api/auth/auth/dang-ky`
**Public (Customer only)** — tương ứng `AuthController.DangKy()`
```json
// Request
{
  "fullName": "Nguyễn Văn A",
  "phone": "0901234567",
  "email": "customer@gmail.com",
  "password": "Pass@123"
}
```

### POST `/api/auth/auth/lam-moi-token`
**Public** — tương ứng `AuthController.LamMoiToken()`
```json
// Request
{ "refreshToken": "eyJ..." }

// Response: new accessToken + refreshToken
```

### POST `/api/auth/auth/dang-xuat`
**Bearer Token** — tương ứng `AuthController.DangXuat()`
```json
// Request: empty body, token in header
// Response 200: { "success": true, "message": "Đăng xuất thành công" }
```

---

## 📦 Products

### GET `/api/products/products/tim-kiem`
**Public** — `ProductsController.TimKiemSanPham()` — Danh sách sản phẩm với phân trang & filter

Query params:
| Param | Type | Default | Mô tả |
|-------|------|---------|-------|
| `page` | int | 1 | Trang |
| `pageSize` | int | 10 | Số item/trang |
| `search` | string | — | Tìm theo tên |
| `categoryId` | long | — | Lọc theo danh mục |
| `isFeatured` | bool | — | Sản phẩm nổi bật |
| `inStock` | bool | — | Còn hàng |
| `minPrice` | decimal | — | Giá từ |
| `maxPrice` | decimal | — | Giá đến |
| `sort` | string | `newest` | newest/oldest/price-asc/price-desc/sold |

```json
// Response 200
{
  "success": true,
  "data": {
    "items": [{
      "id": 1,
      "categoryId": 1,
      "categoryName": "Cá cảnh",
      "productCode": "SP001",
      "name": "Cá Betta Halfmoon",
      "slug": "ca-betta-halfmoon",
      "shortDescription": "Cá Betta đuôi nửa vầng trăng",
      "imageUrl": "https://...",
      "salePrice": 150000,
      "stockQuantity": 50,
      "soldQuantity": 120,
      "isFeatured": true,
      "status": 1
    }],
    "totalCount": 45,
    "page": 1,
    "pageSize": 10,
    "totalPages": 5
  }
}
```

### GET `/api/products/products/chi-tiet?id={id}`
**Public** — `ProductsController.ChiTietSanPham()`
```json
{
  "success": true,
  "data": {
    "id": 1,
    "categoryId": 1, "categoryName": "Cá cảnh",
    "productCode": "SP001", "sku": "BETTA-HM-001",
    "name": "Cá Betta Halfmoon",
    "slug": "ca-betta-halfmoon",
    "shortDescription": "...",
    "description": "<p>Rich text...</p>",
    "imageUrl": "...",
    "images": [
      { "id": 1, "imageUrl": "...", "isPrimary": true, "displayOrder": 0 }
    ],
    "costPrice": 50000,
    "salePrice": 150000,
    "stockQuantity": 50,
    "soldQuantity": 120,
    "weightGrams": 50,
    "status": 1,
    "isFeatured": true,
    "createdAt": "2026-01-01T00:00:00"
  }
}
```

### GET `/api/products/products/theo-slug?slug={slug}`
**Public** — `ProductsController.SanPhamTheoSlug()` — Dùng slug SEO-friendly

### POST `/api/products/products/them-moi-cap-nhat`
**[ApiKey] [ADMIN, STAFF]** — `ProductsController.ThemMoiCapNhatSanPham()`
```json
// Request
{
  "categoryId": 1,
  "name": "Cá Vàng Ryukin",
  "shortDescription": "...",
  "description": "<p>...</p>",
  "costPrice": 30000,
  "salePrice": 80000,
  "stockQuantity": 100,
  "isFeatured": false,
  "status": 1
}
```

> Upsert (thêm mới + cập nhật) dùng chung 1 endpoint `them-moi-cap-nhat` — nếu request có Id thì update, không có thì insert.

### POST `/api/products/products/xoa`
**[ApiKey] [ADMIN]** — `ProductsController.XoaSanPham()`
```json
{ "id": 1, "reason": "Không còn kinh doanh" }
```

---

## 🗂️ Categories

### GET `/api/products/categories/tim-kiem`
**Public** — `CategoriesController.TimKiemDanhMuc()`
```json
{
  "success": true,
  "data": [{
    "id": 1, "categoryCode": "FISH",
    "name": "Cá cảnh", "slug": "ca-canh",
    "description": "...", "imageUrl": "...",
    "parentId": null, "displayOrder": 1,
    "children": [
      { "id": 4, "name": "Cá nhiệt đới", "slug": "ca-nhiet-doi", ... }
    ]
  }]
}
```

### POST `/api/products/categories/them-moi-cap-nhat` & POST `/api/products/categories/xoa`
**[ApiKey] [ADMIN]**

---

## 🛒 Cart

### GET `/api/orders/cart/gio-hang-hien-tai`
**[ApiKey] Bearer [CUSTOMER]** — `CartController.GioHangHienTai()`
```json
{
  "success": true,
  "data": {
    "id": 1,
    "items": [{
      "id": 1, "productId": 1,
      "productName": "Cá Betta Halfmoon",
      "imageUrl": "...",
      "quantity": 2,
      "unitPrice": 150000,
      "subtotal": 300000
    }],
    "totalItems": 2,
    "totalAmount": 300000
  }
}
```

### POST `/api/orders/cart/them-san-pham`
**[ApiKey] Bearer [CUSTOMER]**
```json
// Request
{ "productId": 1, "quantity": 2 }
```

### POST `/api/orders/cart/cap-nhat-so-luong`
**[ApiKey] Bearer [CUSTOMER]**
```json
{ "quantity": 3 }
```

### POST `/api/orders/cart/xoa-san-pham`
**[ApiKey] Bearer [CUSTOMER]**

### POST `/api/orders/cart/xoa-gio-hang`
**[ApiKey] Bearer [CUSTOMER]**

---

## 📦 Orders

### POST `/api/orders/orders/dat-hang`
**Public (có thể guest checkout)** — `OrdersController.DatHang()`
```json
// Request
{
  "customerId": 1,
  "addressId": 2,
  "promoCode": "SALE10",
  "paymentMethod": "COD",
  "customerName": "Nguyễn Văn A",
  "customerPhone": "0901234567",
  "customerAddress": "123 Đường Trần Phú, Hà Đông, Hà Nội",
  "note": "Giao buổi sáng",
  "source": "ONLINE",
  "items": [
    { "productId": 1, "quantity": 2, "unitPrice": 150000 }
  ]
}

// Response 201
{
  "success": true,
  "data": {
    "orderCode": "FS20260406001",
    "totalAmount": 280000,
    "orderStatus": "PENDING",
    "paymentMethod": "COD"
  }
}
```

### GET `/api/orders/orders/don-hang-cua-toi`
**[ApiKey] Bearer [CUSTOMER]**
Query: `page`, `pageSize`, `status`

### GET `/api/orders/orders/chi-tiet?code={orderCode}`
**[ApiKey] Bearer [CUSTOMER/ADMIN/STAFF]**

### POST `/api/orders/orders/huy-don`
**[ApiKey] Bearer [CUSTOMER]** (chỉ khi status = PENDING)
```json
{ "orderCode": "FS20260406001", "reason": "Tôi muốn thay đổi sản phẩm" }
```

### GET `/api/orders/orders/tim-kiem` (Admin)
**[ApiKey] Bearer [ADMIN, STAFF]**
Query: `page`, `pageSize`, `status`, `source`, `fromDate`, `toDate`, `keyword`

### POST `/api/orders/orders/cap-nhat-trang-thai` (Admin)
**[ApiKey] Bearer [ADMIN, STAFF]**
```json
{ "orderCode": "FS20260406001", "status": "CONFIRMED", "note": "Đã xác nhận, chuẩn bị hàng" }
```

---

## 💳 Payments

### POST `/api/orders/payments/tao-giao-dich`
**[ApiKey] Public hoặc Bearer** — `PaymentsController.TaoGiaoDich()`
```json
// Request
{ "method": "BANK_TRANSFER", "amount": 280000 }

// Response (khi BANK_TRANSFER)
{
  "success": true,
  "data": {
    "paymentCode": "PAY20260406001",
    "bankName": "Vietcombank",
    "accountNumber": "1234567890",
    "accountName": "CONG TY CA CANH",
    "transferContent": "FS20260406001",
    "amount": 280000,
    "qrCodeUrl": "https://..." // QR code image
  }
}
```

---

## 🏷️ Promotions

### POST `/api/orders/promotions/kiem-tra-ma`
**Public** — `PromotionsController.KiemTraMa()`
```json
// Request
{ "code": "SALE10", "orderAmount": 300000 }

// Response
{
  "success": true,
  "data": {
    "promoCode": "SALE10",
    "discountType": "PERCENT",
    "discountValue": 10,
    "discountAmount": 30000,
    "finalAmount": 270000
  }
}
```

---

## 📊 Dashboard (Admin)

### GET `/api/admin/dashboard/thong-ke-tong-quat`
**[ApiKey] Bearer [ADMIN, STAFF]** — `DashboardController.ThongKeTongQuat()`
```json
{
  "success": true,
  "data": {
    "todayOrders": 15,
    "todayRevenue": 3500000,
    "todayProfit": 1200000,
    "totalOrders": 450,
    "totalRevenue": 125000000,
    "totalCustomers": 120,
    "lowStockCount": 5,
    "pendingOrders": 8
  }
}
```

### GET `/api/admin/dashboard/bieu-do`
**[ApiKey] Bearer [ADMIN, STAFF]** — `DashboardController.BieuDo()`
Query: `days=7` (mặc định 7 ngày)
```json
{
  "success": true,
  "data": {
    "revenueChart": [
      { "date": "2026-03-31", "revenue": 2500000, "orders": 10 }
    ],
    "topProducts": [
      { "productName": "Cá Betta Halfmoon", "sold": 45 }
    ],
    "orderStatusChart": [
      { "status": "COMPLETED", "count": 300 }
    ]
  }
}
```

---

## 📝 Blog

### GET `/api/content/blog/tim-kiem`
**Public** — `BlogController.TimKiemBaiViet()`
Query: `page`, `pageSize`, `categoryId`, `status=PUBLISHED`

### GET `/api/content/blog/chi-tiet-theo-slug?slug={slug}`
**Public** — `BlogController.ChiTietTheoSlug()`

### POST `/api/content/blog/them-moi-cap-nhat` & POST `/api/content/blog/xoa`
**[ApiKey] Bearer [ADMIN]**

---

## 📞 Contact

### POST `/api/content/contact/gui-lien-he`
**Public** — `ContactController.GuiLienHe()`
```json
{
  "fullName": "Nguyễn Văn A",
  "email": "test@gmail.com",
  "phone": "0901234567",
  "subject": "Hỏi về cá Koi",
  "message": "Tôi muốn hỏi về..."
}
```

### GET `/api/content/contact/tim-kiem` (Admin)
**[ApiKey] Bearer [ADMIN, STAFF]**

### POST `/api/content/contact/cap-nhat-trang-thai` (Admin)
**[ApiKey] Bearer [ADMIN, STAFF]**
```json
{ "status": "RESOLVED" }
```

---

## 👤 Profile (Customer)

### GET `/api/auth/auth/thong-tin-nguoi-dung`
**[ApiKey] Bearer [CUSTOMER]**

### POST `/api/auth/auth/cap-nhat-thong-tin`
**[ApiKey] Bearer [CUSTOMER]**
```json
{ "fullName": "...", "phone": "...", "email": "...", "dateOfBirth": "1990-01-01" }
```

### GET `/api/orders/orders/danh-sach-dia-chi`
**[ApiKey] Bearer [CUSTOMER]**

### POST `/api/orders/orders/them-dia-chi`
**[ApiKey] Bearer [CUSTOMER]**
```json
{
  "receiverName": "...", "receiverPhone": "...",
  "province": "Hà Nội", "district": "Hoàn Kiếm",
  "ward": "Phường Tràng Tiền",
  "addressLine": "123 Đường Đinh Tiên Hoàng",
  "isDefault": true
}
```

---

## 📦 Inventory (Admin)

### GET `/api/products/inventory/lich-su-giao-dich`
**[ApiKey] Bearer [ADMIN, STAFF]**
Query: `page`, `pageSize`, `productId`, `type` (IMPORT/EXPORT/...)

### POST `/api/products/inventory/nhap-hang`
**[ApiKey] Bearer [ADMIN]**
```json
{
  "productId": 1,
  "quantity": 50,
  "unitCost": 45000,
  "note": "Nhập lô cá Betta tháng 4"
}
```

---

## 📈 Reports (Admin)

### GET `/api/admin/reports/doanh-thu`
**[ApiKey] Bearer [ADMIN]**
Query: `fromDate`, `toDate`, `groupBy` (day/week/month)

### GET `/api/admin/reports/san-pham-ban-chay`
**[ApiKey] Bearer [ADMIN]**
Query: `fromDate`, `toDate`, `limit=10`

### GET `/api/admin/reports/tong-hop-don-hang`
**[ApiKey] Bearer [ADMIN]**
Query: `fromDate`, `toDate`
