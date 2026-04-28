import axiosClient from '../axiosClient';
import { API_ENDPOINTS } from '../constants';
import type {
  PagedResult,
  Promotion,
  PromotionUpsertRequest,
} from '@/interfaces';

export interface PromotionSearchParams {
  page?: number;
  pageSize?: number;
  keyword?: string;
  status?: number;
}

export const promotionService = {
  search: (params: PromotionSearchParams = {}) =>
    axiosClient.get<PagedResult<Promotion>>(API_ENDPOINTS.PROMOTION_SEARCH, {
      params,
    }),

  upsert: (data: PromotionUpsertRequest) =>
    axiosClient.post<Promotion>(API_ENDPOINTS.PROMOTION_UPSERT, data),

  delete: (id: number) =>
    axiosClient.post<void>(API_ENDPOINTS.PROMOTION_DELETE, { id }),

  verify: (code: string, orderAmount: number) =>
    axiosClient.post<{
      promoCode: string;
      discountType: 'PERCENT' | 'AMOUNT';
      discountValue: number;
      discountAmount: number;
      finalAmount: number;
    }>(API_ENDPOINTS.PROMOTION_VERIFY, { code, orderAmount }),
};
