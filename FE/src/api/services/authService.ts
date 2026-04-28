import axiosClient from '../axiosClient';
import { API_ENDPOINTS } from '../constants';
import type { LoginRequest, LoginResponse, User } from '@/interfaces';

export const authService = {
  login: (data: LoginRequest) =>
    axiosClient.post<LoginResponse>(API_ENDPOINTS.AUTH_LOGIN, data),

  logout: () => axiosClient.post<void>(API_ENDPOINTS.AUTH_LOGOUT),

  refreshToken: (refreshToken: string) =>
    axiosClient.post<LoginResponse>(API_ENDPOINTS.AUTH_REFRESH, { refreshToken }),

  getCurrentUser: () => axiosClient.get<User>(API_ENDPOINTS.AUTH_ME),
};
