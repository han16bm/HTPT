// ============================================
// FISH SHOP — Admin FE Type Definitions
// Khớp với BE contracts (Plan/07_API_CONTRACTS.md)
// ============================================

// ===== ENUMS / UNIONS =====
export type UserRole = 'ADMIN' | 'STAFF' | 'CUSTOMER';

export type OrderStatus =
  | 'PENDING'
  | 'CONFIRMED'
  | 'SHIPPING'
  | 'COMPLETED'
  | 'CANCELLED';

export type PaymentMethod = 'COD' | 'BANK_TRANSFER' | 'CASH';

export type PaymentStatus = 'PENDING' | 'UNPAID' | 'PARTIAL' | 'PAID' | 'REFUNDED';

export type OrderSource = 'ONLINE' | 'POS';

export type DiscountType = 'PERCENT' | 'AMOUNT';

export type ContactStatus = 'NEW' | 'READ' | 'RESOLVED';

export type BlogStatus = 'DRAFT' | 'PUBLISHED';

export type InventoryTransactionType =
  | 'IMPORT'
  | 'EXPORT'
  | 'ADJUST'
  | 'ADJUSTMENT'
  | 'SALE'
  | 'RETURN';

// ===== API ENVELOPES =====
export interface ApiResponse<T = unknown> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ===== AUTH =====
export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: User;
}

export interface User {
  id: number;
  username: string;
  email?: string;
  fullName?: string;
  role: UserRole;
  avatarUrl?: string | null;
}

// ===== CATEGORY =====
export interface Category {
  id: number;
  categoryCode: string;
  name: string;
  slug: string;
  parentId?: number | null;
  description?: string;
  imageUrl?: string;
  displayOrder: number;
  status: boolean;
  children?: Category[];
}

export interface CategoryUpsertRequest {
  id?: number;
  categoryCode?: string;
  name: string;
  slug?: string;
  parentId?: number | null;
  description?: string;
  imageUrl?: string;
  imageFile?: File;
  removeImage?: boolean;
  displayOrder?: number;
  status?: boolean;
}

// ===== PRODUCT =====
export interface ProductImage {
  id: number;
  imageUrl: string;
  altText?: string;
  isPrimary: boolean;
  displayOrder: number;
}

export interface Product {
  id: number;
  categoryId: number;
  categoryName?: string;
  productCode: string;
  sku?: string;
  name: string;
  slug: string;
  shortDescription?: string;
  description?: string;
  imageUrl?: string;
  images?: ProductImage[];
  costPrice: number;
  salePrice: number;
  stockQuantity: number;
  soldQuantity: number;
  weightGrams?: number;
  status: number;
  isFeatured: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface ProductUpsertRequest {
  id?: number;
  categoryId: number;
  name: string;
  shortDescription?: string;
  description?: string;
  imageUrl?: string;
  imageFile?: File;
  removeImage?: boolean;
  images?: ProductImageUpsertItem[];
  costPrice: number;
  salePrice: number;
  stockQuantity: number;
  weightGrams?: number;
  isFeatured?: boolean;
  status?: number;
}

export interface ProductImageUpsertItem {
  id?: number;
  imageUrl?: string;
  imageFile?: File;
  altText?: string;
  isPrimary?: boolean;
  displayOrder: number;
  remove?: boolean;
}

export interface ProductSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: number;
  isFeatured?: boolean;
  inStock?: boolean;
  minPrice?: number;
  maxPrice?: number;
  sort?: 'newest' | 'oldest' | 'price-asc' | 'price-desc' | 'sold';
}

// ===== CUSTOMER =====
export interface Customer {
  id: number;
  customerCode: string;
  username?: string;
  fullName: string;
  phone: string;
  email?: string;
  dateOfBirth?: string;
  gender?: string;
  isActive?: boolean;
  isAdmin?: boolean;
  status?: number;
  createdAt: string;
}

export interface CustomerUpsertRequest {
  id?: number;
  fullName: string;
  phone: string;
  email?: string;
  dateOfBirth?: string;
  gender?: string;
  status?: number;
  isAdmin?: boolean;
}

// ===== ORDER =====
export interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  imageUrl?: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Order {
  id: number;
  orderCode: string;
  customerId?: number | null;
  customerName: string;
  customerPhone: string;
  customerAddress: string;
  note?: string;
  orderStatus: OrderStatus;
  paymentMethod: PaymentMethod;
  paymentStatus: PaymentStatus;
  orderSource: OrderSource;
  subtotalAmount: number;
  discountAmount: number;
  shippingFee: number;
  totalAmount: number;
  items: OrderItem[];
  createdAt: string;
  updatedAt?: string;
}

export interface OrderSearchParams {
  page?: number;
  pageSize?: number;
  status?: OrderStatus;
  source?: OrderSource;
  fromDate?: string;
  toDate?: string;
  keyword?: string;
}

export interface OrderCreateRequest {
  customerId?: number | null;
  customerName: string;
  customerPhone: string;
  customerAddress: string;
  note?: string;
  paymentMethod: PaymentMethod;
  source: OrderSource;
  promoCode?: string;
  items: Array<{
    productId: number;
    quantity: number;
    unitPrice: number;
  }>;
}

export interface OrderUpdateStatusRequest {
  orderCode: string;
  status: OrderStatus;
  note?: string;
}

// ===== DASHBOARD =====
export interface DashboardStats {
  todayOrders: number;
  todayRevenue: number;
  todayProfit: number;
  totalOrders: number;
  totalRevenue: number;
  totalCustomers: number;
  lowStockCount: number;
  pendingOrders: number;
}

export interface DashboardCharts {
  revenueChart: Array<{ date: string; revenue: number; orders: number }>;
  topProducts: Array<{ productName: string; sold: number }>;
  orderStatusChart: Array<{ status: OrderStatus; count: number }>;
}

// ===== INVENTORY =====
export interface InventoryTransaction {
  id: number;
  productId: number;
  productName: string;
  transactionType: InventoryTransactionType;
  quantity: number;
  unitCost?: number;
  note?: string;
  createdBy: string;
  createdAt: string;
}

export interface InventoryImportRequest {
  productId: number;
  quantity: number;
  unitCost: number;
  note?: string;
}

// ===== PROMOTION =====
export interface Promotion {
  id: number;
  promoCode: string;
  title: string;
  description?: string;
  discountType: DiscountType;
  discountValue: number;
  maxDiscountValue?: number;
  minOrderValue: number;
  startAt: string;
  endAt: string;
  usageLimit?: number;
  usedCount: number;
  status: number;
}

export interface PromotionUpsertRequest {
  id?: number;
  promoCode: string;
  title: string;
  description?: string;
  discountType: DiscountType;
  discountValue: number;
  maxDiscountValue?: number;
  minOrderValue: number;
  startAt: string;
  endAt: string;
  usageLimit?: number;
  status?: number;
}

// ===== BLOG =====
export interface BlogCategory {
  id: number;
  name: string;
  slug: string;
}

export interface BlogPost {
  id: number;
  title: string;
  slug: string;
  summary?: string;
  content: string;
  thumbnailUrl?: string;
  categoryId?: number;
  categoryName?: string;
  status: BlogStatus;
  authorName?: string;
  publishedAt?: string;
  createdAt: string;
}

export interface BlogPostUpsertRequest {
  id?: number;
  title: string;
  summary?: string;
  content: string;
  thumbnailUrl?: string;
  thumbnailFile?: File;
  removeThumbnail?: boolean;
  categoryId?: number;
  status: BlogStatus;
}

// ===== CONTACT =====
export interface Contact {
  id: number;
  fullName: string;
  email: string;
  phone: string;
  subject: string;
  message: string;
  status: ContactStatus;
  createdAt: string;
}

// ===== REPORTS =====
export interface RevenueReportRow {
  period: string;
  revenue: number;
  orders: number;
  profit?: number;
}

export interface TopProductReportRow {
  productId: number;
  productName: string;
  sold: number;
  revenue: number;
}

export interface OrderSummaryReport {
  totalOrders: number;
  byStatus: Array<{ status: OrderStatus; count: number; revenue: number }>;
}
