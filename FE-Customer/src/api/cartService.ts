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
  /** GET /api/order/cart */
  getCart: (): Promise<ApiResponse<Cart>> => {
    return axiosClient.get(API_ENDPOINTS.CART);
  },

  /** POST /api/order/cart/items */
  addToCart: (
    productId: number,
    quantity: number,
    productName: string,
    unitPrice: number,
    imageUrl?: string,
  ): Promise<ApiResponse<Cart>> => {
    return axiosClient.post(API_ENDPOINTS.CART_ADD, {
      productId,
      quantity,
      productName,
      unitPrice,
      imageUrl,
    });
  },

  /** PUT /api/order/cart/items/{cartItemId} */
  updateQuantity: (cartItemId: number, quantity: number): Promise<ApiResponse<Cart>> => {
    return axiosClient.put(API_ENDPOINTS.CART_UPDATE(cartItemId), { quantity });
  },

  /** DELETE /api/order/cart/items/{cartItemId} */
  removeItem: (cartItemId: number): Promise<ApiResponse<Cart>> => {
    return axiosClient.delete(API_ENDPOINTS.CART_REMOVE(cartItemId));
  },
};
