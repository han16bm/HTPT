import axiosClient from '../axiosClient';
import { API_ENDPOINTS } from '../constants';
import type {
  OrderSummaryReport,
  RevenueReportRow,
  TopProductReportRow,
} from '@/interfaces';

export interface RevenueReportParams {
  fromDate: string;
  toDate: string;
  groupBy?: 'day' | 'week' | 'month';
}

export const reportService = {
  getRevenue: (params: RevenueReportParams) =>
    axiosClient.get<RevenueReportRow[]>(API_ENDPOINTS.REPORT_REVENUE, {
      params,
    }),

  getTopProducts: (params: { fromDate: string; toDate: string; limit?: number }) =>
    axiosClient.get<TopProductReportRow[]>(API_ENDPOINTS.REPORT_TOP_PRODUCTS, {
      params,
    }),

  getOrderSummary: (params: { fromDate: string; toDate: string }) =>
    axiosClient.get<OrderSummaryReport>(API_ENDPOINTS.REPORT_ORDER_SUMMARY, {
      params,
    }),
};
