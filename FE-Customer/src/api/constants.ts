export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export const API_ENDPOINTS = {
  // Auth
  LOGIN: '/user/auth/login',
  REGISTER: '/user/auth/register',
  LOGOUT: '/user/auth/logout',
  REFRESH_TOKEN: '/user/auth/refresh-token',

  // Products
  PRODUCTS: '/product/products',
  PRODUCT_DETAIL: (id: string | number) => `/product/products/${id}`,
  PRODUCT_BY_SLUG: (slug: string) => `/product/products/slug/${encodeURIComponent(slug)}`,
  PRODUCT_SEARCH: '/product/products',
  PRODUCT_FEATURED: '/product/products/featured',

  // Cart
  CART: '/order/cart',
  CART_ADD: '/order/cart/items',
  CART_UPDATE: (cartItemId: string | number) => `/order/cart/items/${cartItemId}`,
  CART_REMOVE: (cartItemId: string | number) => `/order/cart/items/${cartItemId}`,
  CART_CLEAR: '/order/cart',

  // Orders
  ORDERS: '/order/orders/me',
  ORDER_DETAIL: (code: string | number) => `/order/orders/${encodeURIComponent(String(code))}`,
  ORDER_CREATE: '/order/orders',
  ORDER_CANCEL: (code: string | number) => `/order/orders/${encodeURIComponent(String(code))}`,
  PROMOTION_SEARCH: '/order/promotions',
  PROMOTION_VERIFY: '/order/promotions/validate',

  // User Profile
  PROFILE: '/user/auth/me',
  UPDATE_PROFILE: '/user/auth/me',
  CHANGE_PASSWORD: '/user/auth/password',

  // Categories
  CATEGORIES: '/product/categories',

  // Blog and Content
  BLOG: '/content/blogs',
  BLOG_DETAIL: (slug: string) => `/content/blogs/slug/${encodeURIComponent(slug)}`,
  CONTACT_SEND: '/content/contacts',
};
