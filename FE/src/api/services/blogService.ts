import axiosClient from '../axiosClient';
import { API_ENDPOINTS } from '../constants';
import type {
  BlogCategory,
  BlogPost,
  BlogPostUpsertRequest,
  BlogStatus,
  PagedResult,
} from '@/interfaces';

export interface BlogSearchParams {
  page?: number;
  pageSize?: number;
  categoryId?: number;
  status?: BlogStatus;
  keyword?: string;
}

export const blogService = {
  search: (params: BlogSearchParams = {}) =>
    axiosClient.get<PagedResult<BlogPost>>(API_ENDPOINTS.BLOG_SEARCH, {
      params,
    }),

  getBySlug: (slug: string) =>
    axiosClient.get<BlogPost>(API_ENDPOINTS.BLOG_DETAIL, { params: { slug } }),

  upsert: (data: BlogPostUpsertRequest) =>
    axiosClient.post<BlogPost>(API_ENDPOINTS.BLOG_UPSERT, data),

  delete: (id: number) =>
    axiosClient.post<void>(API_ENDPOINTS.BLOG_DELETE, { id }),

  getCategories: () =>
    axiosClient.get<BlogCategory[]>(API_ENDPOINTS.BLOG_CATEGORIES),
};
