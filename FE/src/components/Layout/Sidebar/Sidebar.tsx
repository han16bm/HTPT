import React, { useMemo } from 'react';
import { Layout, Menu } from 'antd';
import {
  DashboardOutlined,
  AppstoreOutlined,
  ShoppingCartOutlined,
  TeamOutlined,
  FileTextOutlined,
  BarChartOutlined,
  TagsOutlined,
  GiftOutlined,
  InboxOutlined,
  ReadOutlined,
  MailOutlined,
  ShoppingOutlined,
  ShopOutlined,
} from '@ant-design/icons';
import { useNavigate, useLocation } from 'react-router-dom';
import classNames from 'classnames/bind';
import type { MenuProps } from 'antd';
import styles from './Sidebar.module.scss';
import { FishLogo } from '@/components/common';
import { useAuth } from '@/contexts';
import { APP_SHORT_NAME } from '@/shared/utils/constants';

const cx = classNames.bind(styles);
const { Sider } = Layout;

interface SidebarProps {
  collapsed: boolean;
}

type MenuItem = Required<MenuProps>['items'][number];

const Sidebar: React.FC<SidebarProps> = ({ collapsed }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { hasRole } = useAuth();

  const menuItems = useMemo<MenuItem[]>(() => {
    const items: MenuItem[] = [
      {
        key: '/',
        icon: <DashboardOutlined />,
        label: 'Dashboard',
      },
      {
        key: 'catalog',
        icon: <AppstoreOutlined />,
        label: 'Sản phẩm',
        children: [
          {
            key: '/products',
            icon: <ShoppingOutlined />,
            label: 'Danh sách sản phẩm',
          },
          {
            key: '/categories',
            icon: <TagsOutlined />,
            label: 'Danh mục',
          },
          {
            key: '/inventory',
            icon: <InboxOutlined />,
            label: 'Quản lý kho',
          },
        ],
      },
      {
        key: 'sales',
        icon: <ShoppingCartOutlined />,
        label: 'Bán hàng',
        children: [
          {
            key: '/sales',
            icon: <ShopOutlined />,
            label: 'Bán tại quầy (POS)',
          },
          {
            key: '/orders',
            icon: <FileTextOutlined />,
            label: 'Đơn hàng',
          },
          {
            key: '/promotions',
            icon: <GiftOutlined />,
            label: 'Khuyến mãi',
          },
        ],
      },
      {
        key: '/customers',
        icon: <TeamOutlined />,
        label: 'Khách hàng',
      },
    ];

    // Chỉ Admin mới thấy nội dung & báo cáo
    if (hasRole('ADMIN')) {
      items.push(
        {
          key: 'content',
          icon: <ReadOutlined />,
          label: 'Nội dung',
          children: [
            {
              key: '/blog',
              icon: <ReadOutlined />,
              label: 'Bài viết',
            },
            {
              key: '/contacts',
              icon: <MailOutlined />,
              label: 'Liên hệ',
            },
          ],
        },
        {
          key: '/reports',
          icon: <BarChartOutlined />,
          label: 'Báo cáo',
        }
      );
    }

    return items;
  }, [hasRole]);

  // Tính selectedKey + openKey từ pathname
  const { selectedKeys, openKeys } = useMemo(() => {
    const path = location.pathname;
    const open: string[] = [];
    if (
      path.startsWith('/products') ||
      path.startsWith('/categories') ||
      path.startsWith('/inventory')
    ) {
      open.push('catalog');
    }
    if (
      path.startsWith('/sales') ||
      path.startsWith('/orders') ||
      path.startsWith('/promotions')
    ) {
      open.push('sales');
    }
    if (path.startsWith('/blog') || path.startsWith('/contacts')) {
      open.push('content');
    }
    return { selectedKeys: [path], openKeys: open };
  }, [location.pathname]);

  const handleMenuClick: MenuProps['onClick'] = ({ key }) => {
    if (key.startsWith('/')) {
      navigate(key);
    }
  };

  return (
    <Sider
      trigger={null}
      collapsible
      collapsed={collapsed}
      className={cx('sidebar')}
      width={250}
      theme="dark"
    >
      <div className={cx('logo', { collapsed })}>
        <div className={cx('logoIcon')}>
          <FishLogo size={collapsed ? 34 : 38} />
        </div>
        {!collapsed && <div className={cx('logoText')}>{APP_SHORT_NAME}</div>}
      </div>
      <Menu
        theme="dark"
        mode="inline"
        selectedKeys={selectedKeys}
        defaultOpenKeys={openKeys}
        items={menuItems}
        onClick={handleMenuClick}
      />
    </Sider>
  );
};

export default Sidebar;