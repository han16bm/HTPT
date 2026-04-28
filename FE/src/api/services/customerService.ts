import axiosClient from '../axiosClient';
import { API_ENDPOINTS } from '../constants';
import type {
  Customer,
  CustomerUpsertRequest,
  PagedResult,
} from '@/interfaces';

export interface CustomerSearchParams {
  page?: number;
  pageSize?: number;
  keyword?: string;
}

const normalizeDateInput = (value?: string) => {
  if (!value) return value;

  const [datePart] = value.split('T');
  return /^\d{4}-\d{2}-\d{2}$/.test(datePart) ? datePart : value;
};

const normalizeCustomer = (customer: Customer & { isActive?: boolean; status?: number | boolean }): Customer => {
  const isActive = typeof customer.isActive === 'boolean'
    ? customer.isActive
    : typeof customer.status === 'boolean'
      ? customer.status
      : customer.status === 1;

  return {
    ...customer,
    dateOfBirth: normalizeDateInput(customer.dateOfBirth),
    isActive,
    status: isActive ? 1 : 0,
  };
};

export const customerService = {
  search: async (params: CustomerSearchParams = {}) => {
    const result = await axiosClient.get<PagedResult<Customer & { isActive?: boolean; status?: number | boolean }>>(
      API_ENDPOINTS.CUSTOMER_SEARCH,
      {
        params,
      },
    );

    return {
      ...result,
      items: (result.items ?? []).map(normalizeCustomer),
    };
  },

  getById: async (id: number) => {
    const result = await axiosClient.get<Customer & { isActive?: boolean; status?: number | boolean }>(
      API_ENDPOINTS.CUSTOMER_DETAIL,
      {
        params: { id },
      },
    );

    return normalizeCustomer(result);
  },

  upsert: (data: CustomerUpsertRequest) =>
    axiosClient.post<Customer>(API_ENDPOINTS.CUSTOMER_UPSERT, data),

  createWalkIn: (data: { fullName: string; phone: string; email?: string }) =>
    axiosClient.post<Customer>(API_ENDPOINTS.CUSTOMER_CREATE_WALKIN, data),

  delete: (id: number) =>
    axiosClient.post<void>(API_ENDPOINTS.CUSTOMER_DELETE, { id }),
};
