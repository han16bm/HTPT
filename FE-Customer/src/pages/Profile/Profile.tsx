import React, { useEffect, useState } from 'react';
import {
  Avatar,
  Button,
  Card,
  Form,
  Input,
  message,
  Select,
  Spin,
  Tabs,
} from 'antd';
import { SaveOutlined, UserOutlined } from '@ant-design/icons';
import {
  authService,
  type ChangePasswordRequest,
  type UpdateProfileRequest,
  type User,
} from '@/api';
import { CustomerLayout } from '@/components/Layout';
import styles from './Profile.module.scss';

const Profile: React.FC = () => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [changingPassword, setChangingPassword] = useState(false);
  const [form] = Form.useForm<UpdateProfileRequest>();
  const [pwdForm] = Form.useForm<ChangePasswordRequest & { confirmPassword: string }>();

  useEffect(() => {
    const loadProfile = async () => {
      setLoading(true);
      try {
        const res = await authService.getProfile();
        if (res.success && res.data) {
          setUser(res.data);
          form.setFieldsValue({
            fullName: res.data.fullName || '',
            email: res.data.email || '',
            phone: res.data.phone || '',
            dateOfBirth: res.data.dateOfBirth ? String(res.data.dateOfBirth).slice(0, 10) : undefined,
            gender: res.data.gender || undefined,
          });
          localStorage.setItem('customer_user', JSON.stringify(res.data));
          window.dispatchEvent(new Event('customer-user-updated'));
          return;
        }

        const local = authService.getCurrentUser();
        if (local) {
          setUser(local);
          form.setFieldsValue({
            fullName: local.fullName || '',
            email: local.email || '',
            phone: local.phone || '',
            dateOfBirth: local.dateOfBirth ? String(local.dateOfBirth).slice(0, 10) : undefined,
            gender: local.gender || undefined,
          });
        }
      } finally {
        setLoading(false);
      }
    };

    void loadProfile();
  }, [form]);

  const handleUpdateProfile = async (values: UpdateProfileRequest) => {
    setSaving(true);
    try {
      const res = await authService.updateProfile(values);
      if (!res.success || !res.data) {
        message.error(res.error || 'Không thể cập nhật thông tin');
        return;
      }

      setUser(res.data);
      form.setFieldsValue({
        fullName: res.data.fullName || '',
        email: res.data.email || '',
        phone: res.data.phone || '',
        dateOfBirth: res.data.dateOfBirth ? String(res.data.dateOfBirth).slice(0, 10) : undefined,
        gender: res.data.gender || undefined,
      });
      localStorage.setItem('customer_user', JSON.stringify(res.data));
      window.dispatchEvent(new Event('customer-user-updated'));
      message.success('Cập nhật thông tin thành công');
    } finally {
      setSaving(false);
    }
  };

  const handleChangePassword = async (values: ChangePasswordRequest & { confirmPassword: string }) => {
    setChangingPassword(true);
    try {
      const res = await authService.changePassword({
        currentPassword: values.currentPassword,
        newPassword: values.newPassword,
      });

      if (!res.success) {
        message.error(res.error || 'Không thể đổi mật khẩu');
        return;
      }

      pwdForm.resetFields();
      message.success(res.message || 'Đổi mật khẩu thành công');
    } finally {
      setChangingPassword(false);
    }
  };

  if (loading) {
    return (
      <CustomerLayout>
        <div className={styles.center}><Spin size="large" /></div>
      </CustomerLayout>
    );
  }

  return (
    <CustomerLayout>
      <div className={styles.container}>
        <div className={styles.header}>
          <Avatar size={84} icon={<UserOutlined />} src={user?.avatarUrl} />
          <div>
            <h1 className={styles.name}>{user?.fullName || user?.username}</h1>
            <p className={styles.username}>@{user?.username}</p>
            {user?.customerCode && <p className={styles.meta}>Mã KH: {user.customerCode}</p>}
          </div>
        </div>

        <Card>
          <Tabs
            defaultActiveKey="info"
            items={[
              {
                key: 'info',
                label: 'Thông tin cá nhân',
                children: (
                    <Form<UpdateProfileRequest>
                    form={form}
                    layout="vertical"
                    onFinish={handleUpdateProfile}
                  >
                    <div className={styles.grid}>
                      <Form.Item label="Tên đăng nhập">
                        <Input value={user?.username} disabled />
                      </Form.Item>

                      <Form.Item label="Mã khách hàng">
                        <Input value={user?.customerCode} disabled />
                      </Form.Item>

                      <Form.Item
                        label="Họ tên"
                        name="fullName"
                        rules={[{ required: true, message: 'Vui lòng nhập họ tên' }]}
                      >
                        <Input />
                      </Form.Item>

                      <Form.Item
                        label="Email"
                        name="email"
                        rules={[{ type: 'email', message: 'Email không hợp lệ' }]}
                      >
                        <Input />
                      </Form.Item>

                      <Form.Item label="Số điện thoại" name="phone">
                        <Input />
                      </Form.Item>

                      <Form.Item label="Ngày sinh" name="dateOfBirth">
                        <Input type="date" />
                      </Form.Item>

                      <Form.Item label="Giới tính" name="gender">
                        <Select
                          allowClear
                          options={[
                            { value: 'MALE', label: 'Nam' },
                            { value: 'FEMALE', label: 'Nữ' },
                            { value: 'OTHER', label: 'Khác' },
                          ]}
                        />
                      </Form.Item>
                    </div>

                    <div className={styles.formFooter}>
                      <Form.Item style={{ marginBottom: 0 }}>
                        <Button
                          type="primary"
                          icon={<SaveOutlined />}
                          htmlType="submit"
                          loading={saving}
                        >
                          Lưu thay đổi
                        </Button>
                      </Form.Item>
                    </div>
                  </Form>
                ),
              },
              {
                key: 'password',
                label: 'Đổi mật khẩu',
                children: (
                  <Form
                    form={pwdForm}
                    layout="vertical"
                    onFinish={handleChangePassword}
                    style={{ maxWidth: 520 }}
                  >
                    <Form.Item
                      label="Mật khẩu hiện tại"
                      name="currentPassword"
                      rules={[{ required: true, message: 'Vui lòng nhập mật khẩu hiện tại' }]}
                    >
                      <Input.Password />
                    </Form.Item>

                    <Form.Item
                      label="Mật khẩu mới"
                      name="newPassword"
                      rules={[{ required: true, min: 6, message: 'Tối thiểu 6 ký tự' }]}
                    >
                      <Input.Password />
                    </Form.Item>

                    <Form.Item
                      label="Xác nhận mật khẩu mới"
                      name="confirmPassword"
                      dependencies={['newPassword']}
                      rules={[
                        { required: true, message: 'Vui lòng xác nhận mật khẩu' },
                        ({ getFieldValue }) => ({
                          validator(_, value) {
                            if (!value || getFieldValue('newPassword') === value) {
                              return Promise.resolve();
                            }

                            return Promise.reject(new Error('Mật khẩu xác nhận không khớp'));
                          },
                        }),
                      ]}
                    >
                      <Input.Password />
                    </Form.Item>

                    <Form.Item>
                      <Button type="primary" htmlType="submit" loading={changingPassword}>
                        Đổi mật khẩu
                      </Button>
                    </Form.Item>
                  </Form>
                ),
              },
            ]}
          />
        </Card>
      </div>
    </CustomerLayout>
  );
};

export default Profile;
