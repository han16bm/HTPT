import React from 'react';
import { Form, Input, Button } from 'antd';
import { UserOutlined, LockOutlined } from '@ant-design/icons';
import { useNavigate, useLocation } from 'react-router-dom';
import { toast } from 'react-toastify';
import classNames from 'classnames/bind';
import styles from './Login.module.scss';
import { FishLogo } from '@/components/common';
import { useAuth } from '@/contexts';
import type { LoginRequest } from '@/interfaces';

const cx = classNames.bind(styles);

interface LocationState {
  from?: string;
}

const Login: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { login, isLoading } = useAuth();

  const onFinish = async (values: LoginRequest) => {
    try {
      const user = await login(values);
      toast.success(`Chào mừng ${user.fullName || user.username}!`);
      const from = (location.state as LocationState | null)?.from || '/';
      navigate(from, { replace: true });
    } catch (err) {
      const msg =
        (err as { message?: string })?.message ||
        'Tài khoản hoặc mật khẩu không chính xác, hoặc bạn không có quyền đăng nhập.';
      toast.error(msg);
    }
  };

  return (
    <div className={cx('loginPage')}>
      <div className={cx('loginBox')}>
        <div className={cx('logoSection')}>
          <div className={cx('fishLogo')}>
            <FishLogo size={84} />
          </div>
        </div>
        <h2 className={cx('title')}>Đăng nhập</h2>
        <p className={cx('subtitle')}>Hệ thống quản trị H&H FISH SHOP</p>

        <Form
          name="login"
          onFinish={onFinish}
          autoComplete="off"
          size="large"
          layout="vertical"
        >
          <Form.Item
            name="username"
            rules={[
              { required: true, message: 'Vui lòng nhập tên đăng nhập!' },
            ]}
          >
            <Input prefix={<UserOutlined />} placeholder="Tên đăng nhập" />
          </Form.Item>

          <Form.Item
            name="password"
            rules={[{ required: true, message: 'Vui lòng nhập mật khẩu!' }]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Mật khẩu" />
          </Form.Item>

          <Form.Item>
            <Button
              type="primary"
              htmlType="submit"
              className={cx('loginBtn')}
              loading={isLoading}
              block
            >
              ĐĂNG NHẬP
            </Button>
          </Form.Item>
        </Form>
      </div>
    </div>
  );
};

export default Login;