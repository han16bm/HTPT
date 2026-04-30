import React from 'react';
import { Card, Col, Row, Statistic, Tag, Typography, Empty } from 'antd';
import {
  ShoppingCartOutlined,
  DollarOutlined,
  RiseOutlined,
  TeamOutlined,
  AppstoreOutlined,
  HourglassOutlined,
  FileTextOutlined,
  TrophyOutlined,
} from '@ant-design/icons';
import classNames from 'classnames/bind';
import {
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  CartesianGrid,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  Legend,
} from 'recharts';
import styles from './Dashboard.module.scss';
import { MasterLayout } from '@/components/Layout';
import { dashboardService, type DashboardOverview } from '@/api';
import { useAsync } from '@/hooks';
import { formatCurrency, formatCompact, formatDate } from '@/shared/utils/format';
import { ORDER_STATUS_LABEL } from '@/shared/utils/constants';
import type { OrderStatus } from '@/interfaces';

const cx = classNames.bind(styles);
const { Title } = Typography;

const STATUS_COLORS: Record<OrderStatus, string> = {
  PENDING: '#ffc107',
  CONFIRMED: '#0d6efd',
  SHIPPING: '#0dcaf0',
  COMPLETED: '#198754',
  CANCELLED: '#dc3545',
};

const Dashboard: React.FC = () => {
  const { data: overview, loading } = useAsync<DashboardOverview>({
    fetcher: () => dashboardService.getOverview(),
    errorMessage: 'Lỗi tải thống kê',
  });

  const revenueData = (overview?.revenueChart ?? []).slice(-7).map((row) => ({
    ...row,
    label: formatDate(row.date),
  }));

  const topProducts = (overview?.topProducts ?? []).map((row) => ({
    productName: row.name,
    sold: row.soldQuantity,
  }));

  const orderStatusData = [
    { status: 'PENDING' as OrderStatus, count: overview?.orderStatus.pending ?? 0 },
    { status: 'CONFIRMED' as OrderStatus, count: overview?.orderStatus.confirmed ?? 0 },
    { status: 'SHIPPING' as OrderStatus, count: overview?.orderStatus.shipped ?? 0 },
    { status: 'COMPLETED' as OrderStatus, count: overview?.orderStatus.delivered ?? 0 },
    { status: 'CANCELLED' as OrderStatus, count: overview?.orderStatus.cancelled ?? 0 },
  ].filter((row) => row.count > 0);

  return (
    <MasterLayout>
      <div className={cx('dashboard')}>
        <Title level={3} className={cx('title')}>
          Tổng quan
        </Title>

        <Row gutter={[16, 16]} className={cx('statsRow')}>
          <Col xs={24} sm={12} lg={6}>
            <Card loading={loading} className={cx('statCard')}>
              <Statistic
                title="Đơn hôm nay"
                value={overview?.today.newOrders ?? 0}
                prefix={<ShoppingCartOutlined style={{ color: '#0d6efd' }} />}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card loading={loading} className={cx('statCard')}>
              <Statistic
                title="Doanh thu hôm nay"
                value={overview?.today.revenue ?? 0}
                formatter={(v) => formatCurrency(Number(v))}
                prefix={<DollarOutlined style={{ color: '#198754' }} />}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card loading={loading} className={cx('statCard')}>
              <Statistic
                title="Lợi nhuận hôm nay"
                value={overview?.today.profit ?? 0}
                formatter={(v) => formatCurrency(Number(v))}
                prefix={<RiseOutlined style={{ color: '#0dcaf0' }} />}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={6}>
            <Card loading={loading} className={cx('statCard')}>
              <Statistic
                title="Khách hàng"
                value={overview?.month.totalCustomers ?? 0}
                prefix={<TeamOutlined style={{ color: '#6f42c1' }} />}
              />
            </Card>
          </Col>
        </Row>

        <Row gutter={[16, 16]} className={cx('statsRow')}>
          <Col xs={24} sm={12} lg={8}>
            <Card loading={loading} className={cx('statCard')}>
              <Statistic
                title="Tổng đơn hàng"
                value={overview?.month.totalOrders ?? 0}
                prefix={<FileTextOutlined style={{ color: '#0d6efd' }} />}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={8}>
            <Card loading={loading} className={cx('statCard')}>
              <Statistic
                title="Đơn chờ xử lý"
                value={overview?.orderStatus.pending ?? 0}
                prefix={<HourglassOutlined style={{ color: '#ffc107' }} />}
                valueStyle={{ color: '#ffc107' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} lg={8}>
            <Card loading={loading} className={cx('statCard')}>
              <Statistic
                title="Sản phẩm đang bán"
                value={overview?.month.totalProducts ?? 0}
                prefix={<AppstoreOutlined style={{ color: '#0d6efd' }} />}
                valueStyle={{ color: '#0d6efd' }}
              />
            </Card>
          </Col>
        </Row>

        <Row gutter={[16, 16]} className={cx('chartsRow')}>
          <Col xs={24} lg={16}>
            <Card
              title="Doanh thu 7 ngày gần nhất"
              loading={loading}
              className={cx('chartCard')}
            >
              {revenueData.length === 0 ? (
                <Empty description="Chưa có dữ liệu" />
              ) : (
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={revenueData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="label" />
                    <YAxis tickFormatter={formatCompact} />
                    <Tooltip formatter={(value) => formatCurrency(Number(value ?? 0))} />
                    <Line
                      type="monotone"
                      dataKey="revenue"
                      stroke="#198754"
                      strokeWidth={2}
                      name="Doanh thu"
                    />
                  </LineChart>
                </ResponsiveContainer>
              )}
            </Card>
          </Col>

          <Col xs={24} lg={8}>
            <Card
              title="Trạng thái đơn hàng"
              loading={loading}
              className={cx('chartCard')}
            >
              {orderStatusData.length === 0 ? (
                <Empty description="Chưa có dữ liệu" />
              ) : (
                <ResponsiveContainer width="100%" height={300}>
                  <PieChart>
                    <Pie
                      data={orderStatusData.map((row) => ({
                        name: ORDER_STATUS_LABEL[row.status],
                        value: row.count,
                        status: row.status,
                      }))}
                      dataKey="value"
                      nameKey="name"
                      outerRadius={90}
                      label
                    >
                      {orderStatusData.map((row) => (
                        <Cell key={row.status} fill={STATUS_COLORS[row.status]} />
                      ))}
                    </Pie>
                    <Tooltip />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              )}
            </Card>
          </Col>
        </Row>

        <Row gutter={[16, 16]} className={cx('chartsRow')}>
          <Col xs={24}>
            <Card
              title={
                <span>
                  <TrophyOutlined style={{ color: '#ffc107', marginRight: 8 }} />
                  Top sản phẩm bán chạy
                </span>
              }
              loading={loading}
              className={cx('chartCard')}
            >
              {topProducts.length === 0 ? (
                <Empty description="Chưa có dữ liệu" />
              ) : (
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={topProducts}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="productName" />
                    <YAxis />
                    <Tooltip />
                    <Bar dataKey="sold" fill="#198754" name="Đã bán" />
                  </BarChart>
                </ResponsiveContainer>
              )}
            </Card>
          </Col>
        </Row>

        <div className={cx('quickInfo')}>
          <Tag color="green">Tổng doanh thu: {formatCurrency(overview?.month.totalRevenue ?? 0)}</Tag>
        </div>
      </div>
    </MasterLayout>
  );
};

export default Dashboard;
