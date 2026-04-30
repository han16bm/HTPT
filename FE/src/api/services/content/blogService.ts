import axiosClient from '../../axiosClient';
import { API_ENDPOINTS } from '../../constants';
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

function buildFormData(data: BlogPostUpsertRequest): FormData {
  const fd = new FormData();

  if (data.id != null) fd.append('id', String(data.id));
  if (data.categoryId != null) fd.append('categoryId', String(data.categoryId));
  fd.append('title', data.title);
  if (data.summary) fd.append('summary', data.summary);
  fd.append('content', data.content);
  if (data.thumbnailUrl) fd.append('thumbnailUrl', data.thumbnailUrl);
  if (data.thumbnailFile) fd.append('thumbnailFile', data.thumbnailFile);
  if (data.removeThumbnail) fd.append('removeThumbnail', 'true');
  fd.append('status', data.status);

  return fd;
}

export const blogService = {
  search: (params: BlogSearchParams = {}) =>
    axiosClient.get<PagedResult<BlogPost>>(API_ENDPOINTS.BLOG_SEARCH, {
      params,
    }),

  getBySlug: (slug: string) =>
    axiosClient.get<BlogPost>(`${API_ENDPOINTS.BLOG_DETAIL}/${encodeURIComponent(slug)}`),

  upsert: (data: BlogPostUpsertRequest) => {
    const body = buildFormData(data);
    const config = {
      headers: { 'Content-Type': 'multipart/form-data' },
    };

    return data.id
      ? axiosClient.put<BlogPost>(`${API_ENDPOINTS.BLOG_UPSERT}/${data.id}`, body, config)
      : axiosClient.post<BlogPost>(API_ENDPOINTS.BLOG_UPSERT, body, config);
  },

  delete: (id: number) =>
    axiosClient.delete<void>(`${API_ENDPOINTS.BLOG_DELETE}/${id}`),

  getCategories: () =>
    axiosClient.get<BlogCategory[]>(API_ENDPOINTS.BLOG_CATEGORIES),
};
