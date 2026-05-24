// ──────────────────────────────────────────────────────────────────────────────
// Auth Service — kết nối User service thông qua Gateway
// Gateway: http://localhost:5000
// ──────────────────────────────────────────────────────────────────────────────

import axiosClient, { ApiResponse } from './axiosClient';
import { API_ENDPOINTS } from './constants';

export interface User {
  id: number;
  username: string;
  customerCode?: string;
  fullName?: string;
  email?: string;
  phone?: string;
  dateOfBirth?: string;
  gender?: string;
  avatarUrl?: string;
  roleCode?: string;      // 'CUSTOMER' | 'ADMIN' | ...
  roleName?: string;
  isAdmin?: boolean;
  customerId?: number;
}

export interface UpdateProfileRequest {
  fullName: string;
  email?: string;
  phone?: string;
  dateOfBirth?: string;
  gender?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface LoginRequest {
  username: string; // email hoặc số điện thoại
  password: string;
}

export interface RegisterRequest {
  username: string;   // BE bắt buộc, min 4 ký tự
  fullName: string;
  email?: string;
  phone?: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;     // BE trả 'accessToken', không phải 'token'
  refreshToken: string;
  expiresIn: number;       // giây
  user: User;
}

// ── Token helpers ─────────────────────────────────────────────────────────────
const TOKEN_KEY = 'customer_token';
const REFRESH_TOKEN_KEY = 'customer_refresh_token';
const USER_KEY = 'customer_user';

export const authService = {
  /** Dang nhap - POST /api/user/auth/login */
  login: async (data: LoginRequest): Promise<ApiResponse<LoginResponse>> => {
    const resp = await axiosClient.post<LoginResponse>(API_ENDPOINTS.LOGIN, data);
    if (resp.success && resp.data) {
      // BE trả 'accessToken', không phải 'token'
      localStorage.setItem(TOKEN_KEY, resp.data.accessToken);
      if (resp.data.refreshToken) {
        localStorage.setItem(REFRESH_TOKEN_KEY, resp.data.refreshToken);
      }
      localStorage.setItem(USER_KEY, JSON.stringify(resp.data.user));
    }
    return resp;
  },

  /** Dang ky - POST /api/user/auth/register */
  register: async (data: RegisterRequest): Promise<ApiResponse<User>> => {
    return axiosClient.post<User>(API_ENDPOINTS.REGISTER, data);
  },

  /** Đăng xuất — gọi POST /api/user/auth/logout */
  logout: async (): Promise<void> => {
    try {
      await axiosClient.post(API_ENDPOINTS.LOGOUT);
    } catch {
      // Dù backend lỗi vẫn xóa token local
    } finally {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(REFRESH_TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
    }
  },

  /** Lấy thông tin người dùng hiện tại từ BE */
  getProfile: async (): Promise<ApiResponse<User>> => {
    return axiosClient.get<User>(API_ENDPOINTS.PROFILE);
  },

  /** Cập nhật thông tin cá nhân */
  updateProfile: async (data: UpdateProfileRequest): Promise<ApiResponse<User>> => {
    return axiosClient.put<User>(API_ENDPOINTS.UPDATE_PROFILE, data);
  },

  /** Đổi mật khẩu */
  changePassword: async (data: ChangePasswordRequest): Promise<ApiResponse<void>> => {
    return axiosClient.put<void>(API_ENDPOINTS.CHANGE_PASSWORD, data);
  },

  /** Lấy user từ localStorage (không gọi API) */
  getCurrentUser: (): User | null => {
    const userStr = localStorage.getItem(USER_KEY);
    return userStr ? JSON.parse(userStr) : null;
  },

  /** Lấy token hiện tại */
  getToken: (): string | null => {
    return localStorage.getItem(TOKEN_KEY);
  },

  /** Kiểm tra đã đăng nhập chưa */
  isAuthenticated: (): boolean => {
    return !!localStorage.getItem(TOKEN_KEY);
  },
};
