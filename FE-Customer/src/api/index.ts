export { default as axiosClient } from './axiosClient';

// --- Types & services (real API) ---
export type { Product, ProductsResponse } from './productService';
export type { CartItem, Cart } from './cartService';
export type { LoginResponse, User } from './authService';
export type { Category } from './categoryService';
export type { BlogCategory, BlogPost, BlogSearchParams, BlogStatus } from './blogService';
export type {
  Promotion,
  PromotionDiscountType,
  PromotionSearchParams,
  PromotionValidation,
} from './promotionService';

// --- Real services connected to BE via Gateway ---
export { contactService } from './contactService';
export { productService } from './productService';
export { categoryService } from './categoryService';
export { cartService } from './cartService';
export { promotionService } from './promotionService';
export { blogService } from './blogService';
export * from './orderService';
export * from './authService';
export * from './constants';
