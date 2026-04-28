// ============================================
// Axios client — auto unwrap ApiResponse<T>
// ============================================
import axios, {
  type AxiosError,
  type AxiosInstance,
  type AxiosRequestConfig,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from 'axios';
import { toast } from 'react-toastify';
import { env } from '@/config/env';
import {
  ACCESS_TOKEN_KEY,
  REFRESH_TOKEN_KEY,
  USER_INFO_KEY,
} from '@/shared/utils/constants';
import type { ApiResponse } from '@/interfaces';
import { ERROR_MESSAGES, HTTP_STATUS, REQUEST_TIMEOUT } from './constants';

export interface ApiError {
  message: string;
  status?: number;
  errors?: string[];
}

const isApiResponse = (data: unknown): data is ApiResponse =>
  !!data &&
  typeof data === 'object' &&
  'success' in (data as Record<string, unknown>);

class AxiosClient {
  private instance: AxiosInstance;
  private isRefreshing = false;
  private refreshWaiters: Array<() => void> = [];

  constructor() {
    this.instance = axios.create({
      baseURL: env.API_BASE_URL,
      timeout: REQUEST_TIMEOUT,
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
    });

    this.setupInterceptors();
  }

  private setupInterceptors(): void {
    this.instance.interceptors.request.use(
      (config: InternalAxiosRequestConfig) => {
        const token = localStorage.getItem(ACCESS_TOKEN_KEY);
        if (token && token !== 'undefined' && token !== 'null') {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    this.instance.interceptors.response.use(
      (response: AxiosResponse) => response,
      (error: AxiosError) => this.handleError(error)
    );
  }

  private handleError(error: AxiosError): Promise<never> {
    const apiError: ApiError = { message: ERROR_MESSAGES.UNKNOWN_ERROR };

    if (error.response) {
      const { status, data } = error.response;
      apiError.status = status;

      const body = (data ?? {}) as {
        message?: string;
        errors?: string[];
      };

      switch (status) {
        case HTTP_STATUS.BAD_REQUEST:
          apiError.message = body.message || 'Dữ liệu không hợp lệ';
          apiError.errors = body.errors;
          break;
        case HTTP_STATUS.UNAUTHORIZED:
          apiError.message = ERROR_MESSAGES.UNAUTHORIZED;
          this.clearAuth();
          if (typeof window !== 'undefined' && window.location.pathname !== '/login') {
            toast.error(apiError.message);
            window.location.href = '/login';
          }
          break;
        case HTTP_STATUS.FORBIDDEN:
          apiError.message = body.message || ERROR_MESSAGES.FORBIDDEN;
          toast.error(apiError.message);
          break;
        case HTTP_STATUS.NOT_FOUND:
          apiError.message = body.message || ERROR_MESSAGES.NOT_FOUND;
          break;
        case HTTP_STATUS.INTERNAL_SERVER_ERROR:
          apiError.message = body.message || ERROR_MESSAGES.SERVER_ERROR;
          toast.error(apiError.message);
          break;
        default:
          apiError.message = body.message || ERROR_MESSAGES.UNKNOWN_ERROR;
      }
    } else if (error.code === 'ECONNABORTED') {
      apiError.message = ERROR_MESSAGES.TIMEOUT;
      toast.error(apiError.message);
    } else if (error.request) {
      apiError.message = ERROR_MESSAGES.NETWORK_ERROR;
      toast.error(apiError.message);
    }

    return Promise.reject(apiError);
  }

  private clearAuth(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_INFO_KEY);
  }

  /**
   * Unwrap `ApiResponse<T>` về `T` trực tiếp.
   * Nếu BE trả raw data (không bọc) thì trả nguyên giá trị.
   * Nếu `success === false` thì throw để caller catch.
   */
  private unwrap<T>(payload: unknown): T {
    if (isApiResponse(payload)) {
      if (payload.success === false) {
        const err: ApiError = {
          message: payload.message || ERROR_MESSAGES.UNKNOWN_ERROR,
          errors: payload.errors,
        };
        throw err;
      }
      return (payload.data as T) ?? (undefined as unknown as T);
    }
    return payload as T;
  }

  async get<T = unknown>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const res = await this.instance.get<ApiResponse<T> | T>(url, config);
    return this.unwrap<T>(res.data);
  }

  async post<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig
  ): Promise<T> {
    const res = await this.instance.post<ApiResponse<T> | T>(url, data, config);
    return this.unwrap<T>(res.data);
  }

  async put<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig
  ): Promise<T> {
    const res = await this.instance.put<ApiResponse<T> | T>(url, data, config);
    return this.unwrap<T>(res.data);
  }

  async patch<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig
  ): Promise<T> {
    const res = await this.instance.patch<ApiResponse<T> | T>(url, data, config);
    return this.unwrap<T>(res.data);
  }

  async delete<T = unknown>(
    url: string,
    config?: AxiosRequestConfig
  ): Promise<T> {
    const res = await this.instance.delete<ApiResponse<T> | T>(url, config);
    return this.unwrap<T>(res.data);
  }

  getInstance(): AxiosInstance {
    return this.instance;
  }
}

const axiosClient = new AxiosClient();
export default axiosClient;
