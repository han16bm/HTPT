# 🐟 FISH SHOP — Tổng Quan Dự Án

> **Website thương mại điện tử bán cá cảnh & phụ kiện**
> Stack: React + Vite + TypeScript (FE) · .NET 8 + EF Core + SQL Server (BE)
> Ngày lập kế hoạch: 06/04/2026

---

## 📂 Danh Sách Tài Liệu Kế Hoạch

| File | Nội dung |
|------|----------|
| `00_OVERVIEW.md` | **[File này]** Tổng quan & chỉ mục |
| `01_ARCHITECTURE.md` | Kiến trúc hệ thống tổng thể |
| `02_PROJECT_STRUCTURE.md` | Cấu trúc thư mục chi tiết |
| `03_DATABASE.md` | Thiết kế CSDL SQL Server + EF Core |
| `04_BACKEND_PLAN.md` | Kế hoạch phát triển Backend .NET 8 |
| `05_FRONTEND_ADMIN_PLAN.md` | Kế hoạch FE Admin (React + Vite + TS) |
| `06_FRONTEND_CUSTOMER_PLAN.md` | Kế hoạch FE Khách hàng (React + Vite + TS) |
| `07_API_CONTRACTS.md` | Hợp đồng API (endpoints, request/response) |
| `08_FEATURES_ROADMAP.md` | Lộ trình tính năng theo giai đoạn |
| `09_CONVENTIONS.md` | Quy ước code, đặt tên, commit |
| `10_DEPLOYMENT.md` | Hướng dẫn triển khai & môi trường |

---

## 🎯 Mục Tiêu Dự Án

### Tầm nhìn
Xây dựng nền tảng thương mại điện tử **chuyên biệt cá cảnh** với:
- **Trải nghiệm khách hàng** hiện đại — tìm kiếm, mua hàng, theo dõi đơn
- **Hệ thống quản trị** mạnh — hàng tồn kho, doanh thu, báo cáo
- **Kiến trúc mở rộng** — phục vụ cả kênh online và POS

### Đối tượng người dùng
| Nhóm | Vai trò | Giao diện |
|------|---------|-----------|
| Khách hàng | Mua sắm online | FE-Customer |
| Nhân viên | Bán hàng tại quầy (POS) | FE Admin → Sales |
| Admin | Quản trị toàn hệ thống | FE Admin → Full |

---

## 🛠️ Công Nghệ Sử Dụng

### Frontend
| Hạng mục | Công nghệ | Phiên bản |
|----------|-----------|-----------|
| Framework | React | ^19.x |
| Build tool | Vite | ^6.x |
| Ngôn ngữ | TypeScript | ~5.9.x |
| UI Library | Ant Design | ^5.x |
| HTTP client | Axios | ^1.x |
| Routing | React Router DOM | ^7.x |
| Styling | SCSS Modules | — |
| Ngày giờ | Day.js | ^1.x |

### Backend
| Hạng mục | Công nghệ | Phiên bản |
|----------|-----------|-----------|
| Framework | ASP.NET Core | .NET 8 |
| ORM | Entity Framework Core | 8.x |
| Database driver | Microsoft.EntityFrameworkCore.SqlServer | 8.x |
| Xác thực | JWT Bearer | — |
| Logging | Serilog | — |
| API Docs | Swagger / OpenAPI | — |
| Pattern | Clean Architecture + CQRS | — |

### Database
| Hạng mục | Công nghệ |
|----------|-----------|
| DBMS | SQL Server 2019+ / 2022 |
| Migration | EF Core Migrations |

---

## 📊 Hiện Trạng Codebase

### Đã có sẵn
- ✅ **SQL Server setup + seed** đầy đủ (`/API/database/sqlserver_full_setup.sql`)
  - 15 bảng: ROLES, USERS, CUSTOMER_PROFILES, CUSTOMER_ADDRESSES, CATEGORIES,
    PRODUCTS, PRODUCT_IMAGES, INVENTORY_TRANSACTIONS, PROMOTIONS,
    SHOPPING_CARTS, CART_ITEMS, ORDERS, ORDER_ITEMS, PAYMENTS,
    BLOG_POSTS, CONTACT_MESSAGES
- ✅ **API Solution** khởi tạo (`/API/API.sln`)
  - `FishApp.API` — API chính (skeleton, chưa implement)
  - `FishApp.Admin.API` — API admin (trong Src/)
  - `FishShop.Domain`, `FishShop.CQRS`, `FishShop.Infrastructure` (Core/)
  - `API.Gateway` — Gateway entry point
- ✅ **FE Admin** (`/FE/`) — React + Vite + TS + Ant Design
  - Pages: Login, Home, Products, Customers, Invoices, Sales
  - Có API layer, routing, interfaces cơ bản
- ✅ **FE Customer** (`/FE-Customer/`) — React + Vite + TS + Ant Design
  - Pages: Home, Products, Cart, Checkout, Login, MyOrders + nhiều trang
  - Có components: ProductCard, HeroBanner, FlashSale, CategoryGrid

### Cần phát triển
- ⬜ Implement Backend (Domain, CQRS, Infrastructure, Controllers)
- ⬜ Kết nối FE → BE thực tế (thay data mock)
- ⬜ Authentication & Authorization hoàn chỉnh
- ⬜ Quản lý state phức tạp (giỏ hàng, đơn hàng)
- ⬜ Upload ảnh, quản lý media
- ⬜ Phân trang, tìm kiếm nâng cao
- ⬜ Dashboard báo cáo
- ⬜ Tích hợp thanh toán
