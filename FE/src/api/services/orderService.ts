import axiosClient from '../axiosClient';
import { API_ENDPOINTS } from '../constants';
import type {
  Order,
  OrderCreateRequest,
  OrderItem,
  OrderSearchParams,
  OrderUpdateStatusRequest,
  PagedResult,
} from '@/interfaces';

type OrderApiItem = Partial<OrderItem> & {
  subTotal?: number;
};

type OrderApiModel = Partial<Order> & {
  notes?: string;
  source?: Order['orderSource'];
  subtotal?: number;
  items?: OrderApiItem[];
};

const normalizeOrderItem = (item: OrderApiItem): OrderItem => ({
  id: Number(item.id ?? 0),
  productId: Number(item.productId ?? 0),
  productName: item.productName ?? '',
  imageUrl: item.imageUrl,
  quantity: Number(item.quantity ?? 0),
  unitPrice: Number(item.unitPrice ?? 0),
  lineTotal: Number(item.lineTotal ?? item.subTotal ?? 0),
});

const normalizeOrder = (order: OrderApiModel): Order => ({
  id: Number(order.id ?? 0),
  orderCode: order.orderCode ?? '',
  customerId: order.customerId ?? null,
  customerName: order.customerName ?? '',
  customerPhone: order.customerPhone ?? '',
  customerAddress: order.customerAddress ?? order.shippingAddress ?? '',
  note: order.note ?? order.notes,
  orderStatus: order.orderStatus ?? 'PENDING',
  paymentMethod: order.paymentMethod ?? 'COD',
  paymentStatus: order.paymentStatus ?? 'PENDING',
  orderSource: order.orderSource ?? order.source ?? 'ONLINE',
  subtotalAmount: Number(order.subtotalAmount ?? order.subtotal ?? 0),
  discountAmount: Number(order.discountAmount ?? 0),
  shippingFee: Number(order.shippingFee ?? 0),
  totalAmount: Number(order.totalAmount ?? 0),
  items: (order.items ?? []).map(normalizeOrderItem),
  createdAt: order.createdAt ?? '',
  updatedAt: order.updatedAt,
});

const normalizePagedOrders = (result: PagedResult<OrderApiModel>): PagedResult<Order> => ({
  ...result,
  items: (result.items ?? []).map(normalizeOrder),
});

export const orderService = {
  search: async (params: OrderSearchParams = {}) => {
    const data = await axiosClient.get<PagedResult<OrderApiModel>>(API_ENDPOINTS.ORDER_SEARCH, { params });
    return normalizePagedOrders(data);
  },

  getByCode: async (orderCode: string) => {
    const data = await axiosClient.get<OrderApiModel>(API_ENDPOINTS.ORDER_DETAIL, {
      params: { order_code: orderCode },
    });

    return normalizeOrder(data);
  },

  updateStatus: (data: OrderUpdateStatusRequest) =>
    axiosClient.post<Order>(API_ENDPOINTS.ORDER_UPDATE_STATUS, data),

  cancel: (data: { orderCode: string; reason?: string }) =>
    axiosClient.post<Order>(API_ENDPOINTS.ORDER_CANCEL, data),

  create: (data: OrderCreateRequest) =>
    axiosClient.post<Order>(API_ENDPOINTS.ORDER_CREATE, data),

  createDirect: (data: OrderCreateRequest) =>
    axiosClient.post<Order>(API_ENDPOINTS.ORDER_CREATE_DIRECT, data),
};
