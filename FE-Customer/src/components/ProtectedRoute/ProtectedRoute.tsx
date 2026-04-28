import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { authService } from '@/api';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

/**
 * Bọc route yêu cầu đăng nhập.
 * Nếu chưa đăng nhập → redirect sang /login?returnUrl=<trang hiện tại>
 */
const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children }) => {
  const location = useLocation();

  if (!authService.isAuthenticated()) {
    return (
      <Navigate
        to={`/login?returnUrl=${encodeURIComponent(location.pathname + location.search)}`}
        replace
      />
    );
  }

  return <>{children}</>;
};

export default ProtectedRoute;
