import axiosClient from '../../axiosClient';
import { API_ENDPOINTS } from '../../constants';
import type { Category, CategoryUpsertRequest } from '@/interfaces';

function buildFormData(data: CategoryUpsertRequest): FormData {
  const fd = new FormData();

  if (data.id != null) fd.append('id', String(data.id));
  if (data.parentId != null) fd.append('parentId', String(data.parentId));
  if (data.categoryCode) fd.append('categoryCode', data.categoryCode);
  fd.append('name', data.name);
  if (data.slug) fd.append('slug', data.slug);
  if (data.description) fd.append('description', data.description);
  if (data.imageUrl) fd.append('imageUrl', data.imageUrl);
  if (data.imageFile) fd.append('imageFile', data.imageFile);
  if (data.removeImage) fd.append('removeImage', 'true');
  fd.append('displayOrder', String(data.displayOrder ?? 1));
  fd.append('status', String(data.status ?? true));

  return fd;
}

export const categoryService = {
  getAll: () => axiosClient.get<Category[]>(API_ENDPOINTS.CATEGORY_SEARCH),
  getTree: () => axiosClient.get<Category[]>(API_ENDPOINTS.CATEGORY_TREE),

  upsert: (data: CategoryUpsertRequest) => {
    const body = buildFormData(data);
    const config = {
      headers: { 'Content-Type': 'multipart/form-data' },
    };

    return data.id
      ? axiosClient.put<Category>(`${API_ENDPOINTS.CATEGORY_UPSERT}/${data.id}`, body, config)
      : axiosClient.post<Category>(API_ENDPOINTS.CATEGORY_UPSERT, body, config);
  },

  delete: (id: number) =>
    axiosClient.delete<void>(`${API_ENDPOINTS.CATEGORY_DELETE}/${id}`),
};
