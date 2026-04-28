import React, { useEffect, useState } from 'react';
import { Table, Tag, Button, message, Empty, Breadcrumb } from 'antd';
import { EyeOutlined, ShoppingOutlined, OrderedListOutlined } from '@ant-design/icons';
import { useNavigate, Link } from 'react-router-dom';
import { CustomerLayout } from '@/components/Layout';
import { orderService, type Order } from '@/api';
import { formatVND } from '@/utils/format';
import type { ColumnsType } from 'antd/es/table';
import styles from './MyOrders.module.scss';

const MyOrders: React.FC = () => {
  const navigate = useNavigate();
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    document.title = 'Đơn hàng của tôi | H&H Fish Shop';
    fetchOrders();

    return () => {
      document.title = 'H&H Fish Shop';
    };
  }, []);

  const fetchOrders = async () => {
    setLoading(true);
    try {
      const response = await orderService.getOrders();
      if (response.success && response.data) {
        setOrders(response.data);
      } else {
        message.error(response.error || 'Không thể tải danh sách đơn hàng');
      }
    } catch {
      message.error('Không thể tải danh sách đơn hàng');
    } finally {
      setLoading(false);
    }
  };

  const formatPrice = formatVND;

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusColor = (status: string) => {
    const statusColors: Record<string, string> = {
      // BE enums
      PENDING: 'gold',
      CONFIRMED: 'blue',
      SHIPPING: 'cyan',
      COMPLETED: 'green',
      CANCELLED: 'red',
    };
    return statusColors[status?.toUpperCase()] || 'default';
  };

  const getStatusText = (status: string) => {
    const statusTexts: Record<string, string> = {
      // BE enums
      PENDING: 'Chờ xác nhận',
      CONFIRMED: 'Đã xác nhận',
      SHIPPING: 'Đang giao',
      COMPLETED: 'Hoàn thành',
      CANCELLED: 'Đã hủy',
    };
    return statusTexts[status?.toUpperCase()] || status;
  };

  const getPaymentText = (method?: string) => {
    if (!method) return '—';
    const map: Record<string, string> = {
      COD: 'Tiền mặt (COD)',
      BANK_TRANSFER: 'Chuyển khoản',
      CASH: 'Tiền mặt',
    };
    return map[method.toUpperCase()] ?? method;
  };

  const columns: ColumnsType<Order> = [
    {
      title: 'Mã đơn hàng',
      dataIndex: 'orderCode',
      key: 'orderCode',
      render: (code, record) => (
        <span className={styles.orderCode}>{code || `#${record.id}`}</span>
      ),
    },
    {
      title: 'Ngày đặt',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => formatDate(date),
    },
    {
      title: 'Thanh toán',
      dataIndex: 'paymentMethod',
      key: 'paymentMethod',
      render: (method) => getPaymentText(method),
    },
    {
      title: 'Tổng tiền',
      dataIndex: 'totalAmount',
      key: 'totalAmount',
      align: 'right' as const,
      render: (total: number) => <strong className={styles.totalAmt}>{formatPrice(total)}</strong>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'orderStatus',
      key: 'orderStatus',
      render: (status: string) => (
        <Tag color={getStatusColor(status)}>
          {getStatusText(status)}
        </Tag>
      ),
    },
    {
      title: '',
      key: 'action',
      render: (_, record) => (
        <Button
          type="link"
          icon={<EyeOutlined />}
          onClick={() => {
            if (record.orderCode) {
              navigate(`/my-orders/${record.orderCode}`);
            } else {
              message.warning('Không thể xem chi tiết: đơn hàng chưa có mã.');
            }
          }}
        >
          Chi tiết
        </Button>
      ),
    },
  ];

  return (
    <CustomerLayout>
      <div className={styles.myOrders}>
        <Breadcrumb
          className={styles.breadcrumb}
          items={[
            { title: <Link to="/">Trang chủ</Link> },
            { title: 'Đơn hàng của tôi' },
          ]}
        />

        <div className={styles.pageHeader}>
          <div className={styles.headingBlock}>
            <h1 className={styles.title}>
              <OrderedListOutlined /> Đơn hàng của tôi
            </h1>
           
          </div>
          {orders.length > 0 && <span className={styles.badge}>{orders.length} đơn</span>}
        </div>

        <div className={styles.tableWrapper}>
          <Table
            columns={columns}
            dataSource={orders}
            rowKey="id"
            loading={loading}
            pagination={{ pageSize: 10 }}
            locale={{
              emptyText: (
                <Empty
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="Bạn chưa có đơn hàng nào"
                >
                  <Button
                    type="primary"
                    icon={<ShoppingOutlined />}
                    onClick={() => navigate('/danhmuc')}
                  >
                    Mua sắm ngay
                  </Button>
                </Empty>
              ),
            }}
          />
        </div>
      </div>
    </CustomerLayout>
  );
};

export default MyOrders;
