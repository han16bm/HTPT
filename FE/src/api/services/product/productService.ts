import axiosClient from '../../axiosClient';
import { API_ENDPOINTS } from '../../constants';
import type {
  PagedResult,
  Product,
  ProductSearchParams,
  ProductUpsertRequest,
} from '@/interfaces';

function buildFormData(data: ProductUpsertRequest): FormData {
  const fd = new FormData();

  if (data.id != null) fd.append('id', String(data.id));
  fd.append('categoryId', String(data.categoryId));
  fd.append('name', data.name);
  if (data.shortDescription) fd.append('shortDescription', data.shortDescription);
  if (data.description) fd.append('description', data.description);
  if (data.imageUrl) fd.append('imageUrl', data.imageUrl);
  if (data.imageFile) fd.append('imageFile', data.imageFile);
  if (data.removeImage) fd.append('removeImage', 'true');
  fd.append('costPrice', String(data.costPrice));
  fd.append('salePrice', String(data.salePrice));
  fd.append('stockQuantity', String(data.stockQuantity));
  if (data.weightGrams != null) fd.append('weightGrams', String(data.weightGrams));
  fd.append('isFeatured', String(data.isFeatured ?? false));
  fd.append('status', String(data.status != null ? data.status === 1 : true));

  (data.images ?? []).forEach((img, i) => {
    const prefix = `images[${i}]`;
    if (img.id != null) fd.append(`${prefix}.id`, String(img.id));
    if (img.imageUrl) fd.append(`${prefix}.imageUrl`, img.imageUrl);
    if (img.imageFile) fd.append(`${prefix}.imageFile`, img.imageFile);
    if (img.altText) fd.append(`${prefix}.altText`, img.altText);
    fd.append(`${prefix}.isPrimary`, String(img.isPrimary ?? false));
    fd.append(`${prefix}.displayOrder`, String(img.displayOrder));
    if (img.remove) fd.append(`${prefix}.remove`, 'true');
  });

  return fd;
}

export const productService = {
  search: (params: ProductSearchParams = {}) => {
    const { search, ...rest } = params;

    return axiosClient.get<PagedResult<Product>>(API_ENDPOINTS.PRODUCT_SEARCH, {
      params: {
        ...rest,
        name: search,
      },
    });
  },

  getById: (id: number | string) =>
    axiosClient.get<Product>(`${API_ENDPOINTS.PRODUCT_DETAIL}/${id}`),

  getBySlug: (slug: string) =>
    axiosClient.get<Product>(`${API_ENDPOINTS.PRODUCT_BY_SLUG}/${encodeURIComponent(slug)}`),

  upsert: (data: ProductUpsertRequest) => {
    const body = buildFormData(data);
    const config = {
      headers: { 'Content-Type': 'multipart/form-data' },
    };

    return data.id
      ? axiosClient.put<Product>(`${API_ENDPOINTS.PRODUCT_UPSERT}/${data.id}`, body, config)
      : axiosClient.post<Product>(API_ENDPOINTS.PRODUCT_UPSERT, body, config);
  },

  delete: (id: number | string) =>
    axiosClient.delete<void>(`${API_ENDPOINTS.PRODUCT_DELETE}/${id}`),
};
