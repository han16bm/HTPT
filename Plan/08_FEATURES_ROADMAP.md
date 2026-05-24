# 🗺️ Lộ Trình Tính Năng — FISH SHOP

> Phân chia theo giai đoạn phát triển (Phase)

---

## Tổng Quan Giai Đoạn

| Giai đoạn | Thời gian | Mục tiêu |
|-----------|-----------|----------|
| **Phase 1** | Tuần 1-2 | Nền tảng Backend + Auth |
| **Phase 2** | Tuần 3-4 | Sản phẩm & Danh mục (FE + BE) |
| **Phase 3** | Tuần 5-6 | Giỏ hàng, Đặt hàng, Thanh toán |
| **Phase 4** | Tuần 7-8 | Admin hoàn chỉnh + POS |
| **Phase 5** | Tuần 9-10 | Nội dung & UX polish |
| **Phase 6** | Tuần 11+ | Nâng cao & tối ưu |

---

## Phase 1 — Nền Tảng (Tuần 1-2)

### Backend
- [ ] Cấu hình SQL Server connection string
- [ ] Implement Domain Entities đầy đủ
- [ ] EF Core Configurations (IEntityTypeConfiguration)
- [ ] AppDbContext với SQL Server
- [ ] Generic Repository + UnitOfWork
- [ ] JwtService (generate/validate access + refresh token)
- [ ] PasswordService (BCrypt hash/verify)
- [ ] Login Command + Handler
- [ ] Register Command + Handler (CUSTOMER role)
- [ ] AuthController (login, register, refresh, logout)
- [ ] Program.cs chuẩn (JWT, Serilog, Swagger, CORS)
- [ ] Middleware xử lý exception chung

### Frontend (cả 2 FE)
- [ ] Cập nhật axiosClient.ts với token interceptor + refresh
- [ ] Cập nhật tất cả interfaces TypeScript
- [ ] Implement useAuth hook
- [ ] ProtectedRoute component
- [ ] Kết nối Login form thực tế → API

### Kiểm Tra Phase 1
- [ ] `POST /api/auth/login` hoạt động với role ADMIN
- [ ] JWT token hợp lệ
- [ ] FE Admin đăng nhập thành công, lưu token, redirect dashboard
- [ ] Route protected hoạt động

---

## Phase 2 — Sản Phẩm & Danh Mục (Tuần 3-4)

### Backend
- [ ] CategoryRepository với GetTree (parent/children)
- [ ] GetCategoriesQuery + Handler
- [ ] CRUD Category Commands + Handlers
- [ ] CategoriesController (Customer + Admin)
- [ ] ProductRepository với phân trang, filter, search
- [ ] GetProductsQuery + Handler (nhiều filter)
- [ ] GetProductByIdQuery + GetProductBySlugQuery
- [ ] CRUD Product Commands + Handlers
- [ ] ProductsController (Customer + Admin)
- [ ] Upload ảnh sản phẩm endpoint (lưu vào wwwroot hoặc cloud)

### FE Admin
- [ ] Products table kết nối API thực
- [ ] Modal thêm/sửa sản phẩm (bao gồm upload ảnh)
- [ ] Filter theo danh mục thực
- [ ] Categories CRUD page (mới)
- [ ] Sidebar menu cập nhật

### FE Customer
- [ ] Products page với filter sidebar thực
- [ ] CategoryGrid kết nối API
- [ ] ProductDetail page mới (đường dẫn slug)
- [ ] SearchResults kết nối API
- [ ] HeroBanner kết nối dữ liệu thực

### Kiểm Tra Phase 2
- [ ] FE Customer: xem danh sách SP, lọc, xem chi tiết
- [ ] FE Admin: CRUD SP và danh mục hoàn chỉnh
- [ ] Phân trang hoạt động đúng
- [ ] Search tìm kiếm theo tên

---

## Phase 3 — Giỏ Hàng & Đặt Hàng (Tuần 5-6)

### Backend
- [ ] CartService: get/add/update/remove/clear items
- [ ] CartController (Customer)
- [ ] Promotion validation endpoint
- [ ] CreateOrderCommand + Handler:
  - Tạo ORDER record
  - Tạo ORDER_ITEMS records  
  - Trừ STOCK_QUANTITY
  - Ghi INVENTORY_TRANSACTIONS (SALE)
  - Clear shopping_cart
- [ ] GetMyOrdersQuery + Handler
- [ ] CancelOrderCommand + Handler
- [ ] OrdersController (Customer)
- [ ] PaymentsController (COD + Bank Transfer cơ bản)

### FE Customer
- [ ] useCart hook hoàn chỉnh (localStorage + API sync)
- [ ] Cart page kết nối API
- [ ] Checkout page hoàn chỉnh:
  - Chọn địa chỉ đã lưu hoặc nhập mới
  - Áp mã giảm giá
  - Chọn phương thức thanh toán
  - Xác nhận đơn
- [ ] Màn hình thành công sau đặt hàng
- [ ] MyOrders kết nối API
- [ ] Register page thực

### FE Admin
- [ ] Orders page (thay thế Invoices):
  - Filter đa điều kiện
  - Cập nhật trạng thái đơn
  - Xem chi tiết expandable

### Kiểm Tra Phase 3
- [ ] Luồng đặt hàng end-to-end: SP → Giỏ → Checkout → Đơn thành công
- [ ] Tồn kho giảm sau khi đặt hàng
- [ ] Admin thấy đơn mới, đổi trạng thái được
- [ ] Khách xem lịch sử đơn, hủy đơn PENDING

---

## Phase 4 — Admin Hoàn Chỉnh + POS (Tuần 7-8)

### Backend
- [ ] DashboardController (stats + charts)
- [ ] SalesController (POS endpoint): tạo đơn nguồn POS
- [ ] InventoryController (import hàng)
- [ ] PromotionsController (CRUD)
- [ ] CustomersController (Admin): list, view, update
- [ ] ProfileController (Customer): view/edit, addresses CRUD

### FE Admin
- [ ] Dashboard page: biểu đồ thực với Recharts/AntD Charts
- [ ] Sales (POS) kết nối API thực, in hóa đơn
- [ ] Inventory page (nhập hàng, lịch sử)
- [ ] Promotions page (CRUD)
- [ ] Customers page mở rộng (xem đơn của khách)
- [ ] Profile dropdown: thông tin tài khoản

### FE Customer
- [ ] Profile page: xem/sửa thông tin
- [ ] Address management: thêm/sửa/xóa/mặc định
- [ ] OrderDetail page (xem chi tiết 1 đơn)
- [ ] Promotions page kết nối API

### Kiểm Tra Phase 4
- [ ] POS bán hàng thành công, tạo đơn POS
- [ ] Dashboard hiển thị số liệu thực
- [ ] Nhập hàng → tồn kho tăng
- [ ] Khách sửa thông tin cá nhân, quản lý địa chỉ

---

## Phase 5 — Nội Dung & Hoàn Thiện UX (Tuần 9-10)

### Backend
- [ ] BlogController (CRUD, publish)
- [ ] ContactController (nhận liên hệ, cập nhật trạng thái)
- [ ] ReportsController (doanh thu, top SP có filter ngày)

### FE Admin
- [ ] Blog management page (CRUD + rich text editor)
- [ ] Contacts page (xem, xử lý)
- [ ] Reports page (biểu đồ theo khoảng ngày)
- [ ] UX polish: loading states, empty states, error states
- [ ] Responsive (tablet support)

### FE Customer
- [ ] Blog list + Blog detail kết nối API
- [ ] Contact form gửi thực
- [ ] Về chúng tôi (About) nội dung thực
- [ ] Flash sale kết nối promotions API
- [ ] UX polish: skeleton loading, animations
- [ ] SEO: meta tags, Open Graph
- [ ] Responsive mobile hoàn chỉnh

---

## Phase 6 — Nâng Cao (Tuần 11+)

### Tính năng nâng cao (ưu tiên cao)
- [ ] **Đánh giá sản phẩm** (PRODUCT_REVIEWS table mới)
- [ ] **Wishlist** (WISHLISTS table mới)
- [ ] **Tìm kiếm nâng cao** (full-text search)
- [ ] **Email thông báo** đơn hàng (SMTP)
- [ ] **QR Code thanh toán** thực (VietQR)
- [ ] **Image optimization** (resize, WebP)

### Tính năng nâng cao (ưu tiên thấp)
- [ ] **Đăng nhập Google** (OAuth 2.0)
- [ ] **Lọc sản phẩm theo attributes** (màu, kích thước)
- [ ] **Chat support** (Tawk.to hoặc tương tự)
- [ ] **Export báo cáo Excel**
- [ ] **Notification real-time** (SignalR)
- [ ] **Mobile App** (React Native)

---

## Tổng Hợp Tính Năng Theo Module

### Module Auth & Users
| Tính năng | FE | BE | Phase |
|-----------|----|----|-------|
| Đăng nhập | ✅Có | 🔧Cần | 1 |
| Đăng ký (Customer) | ✅Có | 🔧Cần | 1 |
| Refresh Token | 🔧Cần | 🔧Cần | 1 |
| Đổi mật khẩu | 🔧Cần | 🔧Cần | 4 |
| Quên mật khẩu | ─ | ─ | 6 |
| Đăng nhập Google | ─ | ─ | 6 |

### Module Sản Phẩm
| Tính năng | FE | BE | Phase |
|-----------|----|----|-------|
| Danh sách SP (phân trang) | ✅Có | 🔧Cần | 2 |
| Filter đa điều kiện | ✅Có | 🔧Cần | 2 |
| Chi tiết SP (slug) | 🔧Cần | 🔧Cần | 2 |
| CRUD SP (Admin) | ✅Có | 🔧Cần | 2 |
| Upload ảnh SP | 🔧Cần | 🔧Cần | 2 |
| Sản phẩm nổi bật | ✅Có | 🔧Cần | 2 |
| Đánh giá sản phẩm | ─ | ─ | 6 |
| Wishlist | ─ | ─ | 6 |

### Module Đặt Hàng
| Tính năng | FE | BE | Phase |
|-----------|----|----|-------|
| Giỏ hàng (localStorage) | 🔧Cần | — | 3 |
| Giỏ hàng (API sync) | 🔧Cần | 🔧Cần | 3 |
| Thanh toán COD | 🔧Cần | 🔧Cần | 3 |
| Thanh toán Bank Transfer | 🔧Cần | 🔧Cần | 3 |
| Mã giảm giá | 🔧Cần | 🔧Cần | 3 |
| Lịch sử đơn | ✅Có | 🔧Cần | 3 |
| Hủy đơn | ✅Có | 🔧Cần | 3 |
| Theo dõi đơn | 🔧Cần | 🔧Cần | 3 |

### Module Admin
| Tính năng | FE | BE | Phase |
|-----------|----|----|-------|
| Dashboard thống kê | ✅Có | 🔧Cần | 4 |
| Biểu đồ doanh thu | 🔧Cần | 🔧Cần | 4 |
| POS bán hàng | ✅Có | 🔧Cần | 4 |
| Quản lý đơn hàng | ✅Có | 🔧Cần | 4 |
| Quản lý khách hàng | ✅Có | 🔧Cần | 4 |
| Nhập kho | 🔧Cần | 🔧Cần | 4 |
| Khuyến mãi | 🔧Cần | 🔧Cần | 4 |
| Blog (Admin) | 🔧Cần | 🔧Cần | 5 |
| Báo cáo | 🔧Cần | 🔧Cần | 5 |

---

## Ưu Tiên Tuyệt Đối (Must-Have MVP)

1. ✅ Đăng nhập / Đăng ký
2. ✅ Danh sách sản phẩm + Chi tiết
3. ✅ Giỏ hàng + Đặt hàng (COD)
4. ✅ Lịch sử đơn (khách)
5. ✅ Quản lý sản phẩm (Admin)
6. ✅ Cập nhật trạng thái đơn (Admin)
7. ✅ Dashboard thống kê cơ bản
