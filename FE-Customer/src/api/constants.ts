export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080/api';

export const API_ENDPOINTS = {
  // Auth
  LOGIN: '/auth/auth/dang-nhap',
  REGISTER: '/auth/auth/dang-ky',
  LOGOUT: '/auth/auth/dang-xuat',
  REFRESH_TOKEN: '/auth/auth/lam-moi-token',

  // Products
  PRODUCTS: '/products/products/tim-kiem',
  PRODUCT_DETAIL: (id: string | number) => `/products/products/chi-tiet?id=${id}`,
  PRODUCT_BY_SLUG: (slug: string) => `/products/products/theo-slug?slug=${encodeURIComponent(slug)}`,
  PRODUCT_SEARCH: '/products/products/tim-kiem',
  PRODUCT_FEATURED: '/products/products/san-pham-noi-bat',

  // Cart  (tất cả dùng POST theo BE design)
  CART: '/orders/cart/gio-hang-hien-tai',
  CART_ADD: '/orders/cart/them-san-pham',
  CART_UPDATE: '/orders/cart/cap-nhat-so-luong',   // POST {cartItemId, quantity}
  CART_REMOVE: '/orders/cart/xoa-san-pham',         // POST {cartItemId}
  CART_CLEAR: '/orders/cart/xoa-gio-hang',

  // Orders
  ORDERS: '/orders/orders/don-hang-cua-toi',
  ORDER_DETAIL: (code: string | number) => `/orders/orders/chi-tiet?order_code=${code}`,
  ORDER_CREATE: '/orders/orders/dat-hang',
  ORDER_CANCEL: '/orders/orders/huy-don',
  PROMOTION_SEARCH: '/orders/promotions/tim-kiem',
  PROMOTION_VERIFY: '/orders/promotions/kiem-tra-ma',

  // User Profile
  PROFILE: '/auth/auth/thong-tin-nguoi-dung',
  UPDATE_PROFILE: '/auth/auth/cap-nhat-thong-tin',
  CHANGE_PASSWORD: '/auth/auth/doi-mat-khau',

  // Categories
  CATEGORIES: '/products/categories/tim-kiem',

  // Blog and Content
  BLOG: '/content/blog/tim-kiem',
  BLOG_DETAIL: (slug: string) => `/content/blog/chi-tiet?slug=${slug}`,
  CONTACT_SEND: '/content/contact/gui-lien-he',
};
