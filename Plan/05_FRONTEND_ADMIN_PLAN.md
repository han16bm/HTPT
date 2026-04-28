# 🖥️ Kế Hoạch Frontend Admin — FISH SHOP

> React 19 + Vite + TypeScript + Ant Design 5 · Port: 5174

---

## Tổng Quan

FE Admin (thư mục `/FE/`) là giao diện dành cho **Admin** và **Staff** của cửa hàng:
- Admin: toàn quyền truy cập tất cả chức năng
- Staff: giới hạn ở bán hàng POS, xem đơn, quản lý sản phẩm

---

## Cấu Trúc Hiện Tại (Đã Có)

```
✅ Có sẵn — cần refactor/mở rộng:
├── Login         (đã có)
├── Home          (Dashboard — đã có, cần mở rộng)
├── Products      (CRUD — đã có)
├── Customers     (CRUD — đã có)
├── Invoices      (xem list — đã có, đổi thành Orders)
└── Sales         (POS — đã có)

🆕 Cần thêm mới:
├── Categories    (quản lý danh mục sản phẩm)
├── Inventory     (nhập/xuất kho)
├── Promotions    (quản lý mã giảm giá)
├── Blog          (quản lý bài viết)
├── Contacts      (xem liên hệ khách hàng)
└── Reports       (báo cáo doanh thu, biểu đồ)
```

---

## Theme & Design System

```typescript
// src/config/theme.ts
export const adminTheme: ThemeConfig = {
  token: {
    colorPrimary: '#198754',     // Xanh lá — màu chủ đạo
    colorError: '#dc3545',
    colorWarning: '#ffc107',
    colorInfo: '#0dcaf0',
    colorSuccess: '#198754',
    borderRadius: 6,
    fontFamily: "'Inter', 'Segoe UI', sans-serif",
    fontSize: 14,
  },
  components: {
    Layout: {
      siderBg: '#001529',       // Sidebar tối
      headerBg: '#ffffff',      // Header trắng
    },
    Menu: {
      darkItemBg: '#001529',
      darkItemHoverBg: '#198754',
      darkItemSelectedBg: '#198754',
    },
    Table: {
      headerBg: '#f8f9fa',
    },
  }
};
```

---

## API Layer — Mở Rộng

### Cập nhật `src/api/constants.ts`
```typescript
export const API_ENDPOINTS = {
  // Auth
  LOGIN: '/auth/login',
  LOGOUT: '/auth/logout',
  REFRESH_TOKEN: '/auth/refresh',

  // Dashboard
  DASHBOARD_STATS: '/dashboard/stats',
  DASHBOARD_CHARTS: '/dashboard/charts',

  // Products
  PRODUCTS: '/products',
  PRODUCT_BY_ID: (id: number | string) => `/products/${id}`,
  PRODUCTS_BY_CATEGORY: (catId: number) => `/products/category/${catId}`,
  PRODUCTS_LOW_STOCK: '/products/low-stock',

  // Categories
  CATEGORIES: '/categories',
  CATEGORY_BY_ID: (id: number | string) => `/categories/${id}`,

  // Customers
  CUSTOMERS: '/customers',
  CUSTOMER_BY_ID: (id: number | string) => `/customers/${id}`,
  CUSTOMER_ORDERS: (id: number | string) => `/customers/${id}/orders`,

  // Orders (thay Invoices)
  ORDERS: '/orders',
  ORDER_BY_CODE: (code: string) => `/orders/${code}`,
  ORDER_UPDATE_STATUS: (code: string) => `/orders/${code}/status`,

  // Sales (POS)
  CREATE_SALE: '/sales/pos',
  SALES_STATISTICS: '/sales/statistics',
  TOP_PRODUCTS: '/sales/top-products',

  // Inventory
  INVENTORY: '/inventory',
  INVENTORY_IMPORT: '/inventory/import',

  // Promotions
  PROMOTIONS: '/promotions',
  PROMOTION_BY_ID: (id: number | string) => `/promotions/${id}`,

  // Blog
  BLOG_POSTS: '/blog',
  BLOG_POST_BY_ID: (id: number | string) => `/blog/${id}`,
  BLOG_CATEGORIES: '/blog/categories',

  // Contact
  CONTACTS: '/contact',
  CONTACT_UPDATE_STATUS: (id: number | string) => `/contact/${id}/status`,

  // Reports
  REPORT_REVENUE: '/reports/revenue',
  REPORT_TOP_PRODUCTS: '/reports/top-products',
  REPORT_ORDER_SUMMARY: '/reports/orders',
} as const;
```

### Cập nhật `src/interfaces/index.ts`
```typescript
// ===== AUTH =====
export interface LoginRequest { username: string; password: string; }
export interface LoginResponse { accessToken: string; refreshToken: string; user: User; }
export interface User {
  id: number; username: string; email: string;
  fullName?: string; role: 'ADMIN' | 'STAFF' | 'CUSTOMER'; avatarUrl?: string;
}

// ===== CATEGORIES =====
export interface Category {
  id: number; categoryCode: string; name: string; slug: string;
  parentId?: number; description?: string; imageUrl?: string;
  displayOrder: number; status: number;
}

// ===== PRODUCTS =====
export interface Product {
  id: number; categoryId: number; categoryName?: string;
  productCode: string; sku?: string; name: string; slug: string;
  shortDescription?: string; description?: string;
  imageUrl?: string; images?: ProductImage[];
  costPrice: number; salePrice: number;
  stockQuantity: number; soldQuantity: number;
  weightGrams?: number; status: number; isFeatured: boolean;
  createdAt: string; updatedAt: string;
}
export interface ProductImage { id: number; imageUrl: string; altText?: string; isPrimary: boolean; displayOrder: number; }

// ===== CUSTOMERS =====
export interface Customer {
  id: number; customerCode: string; fullName: string;
  phone: string; email?: string; dateOfBirth?: string;
  gender?: string; status: number; createdAt: string;
}

// ===== ORDERS =====
export type OrderStatus = 'PENDING' | 'CONFIRMED' | 'SHIPPING' | 'COMPLETED' | 'CANCELLED';
export type PaymentMethod = 'COD' | 'BANK_TRANSFER' | 'CASH';
export type PaymentStatus = 'UNPAID' | 'PARTIAL' | 'PAID' | 'REFUNDED';
export interface Order {
  id: number; orderCode: string; customerId?: number; customerName: string;
  customerPhone: string; customerAddress: string; note?: string;
  orderStatus: OrderStatus; paymentMethod: PaymentMethod; paymentStatus: PaymentStatus;
  orderSource: 'ONLINE' | 'POS';
  subtotalAmount: number; discountAmount: number; shippingFee: number; totalAmount: number;
  items: OrderItem[];
  createdAt: string;
}
export interface OrderItem {
  id: number; productId: number; productName: string;
  quantity: number; unitPrice: number; lineTotal: number;
}

// ===== DASHBOARD =====
export interface DashboardStats {
  todayOrders: number; todayRevenue: number; todayProfit: number;
  totalOrders: number; totalRevenue: number; totalCustomers: number;
  lowStockCount: number;
}

// ===== INVENTORY =====
export interface InventoryTransaction {
  id: number; productId: number; productName: string;
  transactionType: string; quantity: number; unitCost?: number;
  note?: string; createdBy: string; createdAt: string;
}

// ===== PROMOTIONS =====
export interface Promotion {
  id: number; promoCode: string; title: string; description?: string;
  discountType: 'PERCENT' | 'AMOUNT'; discountValue: number;
  maxDiscountValue?: number; minOrderValue: number;
  startAt: string; endAt: string; usageLimit?: number;
  usedCount: number; status: number;
}

// ===== PAGINATION =====
export interface PagedResult<T> {
  items: T[]; totalCount: number; page: number; pageSize: number; totalPages: number;
}

// ===== API RESPONSE =====
export interface ApiResponse<T> {
  success: boolean; data?: T; message?: string; errors?: string[];
}
```

---

## Các Trang Cần Phát Triển

### 1. Home — Dashboard
**Route:** `/` | **Role:** Admin, Staff

**Hiển thị:**
- Cards thống kê: Đơn hôm nay, Doanh thu, Lợi nhuận, Khách hàng
- Biểu đồ doanh thu 7 ngày (Line Chart — Ant Design Charts / Recharts)
- Biểu đồ top 5 sản phẩm bán chạy (Bar Chart)
- Bảng 5 đơn hàng mới nhất
- Cảnh báo hàng sắp hết

**APIs:** `GET /dashboard/stats`, `GET /dashboard/charts`

---

### 2. Products — Quản Lý Sản Phẩm
**Route:** `/products` | **Role:** Admin, Staff

**Hiện có:** Table, CRUD, filter theo category
**Cần thêm:**
- Upload ảnh sản phẩm (upload tới backend)
- Rich text editor cho Description (có thể dùng React Quill)
- Hiển thị tồn kho với màu cảnh báo
- Toggle Featured product

---

### 3. Categories — Quản Lý Danh Mục [MỚI]
**Route:** `/categories` | **Role:** Admin

**Layout:** Table + Modal thêm/sửa

**Fields:** Tên, Code, Slug (auto-gen), Mô tả, Ảnh, Danh mục cha, Thứ tự, Trạng thái

---

### 4. Orders — Quản Lý Đơn Hàng [MỞ RỘNG TỪ Invoices]
**Route:** `/orders` | **Role:** Admin, Staff

**Hiển thị:**
- Filter: Trạng thái đơn, Nguồn (Online/POS), Thời gian, Khách hàng
- Table: Mã đơn, Khách hàng, Tổng tiền, Trạng thái, Ngày tạo
- Expandable row: Chi tiết sản phẩm
- Actions: Cập nhật trạng thái, Xem chi tiết

**Trạng thái flow:**
```
PENDING → CONFIRMED → SHIPPING → COMPLETED
                              ↘ CANCELLED (từ bất kỳ giai đoạn)
```

---

### 5. Sales — POS Bán Hàng Tại Quầy
**Route:** `/sales` | **Role:** Admin, Staff

**Layout hiện có tốt**, cần kết nối API thực:
- Chọn sản phẩm → Giỏ hàng → Thanh toán
- Chọn khách hàng (search autocomplete)
- Áp mã giảm giá
- In hóa đơn (window.print hoặc PDF)

---

### 6. Customers — Quản Lý Khách Hàng
**Route:** `/customers` | **Role:** Admin, Staff

**Mở rộng thêm:** Tab lịch sử đơn hàng của khách

---

### 7. Inventory — Quản Lý Kho [MỚI]
**Route:** `/inventory` | **Role:** Admin

**Hiển thị:**
- Danh sách sản phẩm + tồn kho hiện tại
- Lịch sử nhập/xuất kho
- Modal nhập hàng (chọn SP, số lượng, giá nhập, ghi chú)

---

### 8. Promotions — Quản Lý Khuyến Mãi [MỚI]
**Route:** `/promotions` | **Role:** Admin

**Fields:** Mã, Tên, Loại giảm (% / tiền), Giá trị, Điều kiện, Thời gian, Giới hạn

---

### 9. Blog — Quản Lý Bài Viết [MỚI]
**Route:** `/blog` | **Role:** Admin

**CRUD với:** Rich text editor, Thumbnail, Danh mục blog, Publish/Draft

---

### 10. Contacts — Liên Hệ Khách Hàng [MỚI]
**Route:** `/contacts` | **Role:** Admin, Staff

**Danh sách:** Tên, Điện thoại, Chủ đề, Thời gian, Trạng thái
**Actions:** Xem nội dung, Đổi trạng thái (New → Read → Resolved)

---

### 11. Reports — Báo Cáo [MỚI]
**Route:** `/reports` | **Role:** Admin

**Tabs:**
- Doanh thu theo ngày/tuần/tháng (Line Chart)
- Top sản phẩm bán chạy (Bar Chart)
- Thống kê đơn hàng theo trạng thái (Pie Chart)
- Export Excel (tùy chọn)

---

## Sidebar Navigation (Cập Nhật)

```typescript
const menuItems = [
  { key: '/', icon: <DashboardOutlined />, label: 'Dashboard' },
  {
    key: 'catalog',
    icon: <AppstoreOutlined />,
    label: 'Danh mục',
    children: [
      { key: '/products', label: 'Sản phẩm' },
      { key: '/categories', label: 'Danh mục SP' },
      { key: '/inventory', label: 'Quản lý kho' },
    ]
  },
  {
    key: 'sales',
    icon: <ShoppingCartOutlined />,
    label: 'Bán hàng',
    children: [
      { key: '/sales', label: 'Bán tại quầy (POS)' },
      { key: '/orders', label: 'Đơn hàng' },
      { key: '/promotions', label: 'Khuyến mãi' },
    ]
  },
  { key: '/customers', icon: <TeamOutlined />, label: 'Khách hàng' },
  {
    key: 'content',
    icon: <FileTextOutlined />,
    label: 'Nội dung',
    children: [
      { key: '/blog', label: 'Bài viết blog' },
      { key: '/contacts', label: 'Liên hệ' },
    ]
  },
  { key: '/reports', icon: <BarChartOutlined />, label: 'Báo cáo' },
];
```

---

## Dependencies Cần Thêm

```json
{
  "recharts": "^2.x",           // hoặc @ant-design/charts
  "react-quill": "^2.x",        // Rich text editor
  "@ant-design/icons": "^5.x",  // Icon set (đã có)
  "file-saver": "^2.x",         // Export file
  "xlsx": "^0.x"                // Export Excel (tùy chọn)
}
```

---

## Thứ Tự Implement FE Admin

```
Phase 1 — Kết Nối API Thực Tế (song song với BE)
  [ ] Cập nhật axiosClient.ts (base URL → gateway)
  [ ] Cập nhật tất cả interfaces
  [ ] Cập nhật apiService.ts theo endpoints mới
  [ ] Implement useAuth hook + token refresh

Phase 2 — Trang Đã Có → Kết Nối API
  [ ] Login → gọi POST /auth/login
  [ ] Products → gọi CRUD thực
  [ ] Customers → gọi CRUD thực
  [ ] Sales (POS) → gọi POST /sales/pos
  [ ] Orders (mở rộng Invoices)

Phase 3 — Trang Mới
  [ ] Categories
  [ ] Inventory
  [ ] Promotions
  [ ] Blog
  [ ] Contacts

Phase 4 — Dashboard & Reports
  [ ] Tích hợp biểu đồ (Recharts)
  [ ] Dashboard thống kê thực
  [ ] Reports page
```
