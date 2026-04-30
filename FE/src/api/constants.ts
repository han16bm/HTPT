// ============================================
// API constants — khớp Plan/07_API_CONTRACTS.md
// URL pattern: /api/{service}/{controller}/{action}
// ============================================

export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  NO_CONTENT: 204,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  INTERNAL_SERVER_ERROR: 500,
  SERVICE_UNAVAILABLE: 503,
} as const;

export const REQUEST_TIMEOUT = 30000;

export const ERROR_MESSAGES = {
  NETWORK_ERROR: 'Lỗi kết nối mạng. Vui lòng kiểm tra lại internet.',
  TIMEOUT: 'Yêu cầu đã hết thời gian chờ. Vui lòng thử lại.',
  UNAUTHORIZED: 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.',
  FORBIDDEN: 'Bạn không có quyền truy cập tài nguyên này.',
  NOT_FOUND: 'Không tìm thấy tài nguyên yêu cầu.',
  SERVER_ERROR: 'Lỗi máy chủ. Vui lòng thử lại sau.',
  UNKNOWN_ERROR: 'Đã xảy ra lỗi không xác định.',
} as const;

export const API_ENDPOINTS = {
  // ===== Auth =====
  AUTH_LOGIN: '/user/auth/login',
  AUTH_LOGOUT: '/user/auth/logout',
  AUTH_REFRESH: '/user/auth/refresh-token',
  AUTH_ME: '/user/auth/me',

  // ===== Products =====
  PRODUCT_SEARCH: '/product/products',
  PRODUCT_DETAIL: '/product/products',
  PRODUCT_BY_SLUG: '/product/products/slug',
  PRODUCT_UPSERT: '/product/products',
  PRODUCT_DELETE: '/product/products',

  // ===== Categories =====
  CATEGORY_SEARCH: '/product/categories',
  CATEGORY_TREE: '/product/categories/tree',
  CATEGORY_UPSERT: '/product/categories',
  CATEGORY_DELETE: '/product/categories',

  // ===== Inventory =====
  INVENTORY_HISTORY: '/product/inventory/transactions',
  INVENTORY_IMPORT: '/product/inventory/imports',

  // ===== Customers (admin) =====
  CUSTOMER_SEARCH: '/user/customers',
  CUSTOMER_DETAIL: '/user/customers',
  CUSTOMER_UPSERT: '/user/customers',
  CUSTOMER_CREATE_WALKIN: '/user/customers/walk-ins',
  CUSTOMER_DELETE: '/user/customers',

  // ===== Orders (admin) =====
  ORDER_SEARCH: '/order/orders',
  ORDER_DETAIL: '/order/orders',
  ORDER_UPDATE_STATUS: '/order/orders',
  ORDER_CANCEL: '/order/orders',
  ORDER_CREATE: '/order/orders',
  ORDER_CREATE_DIRECT: '/order/orders/direct',

  // ===== Promotions =====
  PROMOTION_SEARCH: '/order/promotions',
  PROMOTION_UPSERT: '/order/promotions',
  PROMOTION_DELETE: '/order/promotions',
  PROMOTION_VERIFY: '/order/promotions/validate',

  // ===== Dashboard =====
  DASHBOARD_STATS: '/order/dashboard/stats',
  DASHBOARD_CHARTS: '/order/dashboard/charts',

  // ===== Reports =====
  REPORT_REVENUE: '/order/reports/revenue',
  REPORT_TOP_PRODUCTS: '/order/reports/top-products',
  REPORT_ORDER_SUMMARY: '/order/reports/order-summary',

  // ===== Blog =====
  BLOG_SEARCH: '/content/blogs',
  BLOG_DETAIL: '/content/blogs/slug',
  BLOG_UPSERT: '/content/blogs',
  BLOG_DELETE: '/content/blogs',
  BLOG_CATEGORIES: '/content/blog-categories',

  // ===== Contact =====
  CONTACT_SEARCH: '/content/contacts',
  CONTACT_UPDATE_STATUS: '/content/contacts',
} as const;
