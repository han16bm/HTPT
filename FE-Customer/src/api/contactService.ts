import axiosClient from './axiosClient';
import { API_ENDPOINTS } from './constants';

export interface ContactRequest {
  fullName: string;
  phone: string;
  email?: string;
  message: string;
}

export const contactService = {
  send: (data: ContactRequest) =>
    axiosClient.post(API_ENDPOINTS.CONTACT_SEND, data),
};
