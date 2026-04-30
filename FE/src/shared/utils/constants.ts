// ============================================
// Hằng số dùng chung của Admin FE
// ============================================

// Local Storage Keys
export const ACCESS_TOKEN_KEY = 'fs_access_token';
export const REFRESH_TOKEN_KEY = 'fs_refresh_token';
export const USER_INFO_KEY = 'fs_user_info';

// App
export const APP_NAME = 'H&H FISH SHOP — Quản trị';
export const APP_SHORT_NAME = 'H&H FISH SHOP';

// Date
export const DATE_FORMAT = 'DD/MM/YYYY';
export const DATETIME_FORMAT = 'DD/MM/YYYY HH:mm';

// Pagination
export const DEFAULT_PAGE_SIZE = 10;
export const PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

// ===== Order labels & colors =====
import type {
  OrderStatus,
  PaymentMethod,
  PaymentStatus,
  OrderSource,
  ContactStatus,
  BlogStatus,
  InventoryTransactionType,
  UserRole,
} from '@/interfaces';

export const ORDER_STATUS_LABEL: Record<OrderStatus, string> = {
  PENDING: 'Chờ xác nhận',
  CONFIRMED: 'Đã xác nhận',
  SHIPPING: 'Đang giao',
  COMPLETED: 'Hoàn thành',
  CANCELLED: 'Đã hủy',
};

export const ORDER_STATUS_COLOR: Record<OrderStatus, string> = {
  PENDING: 'gold',
  CONFIRMED: 'blue',
  SHIPPING: 'cyan',
  COMPLETED: 'green',
  CANCELLED: 'red',
};

export const ORDER_STATUS_FLOW: Record<OrderStatus, OrderStatus[]> = {
  PENDING: ['CONFIRMED', 'CANCELLED'],
  CONFIRMED: ['SHIPPING', 'CANCELLED'],
  SHIPPING: ['COMPLETED', 'CANCELLED'],
  COMPLETED: [],
  CANCELLED: [],
};

export const PAYMENT_METHOD_LABEL: Record<PaymentMethod, string> = {
  COD: 'Tiền mặt khi nhận hàng',
  BANK_TRANSFER: 'Chuyển khoản',
  CASH: 'Tiền mặt tại quầy',
};

export const PAYMENT_STATUS_LABEL: Record<PaymentStatus, string> = {
  PENDING: 'Chờ thanh toán',
  UNPAID: 'Chưa thanh toán',
  PARTIAL: 'Thanh toán 1 phần',
  PAID: 'Đã thanh toán',
  REFUNDED: 'Đã hoàn tiền',
};

export const PAYMENT_STATUS_COLOR: Record<PaymentStatus, string> = {
  PENDING: 'gold',
  UNPAID: 'red',
  PARTIAL: 'orange',
  PAID: 'green',
  REFUNDED: 'default',
};

export const ORDER_SOURCE_LABEL: Record<OrderSource, string> = {
  ONLINE: 'Online',
  POS: 'Tại quầy',
};

export const CONTACT_STATUS_LABEL: Record<ContactStatus, string> = {
  NEW: 'Mới',
  READ: 'Đã đọc',
  RESOLVED: 'Đã xử lý',
};

export const CONTACT_STATUS_COLOR: Record<ContactStatus, string> = {
  NEW: 'red',
  READ: 'blue',
  RESOLVED: 'green',
};

export const BLOG_STATUS_LABEL: Record<BlogStatus, string> = {
  DRAFT: 'Nháp',
  PUBLISHED: 'Đã đăng',
};

export const BLOG_STATUS_COLOR: Record<BlogStatus, string> = {
  DRAFT: 'default',
  PUBLISHED: 'green',
};

export const INVENTORY_TYPE_LABEL: Record<InventoryTransactionType, string> = {
  IMPORT: 'Nhập kho',
  EXPORT: 'Xuất kho',
  ADJUST: 'Điều chỉnh',
  ADJUSTMENT: 'Điều chỉnh',
  SALE: 'Bán hàng',
  RETURN: 'Trả hàng',
};

export const ROLE_LABEL: Record<UserRole, string> = {
  ADMIN: 'Quản trị viên',
  STAFF: 'Nhân viên',
  CUSTOMER: 'Khách hàng',
};

// Stock thresholds
export const LOW_STOCK_THRESHOLD = 10;
