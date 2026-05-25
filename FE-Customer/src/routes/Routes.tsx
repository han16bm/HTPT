import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Home } from '@/pages/Home';
import { Cart } from '@/pages/Cart';
import { Checkout } from '@/pages/Checkout';
import { MyOrders } from '@/pages/MyOrders';
import { Login } from '@/pages/Login';
import { Register } from '@/pages/Register';
import { Categories } from '@/pages/Categories';
import { SearchResults } from '@/pages/SearchResults';
import { About } from '@/pages/About';
import { Contact } from '@/pages/Contact';
import { Blog } from '@/pages/Blog';
import { Promotions } from '@/pages/Promotions';
import { Policies } from '@/pages/Policies';
import { Services } from '@/pages/Services';
import { ProductDetail } from '@/pages/ProductDetail';
import { Profile } from '@/pages/Profile';
import { OrderDetail } from '@/pages/OrderDetail';
import { Page404 } from '@/pages/Page404';
import ProtectedRoute from '@/components/ProtectedRoute/ProtectedRoute';
import { ScrollToTop } from '@/components/ScrollToTop';

const AppRoutes: React.FC = () => {
  return (
    <Router>
      <ScrollToTop />
      <Routes>
        {/* Public Routes */}
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        {/* Products */}
        <Route path="/products/:slug" element={<ProductDetail />} />

        {/* Categories */}
        <Route path="/danhmuc" element={<Categories />} />

        {/* Search */}
        <Route path="/search" element={<SearchResults />} />

        {/* Info pages */}
        <Route path="/about" element={<About />} />
        <Route path="/contact" element={<Contact />} />
        <Route path="/blog" element={<Blog />} />
        <Route path="/blog/:slug" element={<Blog />} />
        <Route path="/promotions" element={<Promotions />} />
        <Route path="/policies" element={<Policies />} />
        <Route path="/services" element={<Services />} />

        {/* Protected Routes */}
        <Route path="/cart" element={<ProtectedRoute><Cart /></ProtectedRoute>} />
        <Route path="/checkout" element={<ProtectedRoute><Checkout /></ProtectedRoute>} />
        <Route path="/my-orders" element={<ProtectedRoute><MyOrders /></ProtectedRoute>} />
        <Route path="/my-orders/:code" element={<ProtectedRoute><OrderDetail /></ProtectedRoute>} />
        <Route path="/profile" element={<ProtectedRoute><Profile /></ProtectedRoute>} />

        {/* 404 Page */}
        <Route path="*" element={<Page404 />} />
      </Routes>
    </Router>
  );
};

export default AppRoutes;
