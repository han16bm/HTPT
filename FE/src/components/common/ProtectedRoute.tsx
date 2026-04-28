import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { Result, Button } from 'antd';
import { useAuth } from '@/contexts';
import type { UserRole } from '@/interfaces';

interface ProtectedRouteProps {
  children: React.ReactNode;
  /** Khi false: trang chỉ dành cho user CHƯA đăng nhập (vd /login). */
  isProtected?: boolean;
  /** Giới hạn role được phép truy cập. */
  roles?: UserRole[];
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  isProtected = true,
  roles,
}) => {
  const { isAuthenticated, user } = useAuth();
  const location = useLocation();

  if (isProtected && !isAuthenticated) {
    return (
      <Navigate to="/login" replace state={{ from: location.pathname }} />
    );
  }

  if (!isProtected && isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  if (roles && user && !roles.includes(user.role)) {
    return (
      <Result
        status="403"
        title="403"
        subTitle="Bạn không có quyền truy cập trang này."
        extra={
          <Button type="primary" onClick={() => window.history.back()}>
            Quay lại
          </Button>
        }
      />
    );
  }

  return <>{children}</>;
};

export default ProtectedRoute;
