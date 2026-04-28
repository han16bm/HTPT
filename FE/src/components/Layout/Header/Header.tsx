import React from 'react';
import { Layout, Button, Avatar, Dropdown, Space, Tag } from 'antd';
import {
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  UserOutlined,
  LogoutOutlined,
  DownOutlined,
  SettingOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import classNames from 'classnames/bind';
import type { MenuProps } from 'antd';
import styles from './Header.module.scss';
import { useAuth } from '@/contexts';
import { ROLE_LABEL } from '@/shared/utils/constants';

const cx = classNames.bind(styles);
const { Header: AntHeader } = Layout;

interface HeaderProps {
  collapsed: boolean;
  onToggle: () => void;
}

const Header: React.FC<HeaderProps> = ({ collapsed, onToggle }) => {
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const handleLogout = async () => {
    await logout();
    navigate('/login', { replace: true });
  };

  const dropdownItems: MenuProps['items'] = [
    {
      key: 'profile',
      icon: <UserOutlined />,
      label: 'Thông tin tài khoản',
      disabled: true,
    },
    {
      key: 'settings',
      icon: <SettingOutlined />,
      label: 'Cài đặt',
      disabled: true,
    },
    { type: 'divider' },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Đăng xuất',
      danger: true,
      onClick: handleLogout,
    },
  ];

  return (
    <AntHeader className={cx('header')}>
      <div className={cx('leftSection')}>
        <Button
          type="text"
          icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
          onClick={onToggle}
          className={cx('triggerBtn')}
        />
        <h1 className={cx('title')}>HỆ THỐNG QUẢN TRỊ H&H FISH SHOP</h1>
      </div>

      <div className={cx('rightSection')}>
        <Dropdown menu={{ items: dropdownItems }} trigger={['click']}>
          <Space className={cx('userInfo')}>
            <Avatar
              icon={<UserOutlined />}
              src={user?.avatarUrl ?? undefined}
              style={{ backgroundColor: '#198754' }}
            />
            <div className={cx('userMeta')}>
              <span className={cx('userName')}>
                {user?.fullName || user?.username || 'User'}
              </span>
              {user?.role && (
                <Tag color={user.role === 'ADMIN' ? 'green' : 'blue'} className={cx('roleTag')}>
                  {ROLE_LABEL[user.role]}
                </Tag>
              )}
            </div>
            <DownOutlined style={{ fontSize: 12, color: '#6c757d' }} />
          </Space>
        </Dropdown>
      </div>
    </AntHeader>
  );
};

export default Header;
