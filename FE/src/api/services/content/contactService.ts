import axiosClient from '../../axiosClient';
import { API_ENDPOINTS } from '../../constants';
import type { Contact, ContactStatus, PagedResult } from '@/interfaces';

export interface ContactSearchParams {
  page?: number;
  pageSize?: number;
  status?: ContactStatus;
  keyword?: string;
}

export const contactService = {
  search: (params: ContactSearchParams = {}) =>
    axiosClient.get<PagedResult<Contact>>(API_ENDPOINTS.CONTACT_SEARCH, {
      params,
    }),

  updateStatus: (id: number, status: ContactStatus) =>
    axiosClient.patch<Contact>(`${API_ENDPOINTS.CONTACT_UPDATE_STATUS}/${id}/status`, { status }),
};
