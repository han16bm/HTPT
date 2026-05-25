import axiosClient, { ApiResponse } from './axiosClient';
import { API_ENDPOINTS } from './constants';

export type BlogStatus = 'DRAFT' | 'PUBLISHED';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages?: number;
}

export interface BlogPost {
  id: number;
  categoryId?: number;
  categoryName?: string;
  title: string;
  slug: string;
  summary?: string;
  content?: string;
  thumbnailUrl?: string;
  status: BlogStatus;
  publishedAt?: string;
  createdAt: string;
}

export interface BlogCategory {
  id: number;
  name: string;
  slug?: string;
  postCount?: number;
}

export interface BlogSearchParams {
  page?: number;
  pageSize?: number;
  keyword?: string;
  categoryId?: number;
  status?: BlogStatus;
}

export const blogService = {
  search: (
    params: BlogSearchParams = {}
  ): Promise<ApiResponse<PagedResult<BlogPost>>> =>
    axiosClient.get(API_ENDPOINTS.BLOG, { params }),

  getBySlug: (slug: string): Promise<ApiResponse<BlogPost>> =>
    axiosClient.get(API_ENDPOINTS.BLOG_DETAIL(slug)),

  getCategories: (): Promise<ApiResponse<BlogCategory[]>> =>
    axiosClient.get(API_ENDPOINTS.BLOG_CATEGORIES),
};
