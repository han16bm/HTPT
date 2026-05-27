import axiosClient, { ApiResponse } from './axiosClient';
import { API_ENDPOINTS } from './constants';

// ============================================================
// Khớp với BE OrderListDto (dùng trong danh sách đơn hàng)
// ============================================================
export interface Order {
  id: number;
  orderCode: string;
  customerName: string;
  customerPhone?: string;
  orderStatus: string;       // PENDING | CONFIRMED | SHIPPING | COMPLETED | CANCELLED
  paymentMethod: string;     // COD | BANK_TRANSFER | CASH
  paymentStatus: string;     // UNPAID | PENDING | AWAITING | PAID | FAILED
  totalAmount: number;
  itemCount?: number;
  source?: string;
  createdAt: string;
  // Chi tiết (chỉ có trong OrderDto, không có trong OrderListDto)
  subtotal?: number;
  discountAmount?: number;
  shippingFee?: number;
  shippingAddress?: string;
  notes?: string;
  promotionCode?: string;
  updatedAt?: string;
  items?: OrderItem[];
}

// Khớp với BE OrderItemDto
export interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  imageUrl?: string;
  quantity: number;
  unitPrice: number;
  subTotal: number;    // BE trả 'subTotal' (không phải totalPrice)
}

// Wrapper cho PagedResult<T> mà BE trả
interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * CreateOrderRequest — khớp với BE API.Order.Models.Commands.CreateOrderRequest
 * BE tự lấy items từ Cart của user (CreateOrderFromCartAsync)
 */
export interface CreateOrderRequest {
  customerName?: string;
  customerPhone?: string;
  customerEmail?: string;
  shippingAddress?: string;
  paymentMethod: 'COD' | 'BANK_TRANSFER' | 'CASH';
  promotionCode?: string;
  shippingFee?: number;
  note?: string;
}

export interface CancelOrderRequest {
  orderCode: string;   // BE nhận orderCode (không phải orderId)
  reason?: string;
}

export const orderService = {
  /**
   * GET /api/order/orders/me
   * BE trả PagedResult<OrderListDto> — ta unwrap ra mảng items
   */
  getOrders: async (): Promise<ApiResponse<Order[]>> => {
    const res = await axiosClient.get<PagedResult<Order>>(API_ENDPOINTS.ORDERS);
    if (res.success && res.data) {
      // Unwrap PagedResult -> mảng đơn hàng
      const pagedData = res.data as unknown as PagedResult<Order>;
      return {
        success: true,
        data: Array.isArray(pagedData.items) ? pagedData.items : (res.data as unknown as Order[]),
      };
    }
    return res as unknown as ApiResponse<Order[]>;
  },

  /** GET /api/order/orders/DH2024001 */
  getOrderByCode: (code: string | number): Promise<ApiResponse<Order>> => {
    return axiosClient.get(API_ENDPOINTS.ORDER_DETAIL(code));
  },

  /**
   * POST /api/order/orders
   * BE tự lấy items từ Cart, chỉ cần địa chỉ + phương thức thanh toán
   */
  createOrder: (data: CreateOrderRequest): Promise<ApiResponse<Order>> => {
    return axiosClient.post(API_ENDPOINTS.ORDER_CREATE, data);
  },

  /** DELETE /api/order/orders/{orderCode} */
  cancelOrder: (data: CancelOrderRequest): Promise<ApiResponse<Order>> => {
    return axiosClient.delete(API_ENDPOINTS.ORDER_CANCEL(data.orderCode), {
      params: { reason: data.reason },
    });
  },
};
