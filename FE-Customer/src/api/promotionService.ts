import axiosClient, { type ApiResponse } from './axiosClient';
import { API_ENDPOINTS } from './constants';

export type PromotionDiscountType = 'PERCENT' | 'AMOUNT';

interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages?: number;
}

export interface Promotion {
  id: number;
  promoCode: string;
  title: string;
  description?: string;
  discountType: PromotionDiscountType;
  discountValue: number;
  minOrderValue?: number | null;
  maxDiscountValue?: number | null;
  usageLimit?: number | null;
  usedCount: number;
  status: number;
  startAt?: string | null;
  endAt?: string | null;
}

export interface PromotionSearchParams {
  page?: number;
  pageSize?: number;
  keyword?: string;
  status?: number;
}

export interface PromotionValidation {
  isValid: boolean;
  message?: string;
  code?: string;
  discountType?: PromotionDiscountType;
  discountValue: number;
  discountAmount: number;
  finalAmount: number;
}

export const promotionService = {
  search: (
    params: PromotionSearchParams = {}
  ): Promise<ApiResponse<PagedResult<Promotion>>> =>
    axiosClient.get<PagedResult<Promotion>>(API_ENDPOINTS.PROMOTION_SEARCH, {
      params,
    }),

  verify: (
    code: string,
    orderAmount: number
  ): Promise<ApiResponse<PromotionValidation>> =>
    axiosClient.post<PromotionValidation>(API_ENDPOINTS.PROMOTION_VERIFY, {
      code,
      orderAmount,
    }),
};
