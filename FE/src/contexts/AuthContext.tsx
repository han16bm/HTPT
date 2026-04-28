import React, {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';
import { authService } from '@/api';
import {
  ACCESS_TOKEN_KEY,
  REFRESH_TOKEN_KEY,
  USER_INFO_KEY,
} from '@/shared/utils/constants';
import type { LoginRequest, User, UserRole } from '@/interfaces';

interface AuthContextValue {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginRequest) => Promise<User>;
  logout: () => Promise<void>;
  hasRole: (...roles: UserRole[]) => boolean;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const readUserFromStorage = (): User | null => {
  try {
    const raw = localStorage.getItem(USER_INFO_KEY);
    if (!raw) return null;
    return JSON.parse(raw) as User;
  } catch {
    return null;
  }
};

const persistAuth = (
  accessToken: string,
  refreshToken: string,
  user: User
): void => {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  localStorage.setItem(USER_INFO_KEY, JSON.stringify(user));
};

const clearAuth = (): void => {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
  localStorage.removeItem(USER_INFO_KEY);
};

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [user, setUser] = useState<User | null>(() => readUserFromStorage());
  const [isLoading, setIsLoading] = useState(false);

  // Đồng bộ user khi token bị xóa ở tab khác
  useEffect(() => {
    const onStorage = (e: StorageEvent) => {
      if (e.key === ACCESS_TOKEN_KEY || e.key === USER_INFO_KEY) {
        setUser(readUserFromStorage());
      }
    };
    window.addEventListener('storage', onStorage);
    return () => window.removeEventListener('storage', onStorage);
  }, []);

  const login = useCallback(async (credentials: LoginRequest): Promise<User> => {
    setIsLoading(true);
    try {
      const data = await authService.login(credentials);
      if (!data?.accessToken || !data?.user) {
        throw new Error('Phản hồi đăng nhập không hợp lệ');
      }

      // Map roleCode từ BE sang role cho FE
      const rawUser = data.user as User & { roleCode?: string };
      const user: User = {
        ...rawUser,
        role: rawUser.role || (rawUser.roleCode as UserRole) || 'CUSTOMER',
      };

      // Trang admin chỉ cho ADMIN/STAFF login
      if (user.role !== 'ADMIN' && user.role !== 'STAFF') {
        throw new Error('Tài khoản hoặc mật khẩu không chính xác, hoặc bạn không có quyền đăng nhập.');
      }

      persistAuth(data.accessToken, data.refreshToken ?? '', user);
      setUser(user);
      return user;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const logout = useCallback(async () => {
    try {
      await authService.logout();
    } catch {
      // ignore — vẫn clear local state
    } finally {
      clearAuth();
      setUser(null);
    }
  }, []);

  const hasRole = useCallback(
    (...roles: UserRole[]) => !!user && roles.includes(user.role),
    [user]
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: !!user,
      isLoading,
      login,
      logout,
      hasRole,
    }),
    [user, isLoading, login, logout, hasRole]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextValue => {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return ctx;
};
