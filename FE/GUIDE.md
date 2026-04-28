# Tài liệu Hướng dẫn - Frontend Quản Lý Cửa Hàng Cá Cảnh

## 📚 Tổng quan dự án

Dự án frontend được xây dựng dựa trên cấu trúc của DMZ-Zone portal-ui, sử dụng React + TypeScript + Vite với Ant Design.

## 🏗️ Kiến trúc dự án

### 1. Cấu trúc thư mục chính

```
FE/
├── src/
│   ├── api/              # API layer
│   ├── components/       # Components tái sử dụng
│   ├── config/          # Cấu hình ứng dụng
│   ├── interfaces/      # TypeScript interfaces
│   ├── pages/           # Các trang chính
│   ├── routes/          # Routing configuration
│   └── shared/          # Utilities và constants
```

### 2. API Layer (`src/api/`)

#### axiosClient.ts
- Singleton axios instance với interceptors
- Xử lý authentication tự động
- Error handling tập trung
- Toast notifications cho lỗi

#### apiService.ts
- Service functions cho các module:
  - `authService` - Đăng nhập/Đăng xuất
  - `productService` - CRUD sản phẩm
  - `customerService` - CRUD khách hàng
  - `invoiceService` - Quản lý hóa đơn
  - `salesService` - Bán hàng và thống kê

#### constants.ts
- HTTP status codes
- API endpoints
- Error messages

### 3. Components

#### Layout Components
- **MasterLayout**: Layout chính với Header, Sidebar, Content
- **Header**: Navigation bar với user info và logout
- **Sidebar**: Menu điều hướng với icons

#### Common Components
- **ProtectedRoute**: HOC bảo vệ routes yêu cầu authentication

### 4. Pages

#### Login (`pages/Login/`)
- Form đăng nhập với validation
- JWT token authentication
- Auto-redirect sau khi đăng nhập

#### Home (`pages/Home/`)
- Dashboard với thống kê:
  - Số hóa đơn hôm nay
  - Doanh thu
  - Lợi nhuận
  - Top sản phẩm bán chạy

#### Products (`pages/Products/`)
- Table danh sách sản phẩm
- CRUD operations
- Filter theo category
- Modal form thêm/sửa

#### Customers (`pages/Customers/`)
- Table danh sách khách hàng
- CRUD operations
- Search functionality
- Modal form thêm/sửa

#### Invoices (`pages/Invoices/`)
- Table danh sách hóa đơn
- Expandable rows hiển thị chi tiết items
- Hiển thị thông tin khách hàng

#### Sales (`pages/Sales/`)
- Giao diện 2 cột:
  - Trái: Danh sách sản phẩm (grid)
  - Phải: Giỏ hàng và thanh toán
- Chọn khách hàng với autocomplete
- Quản lý số lượng trong giỏ
- Tính tổng tiền tự động

### 5. Routing (`src/routes/`)

Routes được cấu hình theo pattern của DMZ-Zone:
```typescript
- /login (public)
- / (protected) - Home
- /products (protected)
- /customers (protected)
- /invoices (protected)
- /sales (protected)
- /404 (public)
```

### 6. Configuration (`src/config/`)

#### theme.ts
- Ant Design theme customization
- Primary color: #198754 (green)
- Component-specific styles

#### env.ts
- Environment variables management
- API base URL configuration
- Debug mode settings

### 7. Interfaces (`src/interfaces/`)

TypeScript interfaces cho:
- User, LoginRequest, LoginResponse
- Product, ProductCategory
- Customer
- Invoice, InvoiceItem
- SaleRequest
- SalesStatistics

## 🎨 Styling

### SCSS Modules
Mỗi component/page có file `.module.scss` riêng:
- Scoped styles
- Tránh conflicts
- Type-safe với classnames/bind

### Global Styles
- `index.scss`: Global styles, scrollbar, Ant Design overrides
- Color scheme: Green (#198754) làm màu chủ đạo

## 🔐 Authentication Flow

1. User nhập credentials tại `/login`
2. Call API `/auth/login`
3. Lưu token vào `localStorage`
4. Redirect về `/`
5. Protected routes check token
6. Nếu không có token → redirect về `/login`
7. Token gửi tự động trong headers của mọi request

## 🔄 Data Flow

```
Component → Service → AxiosClient → API Backend
                ↓
         Update State
                ↓
         Re-render UI
```

## 📱 Responsive Design

- Grid layouts với breakpoints
- Ant Design responsive components
- Mobile-friendly tables và forms

## 🚀 Cách chạy dự án

### Development
```bash
cd FE
npm install
npm run dev
```

### Production Build
```bash
npm run build
npm run preview
```

## 🔧 Customization

### Thêm trang mới
1. Tạo folder trong `src/pages/`
2. Tạo component và styles
3. Thêm route trong `src/routes/Routes.tsx`
4. Thêm menu item trong `Sidebar.tsx`

### Thêm API service
1. Thêm endpoint trong `api/constants.ts`
2. Thêm interface trong `interfaces/index.ts`
3. Tạo service function trong `api/apiService.ts`
4. Sử dụng trong component

### Thay đổi theme
Sửa file `src/config/theme.ts`:
```typescript
colorPrimary: '#your-color',
```

## 📦 Dependencies chính

```json
{
  "antd": "^5.28.0",           // UI Library
  "react": "^19.2.1",          // Core
  "react-router-dom": "^7.9.5", // Routing
  "axios": "^1.13.2",          // HTTP client
  "dayjs": "^1.11.10",         // Date formatting
  "sass": "^1.93.3",           // SCSS support
  "typescript": "~5.9.3"       // Type safety
}
```

## 🎯 Best Practices được áp dụng

1. **TypeScript**: Strong typing cho tất cả components và services
2. **Component Composition**: Tách nhỏ components, tái sử dụng
3. **Service Layer**: Tách biệt logic API khỏi UI
4. **Protected Routes**: Authentication và authorization
5. **Error Handling**: Centralized error handling với toast notifications
6. **Loading States**: Loading indicators cho async operations
7. **SCSS Modules**: Scoped styling, tránh conflicts
8. **Environment Config**: Separate dev và prod configs

## 🔍 Debugging

### Enable Debug Mode
Trong `.env.development`:
```env
VITE_ENABLE_DEBUG=true
```

### Console Logs
- API requests/responses
- Error details
- State changes

## 📝 Notes

- Backend API phải chạy tại `http://localhost:8080/api`
- Cập nhật API URL trong `.env` nếu khác
- Token được lưu trong localStorage
- Clear localStorage nếu gặp vấn đề authentication

## 🤝 Integration với Backend

Backend cần implement các endpoints trong `api/constants.ts` với format:

```typescript
{
  success: boolean,
  data: T,
  message?: string,
  errors?: string[]
}
```

## 📚 Tài liệu tham khảo

- [React Documentation](https://react.dev)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [Ant Design](https://ant.design/components/overview)
- [Vite Guide](https://vitejs.dev/guide/)
- [React Router](https://reactrouter.com)
