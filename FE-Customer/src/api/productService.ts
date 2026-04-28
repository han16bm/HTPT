import axiosClient, { ApiResponse } from './axiosClient';
import { API_ENDPOINTS } from './constants';

export interface Product {
  id: number;
  productCode: string;
  name: string;
  slug: string;
  shortDescription?: string;
  description?: string;
  imageUrl?: string;
  salePrice: number;
  costPrice?: number;
  stockQuantity: number;
  soldQuantity: number;
  status: boolean;
  isFeatured: boolean;
  categoryId: number;
  categoryName?: string;
  createdAt?: string;
}

export interface ProductsResponse {
  items: Product[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages?: number;
  hasNextPage?: boolean;
  hasPreviousPage?: boolean;
}

export type ProductSortOption =
  | 'newest'
  | 'price-asc'
  | 'price-desc'
  | 'best-seller'
  | 'name-asc'
  | 'name-desc';

export interface ProductListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: number;
  category?: string;
  status?: boolean;
  minPrice?: number;
  maxPrice?: number;
  sort?: ProductSortOption | string;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
}

const mapSortParams = (sort?: ProductListParams['sort']) => {
  switch (sort) {
    case 'price-asc':
      return { sortBy: 'price', sortDir: 'asc' as const };
    case 'price-desc':
      return { sortBy: 'price', sortDir: 'desc' as const };
    case 'best-seller':
      return { sortBy: 'sold', sortDir: 'desc' as const };
    case 'name-asc':
      return { sortBy: 'name', sortDir: 'asc' as const };
    case 'name-desc':
      return { sortBy: 'name', sortDir: 'desc' as const };
    case 'newest':
    default:
      return { sortBy: 'created_at', sortDir: 'desc' as const };
  }
};

const buildProductQueryParams = (params: ProductListParams = {}) => {
  const {
    search,
    categoryId,
    status = true,
    minPrice,
    maxPrice,
    sort,
    sortBy,
    sortDir,
    category: _category,
    ...rest
  } = params;

  const query: Record<string, string | number | boolean> = {
    ...rest,
    status,
  };

  if (search?.trim()) {
    query.name = search.trim();
  }

  if (typeof categoryId === 'number') {
    query.categoryId = categoryId;
  }

  if (typeof minPrice === 'number') {
    query.minPrice = minPrice;
  }

  if (typeof maxPrice === 'number') {
    query.maxPrice = maxPrice;
  }

  const resolvedSort = sortBy
    ? { sortBy, sortDir: sortDir ?? 'desc' }
    : mapSortParams(sort);

  query.sortBy = resolvedSort.sortBy;
  query.sortDir = resolvedSort.sortDir;

  return query;
};

export const productService = {
  getAll: async (
    params?: ProductListParams
  ): Promise<ApiResponse<ProductsResponse>> => {
    return axiosClient.get(API_ENDPOINTS.PRODUCTS, {
      params: buildProductQueryParams(params),
    });
  },

  getAllPages: async (params?: ProductListParams): Promise<Product[]> => {
    const pageSize = Math.max(params?.pageSize ?? 100, 1);
    const items: Product[] = [];
    let page = 1;
    let totalPages = 1;

    do {
      const response = await productService.getAll({
        ...params,
        page,
        pageSize,
      });

      if (!response.success || !response.data) {
        throw new Error(response.error || 'Khong the tai danh sach san pham');
      }

      items.push(...response.data.items);

      totalPages = response.data.totalPages
        ?? Math.max(1, Math.ceil(response.data.totalCount / response.data.pageSize));

      if (response.data.items.length === 0) {
        break;
      }

      page += 1;
    } while (page <= totalPages);

    return items;
  },

  getById: (id: string | number): Promise<ApiResponse<Product>> => {
    return axiosClient.get(API_ENDPOINTS.PRODUCT_DETAIL(id));
  },

  getBySlug: (slug: string): Promise<ApiResponse<Product>> => {
    return axiosClient.get(API_ENDPOINTS.PRODUCT_BY_SLUG(slug));
  },

  search: (
    keyword: string,
    page = 1,
    pageSize = 12
  ): Promise<ApiResponse<ProductsResponse>> => {
    return productService.getAll({ page, pageSize, search: keyword });
  },

  getFeatured: (top = 12): Promise<ApiResponse<Product[]>> => {
    return axiosClient.get(API_ENDPOINTS.PRODUCT_FEATURED, { params: { top } });
  },
};
