import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse, AxiosError } from 'axios';
import { API_BASE_URL } from './constants';

export interface ApiResponse<T = any> {
  success: boolean;
  data?: T;
  message?: string;
  error?: string;
}

class AxiosClient {
  private instance: AxiosInstance;

  constructor() {
    this.instance = axios.create({
      baseURL: API_BASE_URL,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.setupInterceptors();
  }

  private setupInterceptors(): void {
    // Request interceptor
    this.instance.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('customer_token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error: AxiosError) => {
        return Promise.reject(error);
      }
    );

    // Response interceptor
    this.instance.interceptors.response.use(
      (response: AxiosResponse) => {
        return response;
      },
      (error: AxiosError) => {
        if (error.response?.status === 401) {
          const isOnLoginPage = window.location.pathname === '/login';
          if (!isOnLoginPage) {
            localStorage.removeItem('customer_token');
            localStorage.removeItem('customer_refresh_token');
            localStorage.removeItem('customer_user');
            window.location.href = '/login';
          }
        }
        return Promise.reject(error);
      }
    );
  }

  async get<T = any>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    try {
      const response = await this.instance.get<ApiResponse<T>>(url, config);
      // BE returns ApiResponse<T> directly, just pass it through
      if (response.data && typeof response.data === 'object' && 'success' in response.data) {
        return response.data as ApiResponse<T>;
      }
      return { success: true, data: response.data as any };
    } catch (error) {
      return this.handleError(error);
    }
  }

  async post<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    try {
      const response = await this.instance.post<ApiResponse<T>>(url, data, config);
      if (response.data && typeof response.data === 'object' && 'success' in response.data) {
        return response.data as ApiResponse<T>;
      }
      return { success: true, data: response.data as any };
    } catch (error) {
      return this.handleError(error);
    }
  }

  async put<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    try {
      const response = await this.instance.put<ApiResponse<T>>(url, data, config);
      if (response.data && typeof response.data === 'object' && 'success' in response.data) {
        return response.data as ApiResponse<T>;
      }
      return { success: true, data: response.data as any };
    } catch (error) {
      return this.handleError(error);
    }
  }

  async delete<T = any>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    try {
      const response = await this.instance.delete<ApiResponse<T>>(url, config);
      if (response.data && typeof response.data === 'object' && 'success' in response.data) {
        return response.data as ApiResponse<T>;
      }
      return { success: true, data: response.data as any };
    } catch (error) {
      return this.handleError(error);
    }
  }

  private handleError(error: any): ApiResponse {
    if (axios.isAxiosError(error)) {
      const beMessage = error.response?.data?.message || error.response?.data?.error;
      return {
        success: false,
        error: beMessage || error.message || 'Đã xảy ra lỗi',
      };
    }
    return {
      success: false,
      error: 'Đã xảy ra lỗi không xác định',
    };
  }
}

export default new AxiosClient();
