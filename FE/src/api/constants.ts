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
  AUTH_LOGIN: '/auth/auth/dang-nhap',
  AUTH_LOGOUT: '/auth/auth/dang-xuat',
  AUTH_REFRESH: '/auth/auth/lam-moi-token',
  AUTH_ME: '/auth/auth/thong-tin-nguoi-dung',

  // ===== Products =====
  PRODUCT_SEARCH: '/products/products/tim-kiem',
  PRODUCT_DETAIL: '/products/products/chi-tiet',
  PRODUCT_BY_SLUG: '/products/products/theo-slug',
  PRODUCT_UPSERT: '/products/products/them-moi-cap-nhat',
  PRODUCT_DELETE: '/products/products/xoa',

  // ===== Categories =====
  CATEGORY_SEARCH: '/products/categories/tim-kiem',
  CATEGORY_TREE: '/products/categories/cay-phan-cap',
  CATEGORY_UPSERT: '/products/categories/them-moi-cap-nhat',
  CATEGORY_DELETE: '/products/categories/xoa',

  // ===== Inventory =====
  INVENTORY_HISTORY: '/products/inventory/lich-su-giao-dich',
  INVENTORY_IMPORT: '/products/inventory/nhap-hang',

  // ===== Customers (admin) =====
  CUSTOMER_SEARCH: '/admin/customers/tim-kiem',
  CUSTOMER_DETAIL: '/admin/customers/chi-tiet',
  CUSTOMER_UPSERT: '/admin/customers/them-moi-cap-nhat',
  CUSTOMER_CREATE_WALKIN: '/admin/customers/tao-khach-vang-lai',
  CUSTOMER_DELETE: '/admin/customers/xoa',

  // ===== Orders (admin) =====
  ORDER_SEARCH: '/orders/orders/tim-kiem',
  ORDER_DETAIL: '/orders/orders/chi-tiet',
  ORDER_UPDATE_STATUS: '/orders/orders/cap-nhat-trang-thai',
  ORDER_CANCEL: '/orders/orders/huy-don',
  ORDER_CREATE: '/orders/orders/dat-hang',
  ORDER_CREATE_DIRECT: '/orders/orders/dat-hang-truc-tiep',

  // ===== Promotions =====
  PROMOTION_SEARCH: '/orders/promotions/tim-kiem',
  PROMOTION_UPSERT: '/orders/promotions/them-moi-cap-nhat',
  PROMOTION_DELETE: '/orders/promotions/xoa',
  PROMOTION_VERIFY: '/orders/promotions/kiem-tra-ma',

  // ===== Dashboard =====
  DASHBOARD_STATS: '/admin/dashboard/thong-ke-tong-quat',
  DASHBOARD_CHARTS: '/admin/dashboard/bieu-do',

  // ===== Reports =====
  REPORT_REVENUE: '/admin/reports/doanh-thu',
  REPORT_TOP_PRODUCTS: '/admin/reports/san-pham-ban-chay',
  REPORT_ORDER_SUMMARY: '/admin/reports/tong-hop-don-hang',

  // ===== Blog =====
  BLOG_SEARCH: '/content/blog/tim-kiem',
  BLOG_DETAIL: '/content/blog/chi-tiet-theo-slug',
  BLOG_UPSERT: '/content/blog/them-moi-cap-nhat',
  BLOG_DELETE: '/content/blog/xoa',
  BLOG_CATEGORIES: '/content/blog/danh-muc',

  // ===== Contact =====
  CONTACT_SEARCH: '/content/contact/tim-kiem',
  CONTACT_UPDATE_STATUS: '/content/contact/cap-nhat-trang-thai',
} as const;
