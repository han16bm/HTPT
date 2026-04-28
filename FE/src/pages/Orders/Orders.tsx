import React, { useState } from 'react';
import {
  Button,
  DatePicker,
  Drawer,
  Dropdown,
  Input,
  Select,
  Space,
  Spin,
  Table,
  Tag,
  Typography,
  message,
} from 'antd';
import {
  DownOutlined,
  SearchOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import classNames from 'classnames/bind';
import styles from './Orders.module.scss';
import { MasterLayout } from '@/components/Layout';
import { orderService } from '@/api';
import { usePagedQuery } from '@/hooks';
import { formatCurrency, formatDate } from '@/shared/utils/format';
import {
  ORDER_SOURCE_LABEL,
  ORDER_STATUS_COLOR,
  ORDER_STATUS_FLOW,
  ORDER_STATUS_LABEL,
  PAYMENT_METHOD_LABEL,
  PAYMENT_STATUS_COLOR,
  PAYMENT_STATUS_LABEL,
} from '@/shared/utils/constants';
import type {
  Order,
  OrderItem,
  OrderSearchParams,
  OrderSource,
  OrderStatus,
  PaymentStatus,
} from '@/interfaces';

const cx = classNames.bind(styles);
const { Title, Text } = Typography;
const { RangePicker } = DatePicker;

const Orders: React.FC = () => {
  const [messageApi, contextHolder] = message.useMessage();
  const [filterStatus, setFilterStatus] = useState<OrderStatus | undefined>();
  const [filterSource, setFilterSource] = useState<'ONLINE' | 'POS' | undefined>();
  const [keyword, setKeyword] = useState('');
  const [dateRange, setDateRange] = useState<[string, string] | null>(null);
  const [drawerOrder, setDrawerOrder] = useState<Order | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [drawerLoading, setDrawerLoading] = useState(false);
  const [orderDetails, setOrderDetails] = useState<Record<string, Order>>({});
  const [expandedLoadingKeys, setExpandedLoadingKeys] = useState<string[]>([]);

  const {
    items,
    total,
    page,
    pageSize,
    loading,
    setParams,
    refetch,
  } = usePagedQuery<Order, OrderSearchParams>({
    fetcher: (params) => orderService.search(params),
    initialParams: { page: 1, pageSize: 10 },
    errorMessage: 'Lỗi khi tải đơn hàng',
  });

  const handleSearch = () => {
    setParams({
      page: 1,
      status: filterStatus,
      source: filterSource,
      keyword: keyword || undefined,
      fromDate: dateRange?.[0],
      toDate: dateRange?.[1],
    });
  };

  const handleResetFilter = () => {
    setFilterStatus(undefined);
    setFilterSource(undefined);
    setKeyword('');
    setDateRange(null);
    setParams({ page: 1 });
  };

  const fetchOrderDetail = async (orderCode: string) => {
    if (orderDetails[orderCode]) {
      return orderDetails[orderCode];
    }

    setExpandedLoadingKeys((prev) => (prev.includes(orderCode) ? prev : [...prev, orderCode]));
    try {
      const detail = await orderService.getByCode(orderCode);
      setOrderDetails((prev) => ({ ...prev, [orderCode]: detail }));
      return detail;
    } finally {
      setExpandedLoadingKeys((prev) => prev.filter((key) => key !== orderCode));
    }
  };

  const handleUpdateStatus = async (order: Order, newStatus: OrderStatus) => {
    try {
      await orderService.updateStatus({ orderCode: order.orderCode, status: newStatus });
      messageApi.success('Cập nhật trạng thái thành công');
      refetch();
      setOrderDetails((prev) => {
        if (!prev[order.orderCode]) {
          return prev;
        }

        return {
          ...prev,
          [order.orderCode]: {
            ...prev[order.orderCode],
            orderStatus: newStatus,
          },
        };
      });
    } catch (err) {
      messageApi.error((err as { message?: string }).message || 'Lỗi khi cập nhật');
    }
  };

  const handleOpenDetail = async (orderCode: string) => {
    setDrawerOpen(true);
    setDrawerLoading(true);

    try {
      const detail = await fetchOrderDetail(orderCode);
      setDrawerOrder(detail);
    } catch (err) {
      setDrawerOrder(null);
      setDrawerOpen(false);
      messageApi.error((err as { message?: string }).message || 'Lỗi khi tải chi tiết đơn hàng');
    } finally {
      setDrawerLoading(false);
    }
  };

  const columns: ColumnsType<Order> = [
    {
      title: 'Mã đơn',
      dataIndex: 'orderCode',
      key: 'orderCode',
      width: 170,
      render: (value: string) => <Text code>{value}</Text>,
    },
    {
      title: 'Khách hàng',
      key: 'customer',
      render: (_, record) => (
        <div>
          <div style={{ fontWeight: 480 }}>{record.customerName}</div>
          <div style={{ fontSize: 12, color: '#888' }}>{record.customerPhone}</div>
        </div>
      ),
    },
    {
      title: 'Tổng tiền',
      dataIndex: 'totalAmount',
      key: 'totalAmount',
      width: 130,
      align: 'right',
      render: (value: number) => (
        <span style={{ fontWeight: 600, color: '#198754' }}>{formatCurrency(value)}</span>
      ),
    },
    {
      title: 'Nguồn',
      dataIndex: 'orderSource',
      key: 'orderSource',
      width: 110,
      render: (value: OrderSource) => <Tag>{ORDER_SOURCE_LABEL[value]}</Tag>,
    },
    {
      title: 'Thanh toán',
      dataIndex: 'paymentStatus',
      key: 'paymentStatus',
      width: 140,
      render: (value: PaymentStatus) => (
        <Tag color={PAYMENT_STATUS_COLOR[value]}>{PAYMENT_STATUS_LABEL[value]}</Tag>
      ),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'orderStatus',
      key: 'orderStatus',
      width: 150,
      render: (status: OrderStatus, record) => {
        const nextSteps = ORDER_STATUS_FLOW[status];

        return (
          <Space size={4}>
            <Tag color={ORDER_STATUS_COLOR[status]}>{ORDER_STATUS_LABEL[status]}</Tag>
            {nextSteps.length > 0 && (
              <Dropdown
                menu={{
                  items: nextSteps.map((nextStatus) => ({
                    key: nextStatus,
                    label: ORDER_STATUS_LABEL[nextStatus],
                    danger: nextStatus === 'CANCELLED',
                  })),
                  onClick: ({ key }) => handleUpdateStatus(record, key as OrderStatus),
                }}
              >
                <Button size="small" icon={<DownOutlined />} />
              </Dropdown>
            )}
          </Space>
        );
      },
    },
    {
      title: 'Ngày tạo',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 130,
      render: (value: string) => formatDate(value),
    },
    {
      title: 'Chi tiết',
      key: 'action',
      width: 90,
      fixed: 'right',
      render: (_, record) => (
        <Button size="small" onClick={() => void handleOpenDetail(record.orderCode)}>
          Xem
        </Button>
      ),
    },
  ];

  const expandedRowRender = (order: Order) => {
    const detail = orderDetails[order.orderCode];
    const itemColumns: ColumnsType<OrderItem> = [
      {
        title: 'STT',
        key: 'index',
        width: 60,
        align: 'center',
        render: (_value, _record, index) => index + 1,
      },
      { title: 'Sản phẩm', dataIndex: 'productName', key: 'productName' },
      { title: 'SL', dataIndex: 'quantity', key: 'quantity', width: 60 },
      {
        title: 'Đơn giá',
        dataIndex: 'unitPrice',
        key: 'unitPrice',
        width: 120,
        render: (value: number) => formatCurrency(value),
      },
      {
        title: 'Thành tiền',
        dataIndex: 'lineTotal',
        key: 'lineTotal',
        width: 130,
        render: (value: number) => formatCurrency(value),
      },
    ];

    if (expandedLoadingKeys.includes(order.orderCode)) {
      return (
        <div className={cx('expandedPanel')}>
          <div className={cx('loadingBlock')}>
            <Spin />
          </div>
        </div>
      );
    }

    return (
      <div className={cx('expandedPanel')}>
        <Table<OrderItem>
          className={cx('detailTable')}
          columns={itemColumns}
          dataSource={detail?.items ?? []}
          rowKey="id"
          pagination={false}
          size="small"
        />
      </div>
    );
  };

  return (
    <MasterLayout>
      {contextHolder}
      <div className={cx('page')}>
        <div className={cx('pageHeader')}>
          <Title level={3}>Đơn hàng</Title>
        </div>

        <div className={cx('filterBar')}>
          <Input
            placeholder="Mã đơn / Khách hàng / SDT..."
            prefix={<SearchOutlined />}
            value={keyword}
            onChange={(e) => setKeyword(e.target.value)}
            style={{ width: 260 }}
            allowClear
          />
          <Select
            placeholder="Trạng thái"
            value={filterStatus}
            onChange={setFilterStatus}
            allowClear
            style={{ width: 200 }}
            options={Object.entries(ORDER_STATUS_LABEL).map(([key, label]) => ({
              value: key,
              label,
            }))}
          />
          <Select
            placeholder="Nguồn"
            value={filterSource}
            onChange={setFilterSource}
            allowClear
            style={{ width: 160 }}
            options={[
              { value: 'ONLINE', label: 'Online' },
              { value: 'POS', label: 'Tại quầy' },
            ]}
          />
          <RangePicker
            onChange={(_, strings) => setDateRange(strings[0] ? [strings[0], strings[1]] : null)}
            format="YYYY-MM-DD"
          />
          <Button type="primary" onClick={handleSearch}>Lọc</Button>
          <Button onClick={handleResetFilter}>Đặt lại</Button>
        </div>

        <div className={cx('tableShell')}>
          <Table<Order>
            className={cx('ordersTable')}
            columns={columns}
            dataSource={items}
            rowKey="id"
            loading={loading}
            scroll={{ x: 1200 }}
            expandable={{
              expandedRowRender,
              onExpand: (expanded, record) => {
                if (expanded) {
                  void fetchOrderDetail(record.orderCode);
                }
              },
            }}
            pagination={{
              current: page,
              pageSize,
              total,
              showSizeChanger: true,
              showTotal: (value) => `Tổng ${value} đơn hàng`,
              onChange: (nextPage, nextPageSize) => setParams({ page: nextPage, pageSize: nextPageSize }),
            }}
          />
        </div>

        <Drawer
          title={`Chi tiết đơn - ${drawerOrder?.orderCode ?? ''}`}
          open={drawerOpen}
          onClose={() => {
            setDrawerOpen(false);
            setDrawerOrder(null);
          }}
          width={560}
        >
          {drawerLoading && (
            <div className={cx('loadingBlock')}>
              <Spin />
            </div>
          )}

          {!drawerLoading && drawerOrder && (
            <div className={cx('drawerDetail')}>
              <div className={cx('detailRow')}>
                <span>Khách hàng:</span>
                <strong>{drawerOrder.customerName}</strong>
              </div>
              <div className={cx('detailRow')}>
                <span>Số điện thoại:</span>
                <span>{drawerOrder.customerPhone || '—'}</span>
              </div>
              <div className={cx('detailRow')}>
                <span>Địa chỉ:</span>
                <span>{drawerOrder.customerAddress || '—'}</span>
              </div>
              <div className={cx('detailRow')}>
                <span>Ghi chú:</span>
                <span>{drawerOrder.note || '—'}</span>
              </div>
              <div className={cx('detailRow')}>
                <span>Thanh toán:</span>
                <span>{PAYMENT_METHOD_LABEL[drawerOrder.paymentMethod]}</span>
              </div>
              <div className={cx('detailRow')}>
                <span>Tạm tính:</span>
                <span>{formatCurrency(drawerOrder.subtotalAmount)}</span>
              </div>
              <div className={cx('detailRow')}>
                <span>Giảm giá:</span>
                <span>{formatCurrency(drawerOrder.discountAmount)}</span>
              </div>
              <div className={cx('detailRow')}>
                <span>Phí ship:</span>
                <span>{formatCurrency(drawerOrder.shippingFee)}</span>
              </div>
              <div className={cx('detailRow', 'total')}>
                <span>Tổng cộng:</span>
                <strong>{formatCurrency(drawerOrder.totalAmount)}</strong>
              </div>

              <div className={cx('expandedPanel')} style={{ marginTop: 16 }}>
                <Table<OrderItem>
                  className={cx('detailTable')}
                  size="small"
                  dataSource={drawerOrder.items}
                  rowKey="id"
                  pagination={false}
                  columns={[
                    {
                      title: 'STT',
                      key: 'index',
                      width: 60,
                      align: 'center',
                      render: (_value, _record, index) => index + 1,
                    },
                    { title: 'Sản phẩm', dataIndex: 'productName' },
                    { title: 'SL', dataIndex: 'quantity', width: 50 },
                    {
                      title: 'Giá',
                      dataIndex: 'lineTotal',
                      render: (value: number) => formatCurrency(value),
                      width: 120,
                    },
                  ]}
                />
              </div>
            </div>
          )}
        </Drawer>
      </div>
    </MasterLayout>
  );
};

export default Orders;
