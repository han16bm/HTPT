import React, { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { formatVND } from '@/utils/format';
import {
  Card,
  Tag,
  Descriptions,
  Table,
  Button,
  Spin,
  Empty,
  Modal,
  Input,
  message,
  Breadcrumb,
} from 'antd';
import { ArrowLeftOutlined, HomeOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { CustomerLayout } from '@/components/Layout';
import { orderService, type Order, type OrderItem } from '@/api';
import styles from './OrderDetail.module.scss';

const STATUS_COLOR: Record<string, string> = {
  PENDING: 'gold',
  CONFIRMED: 'blue',
  SHIPPING: 'cyan',
  COMPLETED: 'green',
  CANCELLED: 'red',
};
const STATUS_TEXT: Record<string, string> = {
  PENDING: 'Chờ xác nhận',
  CONFIRMED: 'Đã xác nhận',
  SHIPPING: 'Đang giao',
  COMPLETED: 'Hoàn thành',
  CANCELLED: 'Đã hủy',
};

const OrderDetail: React.FC = () => {
  const { code } = useParams<{ code: string }>();
  const navigate = useNavigate();
  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(false);
  const [cancelOpen, setCancelOpen] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [cancelling, setCancelling] = useState(false);

  const fmt = formatVND;

  const fetchOrder = async () => {
    if (!code) return;
    setLoading(true);
    try {
      const res = await orderService.getOrderByCode(code);
      if (res.success && res.data) setOrder(res.data);
      else setOrder(null);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrder();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [code]);

  const handleCancel = async () => {
    if (!order) return;
    setCancelling(true);
    try {
      const res = await orderService.cancelOrder({
        orderCode: order.orderCode,   // BE dùng orderCode
        reason: cancelReason || 'Khách hàng hủy',
      });
      if (res.success) {
        message.success('Đã hủy đơn hàng');
        setCancelOpen(false);
        setCancelReason('');
        fetchOrder();
      } else {
        message.error(res.error || 'Không thể hủy đơn');
      }
    } finally {
      setCancelling(false);
    }
  };

  const columns: ColumnsType<OrderItem> = [
    { title: 'Sản phẩm', dataIndex: 'productName', key: 'productName' },
    {
      title: 'Đơn giá',
      dataIndex: 'unitPrice',
      key: 'unitPrice',
      render: (v: number) => fmt(v),
      align: 'right',
    },
    {
      title: 'SL',
      dataIndex: 'quantity',
      key: 'quantity',
      align: 'center',
      width: 80,
    },
    {
      title: 'Thành tiền',
      dataIndex: 'subTotal',   // BE trả 'subTotal'
      key: 'subTotal',
      render: (v: number) => <strong>{fmt(v)}</strong>,
      align: 'right',
    },
  ];

  if (loading) {
    return (
      <CustomerLayout>
        <div className={styles.center}><Spin size="large" /></div>
      </CustomerLayout>
    );
  }

  if (!order) {
    return (
      <CustomerLayout>
        <div className={styles.center}>
          <Empty description="Không tìm thấy đơn hàng" />
        </div>
      </CustomerLayout>
    );
  }

  const canCancel = order.orderStatus?.toUpperCase() === 'PENDING';

  return (
    <CustomerLayout>
      <div className={styles.container}>
        <Breadcrumb
          className={styles.breadcrumb}
          items={[
            { title: <Link to="/"><HomeOutlined /> Trang chủ</Link> },
            { title: <Link to="/my-orders">Đơn hàng của tôi</Link> },
            { title: order.orderCode || `#${order.id}` },
          ]}
        />

        <div className={styles.headerRow}>
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/my-orders')}>
            Quay lại
          </Button>
          {canCancel && (
            <Button danger onClick={() => setCancelOpen(true)}>
              Hủy đơn
            </Button>
          )}
        </div>

        <Card title={`Đơn hàng ${order.orderCode || `#${order.id}`}`}>
          <Descriptions column={{ xs: 1, sm: 2 }} bordered size="small">
            <Descriptions.Item label="Trạng thái">
              <Tag color={STATUS_COLOR[order.orderStatus?.toUpperCase()] || 'default'}>
                {STATUS_TEXT[order.orderStatus?.toUpperCase()] || order.orderStatus}
              </Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Ngày đặt">
              {order.createdAt
                ? new Date(order.createdAt).toLocaleString('vi-VN')
                : '—'}
            </Descriptions.Item>
            <Descriptions.Item label="Thanh toán">
              {order.paymentMethod || '—'}
            </Descriptions.Item>
            <Descriptions.Item label="Tổng tiền">
              <strong style={{ color: '#ef233c' }}>{fmt(order.totalAmount ?? 0)}</strong>
            </Descriptions.Item>
            <Descriptions.Item label="Địa chỉ giao hàng" span={2}>
              {order.shippingAddress || '—'}
            </Descriptions.Item>
            {order.notes && (
              <Descriptions.Item label="Ghi chú" span={2}>
                {order.notes}
              </Descriptions.Item>
            )}
          </Descriptions>

          <div className={styles.itemsTitle}>Sản phẩm</div>
          <Table
            rowKey="id"
            columns={columns}
            dataSource={order.items}
            pagination={false}
            size="small"
          />
        </Card>

        <Modal
          open={cancelOpen}
          title="Hủy đơn hàng"
          onCancel={() => setCancelOpen(false)}
          onOk={handleCancel}
          okButtonProps={{ danger: true, loading: cancelling }}
          okText="Xác nhận hủy"
          cancelText="Đóng"
        >
          <p>Vui lòng nhập lý do hủy đơn (tùy chọn):</p>
          <Input.TextArea
            rows={3}
            value={cancelReason}
            onChange={(e) => setCancelReason(e.target.value)}
            placeholder="Lý do..."
          />
        </Modal>
      </div>
    </CustomerLayout>
  );
};

export default OrderDetail;
