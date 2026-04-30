import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Layout, Badge, Dropdown, Button, Input, Row, Col } from 'antd';
import {
  ShoppingCartOutlined,
  UserOutlined,
  LoginOutlined,
  LogoutOutlined,
  ProfileOutlined,
  ShoppingOutlined,
  SearchOutlined,
  PhoneOutlined,
  EnvironmentOutlined,
} from '@ant-design/icons';
import { authService, cartService, categoryService, type Category } from '@/api';
import { OceanBackground } from '@/components/OceanBackground';
import { resolveImageUrl, useImageFallback } from '@/utils/image';
import styles from './CustomerLayout.module.scss';

const { Header, Content, Footer } = Layout;

interface CustomerLayoutProps {
  children: React.ReactNode;
}

const CustomerLayout: React.FC<CustomerLayoutProps> = ({ children }) => {
  const navigate = useNavigate();
  const [isAuth, setIsAuth] = React.useState(() => authService.isAuthenticated());
  const [currentUser, setCurrentUser] = React.useState(() => authService.getCurrentUser());
  const [searchQuery, setSearchQuery] = React.useState('');
  const [scrolled, setScrolled] = React.useState(false);
  const [categories, setCategories] = React.useState<Category[]>([]);
  const [hoveredParentId, setHoveredParentId] = React.useState<number | null>(null);

  React.useEffect(() => {
    categoryService.getAll().then(res => {
      if (res.success && res.data) setCategories(res.data);
    });
  }, []);
  const [cartCount, setCartCount] = React.useState(0);

  // Re-sync auth state whenever localStorage changes (cross-tab) or on mount
  React.useEffect(() => {
    const sync = () => {
      setIsAuth(authService.isAuthenticated());
      setCurrentUser(authService.getCurrentUser());
    };
    window.addEventListener('storage', sync);
    window.addEventListener('customer-user-updated', sync);
    return () => {
      window.removeEventListener('storage', sync);
      window.removeEventListener('customer-user-updated', sync);
    };
  }, []);

  React.useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 48);
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  // Lấy số lượng giỏ hàng khi đăng nhập lần đầu
  React.useEffect(() => {
    if (!isAuth) { setCartCount(0); return; }
    cartService.getCart().then((res) => {
      if (res.success && res.data) setCartCount(res.data.totalItems);
    }).catch(() => {/* ignore */});
  }, [isAuth]);

  // Lắng nghe sự kiện "cart-updated" từ các trang Cart / Checkout
  // để cập nhật badge mà không cần gọi API thêm lần nào
  React.useEffect(() => {
    const onCartUpdated = (e: Event) => {
      const count = (e as CustomEvent<number>).detail;
      setCartCount(count);
    };
    window.addEventListener('cart-updated', onCartUpdated);
    return () => window.removeEventListener('cart-updated', onCartUpdated);
  }, []);

  const handleLogout = async () => {
    await authService.logout();
    setIsAuth(false);
    setCurrentUser(null);
    navigate('/login');
  };

  const handleSearch = () => {
    if (searchQuery.trim()) {
      navigate(`/search?q=${encodeURIComponent(searchQuery)}`);
    }
  };

  const userMenuItems = isAuth
    ? [
        {
          key: 'profile',
          icon: <ProfileOutlined />,
          label: 'Thông tin cá nhân',
          onClick: () => navigate('/profile'),
        },
        {
          key: 'orders',
          icon: <ShoppingOutlined />,
          label: 'Đơn hàng của tôi',
          onClick: () => navigate('/my-orders'),
        },
        {
          type: 'divider' as const,
        },
        {
          key: 'logout',
          icon: <LogoutOutlined />,
          label: 'Đăng xuất',
          onClick: handleLogout,
        },
      ]
    : [
        {
          key: 'login',
          icon: <LoginOutlined />,
          label: 'Đăng nhập',
          onClick: () => navigate('/login'),
        },
      ];

  const rootCategories = categories.filter(c => !c.parentId);
  const activeChildren = hoveredParentId
    ? categories.filter(c => c.parentId === hoveredParentId)
    : [];

  return (
    <Layout className={styles.layout}>
      <OceanBackground />
      {/* Main Header */}
      <Header className={`${styles.header} ${scrolled ? styles.headerScrolled : ''}`}>
        <div className={styles.headerContainer}>
          {/* Logo */}
          <div className={styles.logo}>
            <Link to="/">
              <span className={styles.logoIcon}>🐟</span>
              <span className={styles.logoText}>H&H Fish Shop</span>
            </Link>
          </div>

          {/* Search Bar */}
          <div className={styles.searchWrapper}>
            <Input
              placeholder="Tìm kiếm cá cảnh, phụ kiện..."
              prefix={<SearchOutlined />}
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onPressEnter={handleSearch}
              className={styles.searchInput}
              size="large"
            />
          </div>

          {/* Right Actions */}
          <div className={styles.headerActions}>
            {/* Contact */}
            <span className={styles.contact}>
              <PhoneOutlined /> 0853997698
            </span>

            {/* Cart */}
            <Badge count={cartCount} color="#ff4d4f" offset={[-8, 8]}>
              <Button
                type="text"
                icon={<ShoppingCartOutlined />}
                onClick={() => navigate('/cart')}
                className={styles.iconBtn}
              />
            </Badge>

            {/* Account Dropdown */}
            {isAuth ? (
              <Dropdown menu={{ items: userMenuItems }} placement="bottomRight">
                <Button type="text" icon={<UserOutlined />} className={styles.iconBtn}>
                  {currentUser?.fullName && (
                    <span className={styles.userName}>
                      {currentUser.fullName.split(' ').pop()}
                    </span>
                  )}
                </Button>
              </Dropdown>
            ) : (
              <Link to="/login" className={styles.loginLink}>
                <Button type="text" icon={<UserOutlined />} className={styles.iconBtn} />
              </Link>
            )}
          </div>
        </div>
      </Header>

      {/* Navigation Bar */}
      <div className={styles.navbar}>
        <div className={styles.navContainer}>
          <Link to="/" className={styles.navLink}>Trang chủ</Link>

          {/* Categories dropdown */}
          <div className={styles.navDropdown} onMouseLeave={() => setHoveredParentId(null)}>
            <Link to="/danhmuc" className={styles.navLink}>
              Danh mục <span className={styles.dropArrow}>▾</span>
            </Link>
            <div className={styles.dropdownMenu}>
              <div className={styles.dropdownTwoCol}>
                {/* Cột trái: danh mục cha */}
                <div className={`${styles.parentCol} ${activeChildren.length > 0 ? styles.parentColWithChildren : ''}`}>
                  {rootCategories.map(cat => (
                    <div
                      key={cat.id}
                      className={`${styles.parentItem} ${hoveredParentId === cat.id ? styles.parentItemActive : ''}`}
                      onMouseEnter={() => setHoveredParentId(cat.id)}
                      onClick={() => {
                        setHoveredParentId(null);
                        navigate('/danhmuc', { state: { slug: cat.slug } });
                      }}
                    >
                      {cat.imageUrl && (
                        <img
                          src={resolveImageUrl(cat.imageUrl)}
                          alt={cat.name}
                          className={styles.dropItemIcon}
                          onError={useImageFallback}
                        />
                      )}
                      <span className={styles.dropItemName}>{cat.name}</span>
                      {categories.some(c => c.parentId === cat.id) && <span className={styles.childArrow}>›</span>}
                    </div>
                  ))}
                </div>

                {/* Cột phải: danh mục con */}
                {activeChildren.length > 0 && (
                  <div className={styles.childCol}>
                    {activeChildren.slice(0, 5).map(child => (
                      <Link
                        key={child.id}
                        to="/danhmuc"
                        state={{ slug: child.slug }}
                        className={styles.childItem}
                      >
                        {child.name}
                      </Link>
                    ))}
                    {activeChildren.length > 5 && (
                      <Link
                        to="/danhmuc"
                        state={{ slug: activeChildren[0].slug }}
                        className={styles.childItemMore}
                      >
                        Xem thêm {activeChildren.length - 5} mục →
                      </Link>
                    )}
                  </div>
                )}
              </div>
            </div>
          </div>

          <Link to="/promotions" className={styles.navLink}>Khuyến mãi</Link>
          <Link to="/blog" className={styles.navLink}>Blog</Link>
          <Link to="/services" className={styles.navLink}>Dịch vụ</Link>
          <Link to="/policies" className={styles.navLink}>Chính sách</Link>
          <Link to="/about" className={styles.navLink}>Về chúng tôi</Link>
          <Link to="/contact" className={styles.navLink}>Liên hệ</Link>
        </div>
      </div>

      <Content className={styles.content}>
        <div className={styles.contentWrapper}>{children}</div>
      </Content>

      {/* Footer */}
      <Footer className={styles.footer}>
        {/* Main Footer */}
        <div className={styles.footerMain}>
          <Row gutter={[32, 32]}>
            {/* About Section */}
            <Col xs={24} sm={24} md={6}>
              <div className={styles.footerSection}>
                <h3>Về Cá Cảnh Shop</h3>
                <p>
                  Chuyên cung cấp các loại cá cảnh chất lượng cao, phụ kiện bể cá và dịch vụ tư vấn.
                </p>
              </div>
            </Col>

            {/* Contact Section */}
            <Col xs={24} sm={24} md={6}>
              <div className={styles.footerSection}>
                <h3>Liên hệ chúng tôi</h3>
                <p>
                  <PhoneOutlined /> Hotline: 0853997698
                </p>
                <p>Email: phieu1601@gmail.com</p>
                <p>
                  <EnvironmentOutlined /> Đường Lê Trọng Tấn, Dương Nội, Hà Nội.
                </p>
              </div>
            </Col>

            {/* Policies Section */}
            <Col xs={24} sm={24} md={6}>
              <div className={styles.footerSection}>
                <h3>Chính sách</h3>
                <ul className={styles.policyList}>
                  <li><Link to="/policies">Chính sách giao hàng</Link></li>
                  <li><Link to="/policies">Chính sách đổi trả</Link></li>
                  <li><Link to="/policies">Chính sách bảo hành</Link></li>
                  <li><Link to="/policies">Chính sách bảo mật</Link></li>
                </ul>
              </div>
            </Col>

            {/* Support Section */}
            <Col xs={24} sm={24} md={6}>
              <div className={styles.footerSection}>
                <h3>Hỗ trợ khách hàng</h3>
                <ul className={styles.policyList}>
                  <li><Link to="/about">Về chúng tôi</Link></li>
                  <li><Link to="/services">Dịch vụ</Link></li>
                  <li><Link to="/contact">Hợp tác kinh doanh</Link></li>
                  <li><Link to="/contact">Hỗ trợ kỹ thuật</Link></li>
                </ul>
              </div>
            </Col>
          </Row>
        </div>

        {/* Bottom Footer */}
        <div className={styles.footerBottom}>
          <Row justify="space-between" align="middle">
            <Col xs={24} md={12}>
              <p className={styles.copyright}>
                © 2026 H&H Fish Shop - Được cung cấp bởi Han và Hieu™
              </p>
            </Col>
            <Col xs={24} md={12} className={styles.socialLinks}>
              <a href="#facebook">Facebook</a>
              <span>|</span>
              <a href="#instagram">Instagram</a>
              <span>|</span>
              <a href="#youtube">YouTube</a>
              <span>|</span>
              <a href="#zalo">Zalo</a>
            </Col>
          </Row>
        </div>
      </Footer>
    </Layout>
  );
};

export default CustomerLayout;
