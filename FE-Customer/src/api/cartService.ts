import axiosClient, { ApiResponse } from './axiosClient';
import { API_ENDPOINTS } from './constants';

export interface CartItem {
  id: number;
  productId: number;
  productName: string;
  productSlug?: string;
  unitPrice: number;
  quantity: number;
  subTotal: number;
  stockQuantity?: number;
  imageUrl?: string;
}

export interface Cart {
  id?: number;
  userId?: number;
  items: CartItem[];
  totalPrice: number;
  totalItems: number;
}

export const cartService = {
  /** GET /api/orders/cart/gio-hang-hien-tai */
  getCart: (): Promise<ApiResponse<Cart>> => {
    return axiosClient.get(API_ENDPOINTS.CART);
  },

  /** POST /api/orders/cart/them-san-pham */
  addToCart: (productId: number, quantity: number): Promise<ApiResponse<Cart>> => {
    return axiosClient.post(API_ENDPOINTS.CART_ADD, {
      productId,
      quantity,
    });
  },

  /** POST /api/orders/cart/cap-nhat-so-luong  (BE dùng POST, không phải PUT) */
  updateQuantity: (cartItemId: number, quantity: number): Promise<ApiResponse<Cart>> => {
    return axiosClient.post(API_ENDPOINTS.CART_UPDATE, { cartItemId, quantity });
  },

  /** POST /api/orders/cart/xoa-san-pham  (BE dùng POST, không phải DELETE) */
  removeItem: (cartItemId: number): Promise<ApiResponse<Cart>> => {
    return axiosClient.post(API_ENDPOINTS.CART_REMOVE, { cartItemId });
  },
};
