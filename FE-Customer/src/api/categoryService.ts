import axiosClient, { ApiResponse } from './axiosClient';
import { API_ENDPOINTS } from './constants';

export interface Category {
  id: number;
  parentId?: number;
  categoryCode: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  displayOrder: number;
  status: boolean;
  children: Category[];
}

export const categoryService = {
  /** GET /api/products/categories/tim-kiem — lấy danh sách danh mục đang active */
  getAll: (): Promise<ApiResponse<Category[]>> => {
    return axiosClient.get(API_ENDPOINTS.CATEGORIES, {
      params: { status: true },
    });
  },
};
