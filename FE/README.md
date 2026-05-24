# Fish Store Management System - Frontend

Hệ thống quản lý cửa hàng cá cảnh - Frontend React + TypeScript + Vite

## 🚀 Công nghệ sử dụng

- **React 19** - Thư viện UI
- **TypeScript** - Type-safe JavaScript
- **Vite** - Build tool và dev server
- **Ant Design 5** - UI Component Library
- **React Router DOM 7** - Routing
- **Axios** - HTTP client
- **SCSS/SASS** - CSS preprocessor
- **React Toastify** - Notifications
- **Recharts** - Charts và biểu đồ

## 📁 Cấu trúc thư mục

```
FE/
├── public/              # Static assets
├── src/
│   ├── api/            # API services và axios configuration
│   ├── assets/         # Images, fonts, etc.
│   ├── components/     # Reusable components
│   │   ├── common/    # Common components (ProtectedRoute, etc.)
│   │   └── Layout/    # Layout components (Header, Sidebar, MasterLayout)
│   ├── config/         # App configuration (theme, env)
│   ├── interfaces/     # TypeScript interfaces
│   ├── pages/          # Page components
│   │   ├── Login/
│   │   ├── Home/
│   │   ├── Products/
│   │   ├── Customers/
│   │   ├── Invoices/
│   │   ├── Sales/
│   │   └── Error/
│   ├── routes/         # Routing configuration
│   ├── shared/         # Shared utilities
│   │   └── utils/     # Utility functions và constants
│   ├── styles/         # Global styles
│   ├── App.tsx         # Root component
│   ├── main.tsx        # Entry point
│   └── index.scss      # Global styles
├── .env.development    # Development environment variables
├── .env.production     # Production environment variables
├── index.html          # HTML template
├── package.json        # Dependencies
├── tsconfig.json       # TypeScript config
├── vite.config.ts      # Vite config
└── README.md
```

## 🎯 Tính năng

### 1. Đăng nhập (Login)
- Xác thực người dùng
- JWT token authentication
- Auto-redirect khi đã đăng nhập

### 2. Trang chủ (Dashboard)
- Thống kê hóa đơn hôm nay
- Doanh thu và lợi nhuận
- Top sản phẩm bán chạy

### 3. Quản lý hàng hóa (Products)
- Danh sách sản phẩm
- Thêm/Sửa/Xóa sản phẩm
- Lọc theo danh mục (Cá cảnh, Phụ kiện, Thức ăn)
- Hiển thị tồn kho và số lượng đã bán

### 4. Quản lý khách hàng (Customers)
- Danh sách khách hàng
- Thêm/Sửa/Xóa khách hàng
- Tìm kiếm khách hàng
- Thông tin liên hệ đầy đủ

### 5. Quản lý hóa đơn (Invoices)
- Danh sách hóa đơn
- Xem chi tiết hóa đơn
- Thống kê theo ngày

### 6. Bán hàng (Sales)
- Giao diện bán hàng trực quan
- Tìm kiếm và chọn sản phẩm
- Quản lý giỏ hàng
- Chọn khách hàng
- Thanh toán và tạo hóa đơn

## 🔧 Cài đặt

### Yêu cầu
- Node.js >= 18
- npm hoặc yarn

### Các bước cài đặt

1. **Cài đặt dependencies:**
```bash
cd FE
npm install
```

2. **Cấu hình môi trường:**
Kiểm tra file `.env.development` và cập nhật URL API nếu cần:
```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_ENABLE_DEBUG=true
```

3. **Chạy development server:**
```bash
npm run dev
```

Ứng dụng sẽ chạy tại: http://localhost:3000

## 📦 Build

### Development Build
```bash
npm run build:dev
```

### Production Build
```bash
npm run build
```

Build output sẽ nằm trong thư mục `dist/`

### Preview Production Build
```bash
npm run preview
```

## 🔌 API Integration

API base URL được cấu hình trong `.env` files:
- Development: `http://localhost:5000/api`
- Production: Cập nhật trong `.env.production`

### API Endpoints

```typescript
// Auth
POST /auth/login           // Đăng nhập
POST /auth/logout          // Đăng xuất

// Products
GET    /products           // Lấy danh sách sản phẩm
GET    /products/:id       // Lấy chi tiết sản phẩm
POST   /products           // Tạo sản phẩm mới
PUT    /products/:id       // Cập nhật sản phẩm
DELETE /products/:id       // Xóa sản phẩm

// Customers
GET    /customers          // Lấy danh sách khách hàng
GET    /customers/:id      // Lấy chi tiết khách hàng
GET    /customers/search   // Tìm kiếm khách hàng
POST   /customers          // Tạo khách hàng mới
PUT    /customers/:id      // Cập nhật khách hàng
DELETE /customers/:id      // Xóa khách hàng

// Invoices
GET /invoices              // Lấy danh sách hóa đơn
GET /invoices/:id          // Lấy chi tiết hóa đơn

// Sales
POST /sales/create         // Tạo đơn bán hàng
GET  /sales/statistics     // Thống kê bán hàng
GET  /sales/top-products   // Top sản phẩm bán chạy
```

## 🎨 Theme Customization

Theme được cấu hình trong `src/config/theme.ts`:
```typescript
const antdTheme: ThemeConfig = {
  token: {
    colorPrimary: '#198754',  // Màu chính (xanh lá)
    colorSuccess: '#198754',
    borderRadius: 8,
  },
};
```

## 🔐 Authentication

- Token được lưu trong `localStorage`
- Auto-redirect khi token hết hạn
- Protected routes với `ProtectedRoute` component

## 🛠️ Development

### Linting
```bash
npm run lint
```

### Type Checking
```bash
npx tsc --noEmit
```

## 📝 Scripts

- `npm run dev` - Start dev server
- `npm run build` - Build production
- `npm run build:dev` - Build development
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## 🌐 Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## 📄 License

MIT

## 👥 Author

Fish Store Management Team

