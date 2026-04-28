import React, { useEffect, useState } from 'react';
import {
  Button, Drawer, Form, Input, Popconfirm, Select, Space,
  Table, Tag, Typography, message,
} from 'antd';
import {
  EditOutlined, DeleteOutlined, SearchOutlined, EyeOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import classNames from 'classnames/bind';
import styles from './Customers.module.scss';
import { MasterLayout } from '@/components/Layout';
import { customerService, orderService } from '@/api';
import { usePagedQuery } from '@/hooks';
import { formatCurrency, formatDate } from '@/shared/utils/format';
import { ORDER_STATUS_LABEL, ORDER_STATUS_COLOR } from '@/shared/utils/constants';
import type { Customer, CustomerUpsertRequest, Order } from '@/interfaces';

const cx = classNames.bind(styles);
const { Title } = Typography;

const getCustomerStatusValue = (customer: Customer) => {
  if (typeof customer.isActive === 'boolean') {
    return customer.isActive ? 1 : 0;
  }

  return customer.status === 1 ? 1 : 0;
};

const Customers: React.FC = () => {
  const [messageApi, contextHolder] = message.useMessage();
  const [keyword, setKeyword] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editing, setEditing] = useState<Customer | null>(null);
  const [editLoadingId, setEditLoadingId] = useState<number | null>(null);
  const [saving, setSaving] = useState(false);
  const [form] = Form.useForm<CustomerUpsertRequest>();

  const [orderDrawerOpen, setOrderDrawerOpen] = useState(false);
  const [drawerCustomer, setDrawerCustomer] = useState<Customer | null>(null);
  const [orders, setOrders] = useState<Order[]>([]);
  const [ordersLoading, setOrdersLoading] = useState(false);

  const {
    items, total, page, pageSize, loading, setParams, refetch,
  } = usePagedQuery<Customer, { keyword?: string; page?: number; pageSize?: number }>({
    fetcher: (p) => customerService.search(p),
    initialParams: { page: 1, pageSize: 10 },
    errorMessage: 'Lỗi khi tải danh sách khách hàng',
  });

  useEffect(() => {
    if (editing) {
      form.setFieldsValue({
        fullName: editing.fullName,
        phone: editing.phone,
        email: editing.email,
        dateOfBirth: editing.dateOfBirth,
        gender: editing.gender,
        status: getCustomerStatusValue(editing),
        isAdmin: editing.isAdmin ?? false,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ status: 1, isAdmin: false });
    }
  }, [editing, form]);

  const handleEdit = async (record: Customer) => {
    setEditLoadingId(record.id);
    try {
      const detail = await customerService.getById(record.id);
      setEditing(detail);
      setIsModalOpen(true);
    } catch (err) {
      messageApi.error((err as { message?: string }).message || 'Không thể tải chi tiết khách hàng');
    } finally {
      setEditLoadingId(null);
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await customerService.delete(id);
      messageApi.success('Xóa khách hàng thành công');
      refetch();
    } catch (err) {
      messageApi.error((err as { message?: string }).message || 'Lỗi khi xóa');
    }
  };

  const handleSubmit = async (values: CustomerUpsertRequest) => {
    setSaving(true);
    try {
      await customerService.upsert({ ...values, id: editing?.id });
      messageApi.success('Cập nhật khách hàng thành công');
      setIsModalOpen(false);
      setEditing(null);
      refetch();
    } catch (err) {
      messageApi.error((err as { message?: string }).message || 'Lỗi khi lưu');
    } finally {
      setSaving(false);
    }
  };

  const openOrders = async (customer: Customer) => {
    setDrawerCustomer(customer);
    setOrderDrawerOpen(true);
    setOrdersLoading(true);
    try {
      const result = await orderService.search({ keyword: customer.phone, pageSize: 50 });
      setOrders(result.items ?? []);
    } catch {
      setOrders([]);
    } finally {
      setOrdersLoading(false);
    }
  };

  const columns: ColumnsType<Customer> = [
    {
      title: 'Mã KH',
      dataIndex: 'customerCode',
      key: 'customerCode',
      width: 150,
      render: (v?: string) => (v ? <Tag className={cx('codeTag')} title={v}>{v}</Tag> : '—'),
    },
    {
      title: 'Họ tên',
      dataIndex: 'fullName',
      key: 'fullName',
      width: 240,
      render: (name: string) => (
        <span className={cx('truncateCell')} style={{ fontWeight: 500 }} title={name}>
          {name}
        </span>
      ),
    },
    {
      title: 'Số điện thoại',
      dataIndex: 'phone',
      key: 'phone',
      width: 140,
    },
    {
      title: 'Email',
      dataIndex: 'email',
      key: 'email',
      width: 220,
      render: (v?: string) => v || '—',
    },
    {
      title: 'Giới tính',
      dataIndex: 'gender',
      key: 'gender',
      width: 100,
      render: (v?: string) => v === 'MALE' ? 'Nam' : v === 'FEMALE' ? 'Nữ' : '—',
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: 110,
      render: (_: number | undefined, record) => {
        const status = getCustomerStatusValue(record);
        return (
          <Tag color={status === 1 ? 'green' : 'red'}>{status === 1 ? 'Hoạt động' : 'Bị khóa'}</Tag>
        );
      },
    },
    {
      title: 'Ngày tạo',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 130,
      render: (v: string) => formatDate(v),
    },
    {
      title: 'Hành động',
      key: 'action',
      width: 130,
      fixed: 'right',
      render: (_, record) => (
        <Space size="small">
          <Button
            size="small"
            icon={<EyeOutlined />}
            onClick={() => openOrders(record)}
            title="Xem lịch sử đơn"
          />
          <Button
            type="primary"
            size="small"
            icon={<EditOutlined />}
            loading={editLoadingId === record.id}
            onClick={() => void handleEdit(record)}
          />
          <Popconfirm
            title="Xóa khách hàng này?"
            onConfirm={() => handleDelete(record.id)}
            okText="Xóa"
            cancelText="Hủy"
          >
            <Button danger size="small" icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  const orderColumns: ColumnsType<Order> = [
    { title: 'Mã đơn', dataIndex: 'orderCode', key: 'orderCode', width: 140 },
    {
      title: 'Tổng tiền',
      dataIndex: 'totalAmount',
      key: 'totalAmount',
      width: 120,
      render: (v: number) => formatCurrency(v),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'orderStatus',
      key: 'orderStatus',
      render: (s: Order['orderStatus']) => <Tag color={ORDER_STATUS_COLOR[s]}>{ORDER_STATUS_LABEL[s]}</Tag>,
    },
    { title: 'Ngày tạo', dataIndex: 'createdAt', key: 'createdAt', render: (v) => formatDate(v) },
  ];

  return (
    <MasterLayout>
      {contextHolder}
      <div className={cx('page')}>
        <div className={cx('pageHeader')}>
          <Title level={3} className={cx('pageTitle')}>Khách hàng</Title>
          <Space>
            <Input
              placeholder="Tìm theo tên / SĐT..."
              prefix={<SearchOutlined />}
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              onPressEnter={() => setParams({ keyword, page: 1 })}
              allowClear
              onClear={() => { setKeyword(''); setParams({ keyword: undefined, page: 1 }); }}
              style={{ width: 280 }}
            />
          </Space>
        </div>

        <Table<Customer>
          columns={columns}
          dataSource={items}
          rowKey="id"
          loading={loading}
          scroll={{ x: 1120 }}
          pagination={{
            current: page,
            pageSize,
            total,
            showSizeChanger: true,
            showTotal: (t) => `Tổng ${t} khách hàng`,
            onChange: (p, ps) => setParams({ page: p, pageSize: ps }),
          }}
        />

        <Drawer
          title="Sửa khách hàng"
          open={isModalOpen}
          onClose={() => {
            setIsModalOpen(false);
            setEditing(null);
          }}
          width={480}
          extra={(
            <Button type="primary" onClick={() => form.submit()}>
              {saving ? 'Đang lưu...' : 'Lưu'}
            </Button>
          )}
          destroyOnClose
        >
          <Form<CustomerUpsertRequest>
            form={form}
            layout="vertical"
            onFinish={handleSubmit}
            onFinishFailed={() => {
              messageApi.warning('Vui lòng kiểm tra lại các trường bắt buộc');
            }}
          >
            <Form.Item label="Tên đăng nhập">
              <Input value={editing?.username} disabled placeholder="Chưa có tên đăng nhập" />
            </Form.Item>

            <Form.Item
              name="fullName"
              label="Họ và tên"
              rules={[{ required: true, message: 'Vui lòng nhập tên' }]}
            >
              <Input placeholder="Nguyễn Văn A" />
            </Form.Item>

            <Form.Item
              name="phone"
              label="Số điện thoại"
              rules={[
                { required: true, message: 'Vui lòng nhập SĐT' },
                { pattern: /^[0-9]{9,11}$/, message: 'SĐT không hợp lệ' },
              ]}
            >
              <Input placeholder="0901234567" />
            </Form.Item>

            <Form.Item
              name="email"
              label="Email"
              rules={[{ type: 'email', message: 'Email không hợp lệ' }]}
            >
              <Input placeholder="example@email.com" />
            </Form.Item>

            <Form.Item name="dateOfBirth" label="Ngày sinh">
              <Input type="date" />
            </Form.Item>

            <Form.Item name="gender" label="Giới tính">
              <Select placeholder="Chọn giới tính" allowClear>
                <Select.Option value="MALE">Nam</Select.Option>
                <Select.Option value="FEMALE">Nữ</Select.Option>
                <Select.Option value="OTHER">Khác</Select.Option>
              </Select>
            </Form.Item>

            <Form.Item name="status" label="Trạng thái" initialValue={1}>
              <Select>
                <Select.Option value={1}>Hoạt động</Select.Option>
                <Select.Option value={0}>Bị khóa</Select.Option>
              </Select>
            </Form.Item>

            <Form.Item name="isAdmin" label="Quyền truy cập" initialValue={false}>
              <Select>
                <Select.Option value={false}>Khách hàng</Select.Option>
                <Select.Option value={true}>Quản trị viên</Select.Option>
              </Select>
            </Form.Item>
          </Form>
        </Drawer>

        <Drawer
          title={`Lịch sử đơn hàng — ${drawerCustomer?.fullName}`}
          open={orderDrawerOpen}
          onClose={() => setOrderDrawerOpen(false)}
          width={640}
        >
          <Table<Order>
            columns={orderColumns}
            dataSource={orders}
            rowKey="id"
            loading={ordersLoading}
            pagination={{ pageSize: 10 }}
            size="small"
          />
        </Drawer>
      </div>
    </MasterLayout>
  );
};

export default Customers;
