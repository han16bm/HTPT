import React, { Suspense, lazy } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ProtectedRoute, PageLoader } from '@/components/common';
import type { UserRole } from '@/interfaces';

// Lazy-loaded pages
const Login = lazy(() => import('@/pages/Login/Login'));
const Dashboard = lazy(() => import('@/pages/Dashboard/Dashboard'));
const Products = lazy(() => import('@/pages/Products/Products'));
const Categories = lazy(() => import('@/pages/Categories/Categories'));
const Inventory = lazy(() => import('@/pages/Inventory/Inventory'));
const Customers = lazy(() => import('@/pages/Customers/Customers'));
const Orders = lazy(() => import('@/pages/Orders/Orders'));
const Sales = lazy(() => import('@/pages/Sales/Sales'));
const Promotions = lazy(() => import('@/pages/Promotions/Promotions'));
const Blog = lazy(() => import('@/pages/Blog/Blog'));
const Contacts = lazy(() => import('@/pages/Contacts/Contacts'));
const Reports = lazy(() => import('@/pages/Reports/Reports'));
const Page404 = lazy(() => import('@/pages/Error/Page404'));

interface RouteConfig {
  path: string;
  element: React.ReactNode;
  isProtected?: boolean;
  roles?: UserRole[];
}

const ALL_STAFF: UserRole[] = ['ADMIN', 'STAFF'];
const ADMIN_ONLY: UserRole[] = ['ADMIN'];

export const appRoutes: RouteConfig[] = [
  // ===== Public auth =====
  { path: '/login', element: <Login />, isProtected: false },

  // ===== Admin + Staff =====
  { path: '/', element: <Dashboard />, roles: ALL_STAFF },
  { path: '/products', element: <Products />, roles: ALL_STAFF },
  { path: '/customers', element: <Customers />, roles: ALL_STAFF },
  { path: '/orders', element: <Orders />, roles: ALL_STAFF },
  { path: '/sales', element: <Sales />, roles: ALL_STAFF },
  { path: '/contacts', element: <Contacts />, roles: ALL_STAFF },

  // ===== Admin only =====
  { path: '/categories', element: <Categories />, roles: ADMIN_ONLY },
  { path: '/inventory', element: <Inventory />, roles: ADMIN_ONLY },
  { path: '/promotions', element: <Promotions />, roles: ADMIN_ONLY },
  { path: '/blog', element: <Blog />, roles: ADMIN_ONLY },
  { path: '/reports', element: <Reports />, roles: ADMIN_ONLY },
];

const AppRoutes: React.FC = () => {
  return (
    <BrowserRouter>
      <Suspense fallback={<PageLoader />}>
        <Routes>
          {appRoutes.map((route) => (
            <Route
              key={route.path}
              path={route.path}
              element={
                <ProtectedRoute
                  isProtected={route.isProtected ?? true}
                  roles={route.roles}
                >
                  {route.element}
                </ProtectedRoute>
              }
            />
          ))}
          <Route path="*" element={<Page404 />} />
        </Routes>
      </Suspense>
    </BrowserRouter>
  );
};

export default AppRoutes;
