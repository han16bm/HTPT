import axiosClient from '../../axiosClient';
import { API_ENDPOINTS } from '../../constants';

export interface DashboardOverview {
  today: {
    newOrders: number;
    revenue: number;
    profit: number;
    newCustomers: number;
  };
  month: {
    totalOrders: number;
    totalRevenue: number;
    totalCustomers: number;
    totalProducts: number;
    revenueGrowth: number;
  };
  revenueChart: Array<{ date: string; revenue: number; orders: number }>;
  topProducts: Array<{ id: number; name: string; imageUrl?: string; soldQuantity: number; revenue: number }>;
  recentOrders: Array<{
    orderCode: string;
    customerName: string;
    totalAmount: number;
    status: string;
    createdAt: string;
  }>;
  orderStatus: {
    pending: number;
    confirmed: number;
    processing: number;
    shipped: number;
    delivered: number;
    cancelled: number;
  };
}

export const dashboardService = {
  getOverview: () =>
    axiosClient.get<DashboardOverview>(API_ENDPOINTS.DASHBOARD_STATS),
};
