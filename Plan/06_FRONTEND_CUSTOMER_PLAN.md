# 🛒 Kế Hoạch Frontend Customer — FISH SHOP

> React 19 + Vite + TypeScript + Ant Design 5 · Port: 5173

---

## Tổng Quan

FE-Customer (thư mục `/FE-Customer/`) là **storefront** cho khách hàng mua sắm:
- Duyệt sản phẩm theo danh mục, tìm kiếm
- Xem giỏ hàng & thanh toán
- Theo dõi lịch sử đơn
- Đọc blog, liên hệ

---

## Cấu Trúc Trang Hiện Tại

```
✅ Đã có (cần kết nối API thực):
├── Home/           — Trang chủ với banner, flash sale, danh mục
├── Products/       — Danh sách sản phẩm
├── Cart/           — Giỏ hàng
├── Checkout/       — Thanh toán
├── Login/          — Đăng nhập
├── Register/       — Đăng ký
├── MyOrders/       — Lịch sử đơn hàng
├── About/          — Giới thiệu
├── Blog/           — Blog
├── Categories/     — Danh mục
├── Promotions/     — Khuyến mãi
├── Services/       — Dịch vụ
├── Contact/        — Liên hệ
├── Policies/       — Chính sách
└── SearchResults/  — Kết quả tìm kiếm

🆕 Cần bổ sung:
├── ProductDetail/  — Trang chi tiết sản phẩm (slug)
├── OrderDetail/    — Chi tiết đơn hàng đã đặt
└── Profile/        — Thông tin cá nhân & địa chỉ
```

---

## Components Hiện Có

| Component | Trạng thái | Ghi chú |
|-----------|------------|---------|
| `CustomerLayout` | ✅ Có | Header + Nav + Footer |
| `HeroBanner` | ✅ Có | Slideshow trang chủ |
| `CategoryGrid` | ✅ Có | Lưới danh mục |
| `ProductCard` | ✅ Có | Card SP với badge, rating, favorites |
| `FlashSale` | ✅ Có | Khu vực Flash Sale với countdown |
| `PromoBanner` | ✅ Có | Banner khuyến mãi |
| `OceanBackground` | ✅ Có | Hiệu ứng nền đại dương |
| `BubbleBackground` | ✅ Có | Hiệu ứng bong bóng |
| `LoadingScreen` | ✅ Có | Màn hình loading |
| `Breadcrumb` | 🆕 Cần | Breadcrumb navigation |
| `ProductFilter` | 🆕 Cần | Sidebar filter SP |
| `SearchBar` | 🆕 Cần | Thanh tìm kiếm nâng cao |
| `OrderTracker` | 🆕 Cần | Theo dõi trạng thái đơn |
| `AddressForm` | 🆕 Cần | Form địa chỉ giao hàng |

---

## Design System (FE-Customer)

### Màu sắc
```scss
// Ocean theme — phù hợp với cá cảnh
$primary-blue: #0066cc;       // Xanh dương đại dương
$primary-dark: #003d7a;       // Xanh đậm
$secondary-teal: #00b4d8;     // Teal nhạt
$accent-gold: #ffd60a;        // Vàng điểm nhấn (Flash sale)
$danger-red: #ef233c;         // Đỏ giảm giá
$success-green: #52b788;      // Xanh lá thành công
$bg-light: #f0f7ff;           // Nền sáng xanh nhạt
$text-dark: #1a1a2e;          // Text chính
$text-gray: #6c757d;          // Text phụ
```

### Typography
```scss
// src/index.scss
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap');

body {
  font-family: 'Inter', -apple-system, sans-serif;
}
```

---

## State Management — Giỏ Hàng

### useCart Hook
```typescript
// src/hooks/useCart.ts
export interface CartState {
  items: CartItem[];
  totalItems: number;
  totalAmount: number;
}

export interface CartItem {
  productId: number;
  productName: string;
  imageUrl?: string;
  quantity: number;
  unitPrice: number;
}

// Lưu giỏ hàng:
// - Nếu chưa đăng nhập: localStorage
// - Nếu đã đăng nhập: sync lên API Cart

export function useCart() {
  const [cart, setCart] = useState<CartState>(loadFromStorage());
  
  const addItem = (product: Product, qty = 1) => { /* ... */ };
  const removeItem = (productId: number) => { /* ... */ };
  const updateQty = (productId: number, qty: number) => { /* ... */ };
  const clearCart = () => { /* ... */ };
  const applyPromo = async (code: string) => { /* ... */ };
  
  return { cart, addItem, removeItem, updateQty, clearCart, applyPromo };
}
```

### useAuth Hook
```typescript
// src/hooks/useAuth.ts
export function useAuth() {
  const [user, setUser] = useState<User | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  
  const login = async (credentials: LoginRequest) => { /* ... */ };
  const logout = () => { /* clear tokens, redirect */ };
  const refreshToken = async () => { /* ... */ };
  
  return { user, isAuthenticated, login, logout };
}
```

---

## Chi Tiết Các Trang

### 1. Home — Trang Chủ
**Route:** `/`

**Bố cục:**
```
[HeroBanner]          — Slideshow 3-5 ảnh với nút CTA
[CategoryGrid]        — 3-4 danh mục nổi bật
[FlashSale]           — Flash sale với countdown timer
[FeaturedProducts]    — Grid 8-12 sản phẩm nổi bật
[PromoBanner]         — Banner khuyến mãi đặc biệt
[NewArrivals]         — Hàng mới về
[BlogHighlights]      — 3 bài blog mới nhất
[Footer]              — Links, contact, social
```

**APIs:**
- `GET /products?isFeatured=true&pageSize=8`
- `GET /products?sort=newest&pageSize=8`
- `GET /categories`
- `GET /promotions/active`
- `GET /blog?status=PUBLISHED&pageSize=3`

---

### 2. Products — Danh Sách Sản Phẩm
**Route:** `/products`

**Layout:**
```
[Breadcrumb]
[Filter Sidebar]  |  [Sort + Product Grid (3-4 cột)]
                  |  [Pagination]
```

**Filter Sidebar:**
- Danh mục (checkbox tree)
- Khoảng giá (range slider)
- Trạng thái (Còn hàng / Hết hàng)
- Sắp xếp: Mới nhất, Bán chạy, Giá tăng/giảm

**APIs:** `GET /products?page=1&pageSize=12&categoryId=&minPrice=&maxPrice=&sort=`

---

### 3. ProductDetail — Chi Tiết Sản Phẩm [MỚI]
**Route:** `/products/:slug`

**Bố cục:**
```
[Breadcrumb]
[Image Gallery]  |  [Product Info]
Left:             |  Right:
- Main image      |  - Tên sản phẩm
- Thumbnail list  |  - Giá (gạch giá gốc nếu có giảm)
                  |  - Danh mục, SKU
                  |  - Tồn kho
                  |  - Số lượng + [Thêm vào giỏ]
                  |  - [Mua ngay]
                  |  - Mô tả ngắn
[Tabs]
  - Mô tả đầy đủ (rich text)
  - Thông tin kỹ thuật
  - Đánh giá (để sau)
[Related Products]  — 4-6 sản phẩm cùng danh mục
```

**APIs:**
- `GET /products/slug/{slug}`
- `GET /products?categoryId={catId}&pageSize=6` (related)

---

### 4. Cart — Giỏ Hàng
**Route:** `/cart`

**Layout:**
```
[Breadcrumb]
┌─────────────────────────────┬──────────────────┐
│ Danh sách sản phẩm trong GH │  Tóm tắt đơn    │
│ (ảnh, tên, giá, qty±, xóa) │  Tạm tính        │
│                              │  [Nhập mã giảm] │
│                              │  Phí ship        │
│                              │  Tổng cộng       │
│                              │  [Thanh toán]    │
└─────────────────────────────┴──────────────────┘
```

**APIs:**
- Nếu guest: localStorage
- Nếu logged in: `GET /cart`, `POST /cart/items`, `PUT /cart/items/{id}`, `DELETE /cart/items/{id}`
- `POST /promotions/validate` (kiểm tra mã)

---

### 5. Checkout — Thanh Toán
**Route:** `/checkout` | **Cần đăng nhập**

**Bước:**
```
1. Chọn/nhập địa chỉ giao hàng
2. Chọn phương thức: COD | Chuyển khoản
3. Ghi chú đơn hàng
4. Xác nhận và đặt hàng
5. Màn hình thành công + mã đơn
```

**APIs:**
- `GET /profile/addresses`
- `POST /profile/addresses` (nếu thêm mới)
- `POST /orders`
- `POST /payments/{orderId}` (nếu bank transfer → hiện QR)

---

### 6. MyOrders — Lịch Sử Đơn
**Route:** `/my-orders` | **Cần đăng nhập**

**Hiển thị:**
- List đơn hàng với trạng thái có màu
- Expandable: chi tiết sản phẩm
- Nút "Hủy đơn" (chỉ khi PENDING)

**Route con:**
- `/my-orders/{orderCode}` — Chi tiết đơn hàng

**APIs:** `GET /orders/my-orders`, `POST /orders/{code}/cancel`

---

### 7. Login & Register
**Route:** `/login`, `/register`

**Features:**
- Form đăng nhập với validation (Ant Design Form)
- Link "Quên mật khẩu" (tùy chọn Phase 2)
- Redirect sau login về trang trước hoặc `/`
- Đăng ký: Họ tên, SĐT, Email, Mật khẩu

---

### 8. Profile — Thông Tin Cá Nhân [MỚI]
**Route:** `/profile` | **Cần đăng nhập**

**Tabs:**
- Thông tin cá nhân (sửa tên, email, SĐT)
- Danh sách địa chỉ (thêm/sửa/xóa/đặt mặc định)
- Đổi mật khẩu

---

### 9. Blog
**Route:** `/blog`, `/blog/:slug`

**Blog list:** Grid 3 cột, phân trang
**Blog detail:** Tiêu đề, ảnh thumbnail, ngày đăng, nội dung rich text

---

### 10. Contact — Liên Hệ
**Route:** `/contact`

**Form:** Họ tên, Email, SĐT, Chủ đề, Nội dung
**API:** `POST /contact`

---

## Header Navigation

```
[Logo - Cá Cảnh Shop]  [Danh mục ▾]  [Blog]  [Khuyến mãi]  [Liên hệ]
                                    [🔍 Tìm kiếm]  [🛒 (3)]  [👤 Đăng nhập]
```

**Dropdown Danh mục:** Hiển thị categories từ API theo dạng mega menu

---

## Footer

```
┌─────────────────────────────────────────────────────────┐
│  [Logo]              Sản phẩm     Hỗ trợ    Kết nối    │
│  Địa chỉ: ...        Cá cảnh      Chính sách  Facebook  │
│  ĐT: 0xxx-xxx-xxx    Phụ kiện     Hướng dẫn  Zalo      │
│  Email: ...          Thức ăn      Liên hệ    Instagram  │
│                      Blog                               │
│  © 2026 Cá Cảnh Shop. All rights reserved.             │
└─────────────────────────────────────────────────────────┘
```

---

## Route Config

```typescript
// src/routes/Routes.tsx
const routes = [
  // Public
  { path: '/', element: <Home /> },
  { path: '/products', element: <Products /> },
  { path: '/products/:slug', element: <ProductDetail /> },
  { path: '/categories/:slug', element: <Categories /> },
  { path: '/search', element: <SearchResults /> },
  { path: '/promotions', element: <Promotions /> },
  { path: '/blog', element: <Blog /> },
  { path: '/blog/:slug', element: <BlogDetail /> },
  { path: '/about', element: <About /> },
  { path: '/contact', element: <Contact /> },
  { path: '/services', element: <Services /> },
  { path: '/policies', element: <Policies /> },
  { path: '/login', element: <Login /> },
  { path: '/register', element: <Register /> },
  { path: '/cart', element: <Cart /> },
  
  // Protected (cần đăng nhập)
  { path: '/checkout', element: <ProtectedRoute><Checkout /></ProtectedRoute> },
  { path: '/my-orders', element: <ProtectedRoute><MyOrders /></ProtectedRoute> },
  { path: '/my-orders/:code', element: <ProtectedRoute><OrderDetail /></ProtectedRoute> },
  { path: '/profile', element: <ProtectedRoute><Profile /></ProtectedRoute> },
  
  // Fallback
  { path: '*', element: <Page404 /> },
];
```

---

## Thứ Tự Implement FE Customer

```
Phase 1 — Kết Nối API + State Management
  [ ] Cập nhật axiosClient, constants, interfaces
  [ ] Implement useCart hook (localStorage + API sync)
  [ ] Implement useAuth hook + ProtectedRoute
  [ ] Token refresh interceptor

Phase 2 — Trang Danh Sách & Chi Tiết
  [ ] Products với filter sidebar thực
  [ ] ProductDetail (trang chi tiết slug)
  [ ] Categories (slug-based)
  [ ] SearchResults

Phase 3 — Mua Sắm
  [ ] Cart (kết nối API)
  [ ] Checkout (form địa chỉ, đặt hàng)
  [ ] MyOrders + OrderDetail
  [ ] Login/Register thực

Phase 4 — Nội Dung & Profile
  [ ] Blog list + detail (từ API)
  [ ] Contact (gửi form thực)
  [ ] Profile + Address management
  [ ] Home (kết nối tất cả API thực)
```
