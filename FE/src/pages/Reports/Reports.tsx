import React, { useState } from 'react';
import {
  Button, Card, Col, Empty, Row, Select, Space, Table, Tabs, Typography,
  DatePicker, Statistic,
} from 'antd';
import {
  BarChart, Bar, ComposedChart, Line, PieChart, Pie, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer,
} from 'recharts';
import { BarChartOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import classNames from 'classnames/bind';
import styles from './Reports.module.scss';
import { MasterLayout } from '@/components/Layout';
import { reportService } from '@/api';
import { useAsync } from '@/hooks';
import { formatCurrency, formatCompact } from '@/shared/utils/format';
import { ORDER_STATUS_LABEL } from '@/shared/utils/constants';
import type { RevenueReportRow, TopProductReportRow, OrderSummaryReport } from '@/interfaces';
import type { RevenueReportParams } from '@/api';

const cx = classNames.bind(styles);
const { Title } = Typography;
const { RangePicker } = DatePicker;

const COLORS = ['#198754', '#0d6efd', '#ffc107', '#dc3545', '#6f42c1'];

const defaultFrom = dayjs().subtract(29, 'day').format('YYYY-MM-DD');
const defaultTo = dayjs().format('YYYY-MM-DD');

const Reports: React.FC = () => {
  const [revenueParams, setRevenueParams] = useState<RevenueReportParams>({
    fromDate: defaultFrom,
    toDate: defaultTo,
    groupBy: 'day',
  });
  const [topParams, setTopParams] = useState({ fromDate: defaultFrom, toDate: defaultTo, limit: 10 });
  const [summaryParams, setSummaryParams] = useState({ fromDate: defaultFrom, toDate: defaultTo });

  const { data: revenueData, loading: revLoading, refetch: refetchRev } = useAsync<RevenueReportRow[]>({
    fetcher: () => reportService.getRevenue(revenueParams),
    errorMessage: 'Lỗi tải báo cáo doanh thu',
  });

  const { data: topData, loading: topLoading, refetch: refetchTop } = useAsync<TopProductReportRow[]>({
    fetcher: () => reportService.getTopProducts(topParams),
    errorMessage: 'Lỗi tải top sản phẩm',
  });

  const { data: summaryData, loading: sumLoading, refetch: refetchSum } = useAsync<OrderSummaryReport>({
    fetcher: () => reportService.getOrderSummary(summaryParams),
    errorMessage: 'Lỗi tải tổng hợp đơn',
  });

  const totalRevenue = (revenueData ?? []).reduce((s, r) => s + r.revenue, 0);
  const totalOrders = (revenueData ?? []).reduce((s, r) => s + r.orders, 0);

  return (
    <MasterLayout>
      <div className={cx('page')}>
        <Title level={3}><BarChartOutlined /> Báo cáo & Thống kê</Title>

        <Tabs
          items={[
            {
              key: 'revenue',
              label: 'Doanh thu',
              children: (
                <div>
                  <div className={cx('filterBar')}>
                    <RangePicker
                      defaultValue={[dayjs(defaultFrom), dayjs(defaultTo)]}
                      onChange={(_, strings) => {
                        if (strings[0] && strings[1]) {
                          setRevenueParams((p) => ({ ...p, fromDate: strings[0], toDate: strings[1] }));
                        }
                      }}
                      format="YYYY-MM-DD"
                    />
                    <Select
                      value={revenueParams.groupBy}
                      onChange={(v) => setRevenueParams((p) => ({ ...p, groupBy: v }))}
                      style={{ width: 140 }}
                      options={[
                        { value: 'day', label: 'Theo ngày' },
                        { value: 'week', label: 'Theo tuần' },
                        { value: 'month', label: 'Theo tháng' },
                      ]}
                    />
                    <Button type="primary" onClick={refetchRev}>Xem báo cáo</Button>
                  </div>

                  <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
                    <Col xs={12} lg={6}>
                      <Card>
                        <Statistic
                          loading={revLoading}
                          title="Tổng doanh thu"
                          value={totalRevenue}
                          formatter={(v) => formatCurrency(Number(v))}
                          valueStyle={{ color: '#198754' }}
                        />
                      </Card>
                    </Col>
                    <Col xs={12} lg={6}>
                      <Card>
                        <Statistic
                          loading={revLoading}
                          title="Tổng đơn hàng"
                          value={totalOrders}
                        />
                      </Card>
                    </Col>
                  </Row>

                  <Card title="Doanh thu theo thời gian" loading={revLoading}>
                    {(revenueData ?? []).length === 0 ? (
                      <Empty description="Chưa có dữ liệu" />
                    ) : (
                      <ResponsiveContainer width="100%" height={320}>
                        <ComposedChart data={revenueData ?? []}>
                          <CartesianGrid strokeDasharray="3 3" />
                          <XAxis dataKey="period" />
                          <YAxis yAxisId="left" tickFormatter={formatCompact} />
                          <YAxis yAxisId="right" orientation="right" allowDecimals={false} />
                          <Tooltip formatter={(v: number, name: string) => name === 'Doanh thu' ? formatCurrency(v) : v} />
                          <Legend />
                          <Bar dataKey="revenue" fill="#198754" name="Doanh thu" yAxisId="left" radius={[6, 6, 0, 0]} barSize={28} />
                          <Line type="monotone" dataKey="orders" stroke="#0d6efd" strokeWidth={2} name="Số đơn" yAxisId="right" />
                        </ComposedChart>
                      </ResponsiveContainer>
                    )}
                  </Card>
                </div>
              ),
            },
            {
              key: 'top-products',
              label: 'Top sản phẩm',
              children: (
                <div>
                  <div className={cx('filterBar')}>
                    <RangePicker
                      defaultValue={[dayjs(defaultFrom), dayjs(defaultTo)]}
                      onChange={(_, strings) => {
                        if (strings[0] && strings[1]) {
                          setTopParams((p) => ({ ...p, fromDate: strings[0], toDate: strings[1] }));
                        }
                      }}
                      format="YYYY-MM-DD"
                    />
                    <Select
                      value={topParams.limit}
                      onChange={(v) => setTopParams((p) => ({ ...p, limit: v }))}
                      style={{ width: 120 }}
                      options={[
                        { value: 5, label: 'Top 5' },
                        { value: 10, label: 'Top 10' },
                        { value: 20, label: 'Top 20' },
                      ]}
                    />
                    <Button type="primary" onClick={refetchTop}>Xem</Button>
                  </div>

                  <Row gutter={[16, 16]}>
                    <Col xs={24} lg={14}>
                      <Card title="Biểu đồ sản phẩm bán chạy" loading={topLoading}>
                        {(topData ?? []).length === 0 ? (
                          <Empty description="Chưa có dữ liệu" />
                        ) : (
                          <ResponsiveContainer width="100%" height={360}>
                            <BarChart data={topData ?? []} layout="vertical">
                              <CartesianGrid strokeDasharray="3 3" />
                              <XAxis type="number" />
                              <YAxis dataKey="productName" type="category" width={150} />
                              <Tooltip />
                              <Bar dataKey="sold" fill="#198754" name="Đã bán" />
                            </BarChart>
                          </ResponsiveContainer>
                        )}
                      </Card>
                    </Col>
                    <Col xs={24} lg={10}>
                      <Card title="Bảng top sản phẩm" loading={topLoading}>
                        <Table
                          size="small"
                          dataSource={topData ?? []}
                          rowKey="productId"
                          pagination={false}
                          columns={[
                            { title: '#', render: (_, __, i) => i + 1, width: 40 },
                            { title: 'Sản phẩm', dataIndex: 'productName' },
                            { title: 'Bán', dataIndex: 'sold', width: 60, align: 'right' },
                            {
                              title: 'Doanh thu',
                              dataIndex: 'revenue',
                              width: 120,
                              align: 'right',
                              render: (v) => formatCurrency(v),
                            },
                          ]}
                        />
                      </Card>
                    </Col>
                  </Row>
                </div>
              ),
            },
            {
              key: 'order-summary',
              label: 'Tổng hợp đơn',
              children: (
                <div>
                  <div className={cx('filterBar')}>
                    <RangePicker
                      defaultValue={[dayjs(defaultFrom), dayjs(defaultTo)]}
                      onChange={(_, strings) => {
                        if (strings[0] && strings[1]) {
                          setSummaryParams({ fromDate: strings[0], toDate: strings[1] });
                        }
                      }}
                      format="YYYY-MM-DD"
                    />
                    <Button type="primary" onClick={refetchSum}>Xem</Button>
                  </div>

                  {summaryData && (
                    <Row gutter={[16, 16]}>
                      <Col xs={24} lg={10}>
                        <Card title="Phân bố đơn theo trạng thái" loading={sumLoading}>
                          <ResponsiveContainer width="100%" height={300}>
                            <PieChart>
                              <Pie
                                data={(summaryData.byStatus ?? []).map((r) => ({
                                  name: ORDER_STATUS_LABEL[r.status],
                                  value: r.count,
                                }))}
                                dataKey="value"
                                nameKey="name"
                                outerRadius={100}
                                label
                              >
                                {(summaryData.byStatus ?? []).map((_, i) => (
                                  <Cell key={i} fill={COLORS[i % COLORS.length]} />
                                ))}
                              </Pie>
                              <Tooltip />
                              <Legend />
                            </PieChart>
                          </ResponsiveContainer>
                        </Card>
                      </Col>
                      <Col xs={24} lg={14}>
                        <Card title="Chi tiết theo trạng thái" loading={sumLoading}>
                          <Table
                            size="small"
                            dataSource={summaryData.byStatus ?? []}
                            rowKey="status"
                            pagination={false}
                            columns={[
                              {
                                title: 'Trạng thái',
                                dataIndex: 'status',
                                render: (s) => ORDER_STATUS_LABEL[s],
                              },
                              { title: 'Số đơn', dataIndex: 'count', width: 80, align: 'right' },
                              {
                                title: 'Doanh thu',
                                dataIndex: 'revenue',
                                width: 140,
                                align: 'right',
                                render: (v) => formatCurrency(v),
                              },
                            ]}
                            summary={(data) => {
                              const total = data.reduce((s, r) => s + r.count, 0);
                              const totalRev = data.reduce((s, r) => s + r.revenue, 0);
                              return (
                                <Table.Summary.Row>
                                  <Table.Summary.Cell index={0}><strong>Tổng cộng</strong></Table.Summary.Cell>
                                  <Table.Summary.Cell index={1} align="right"><strong>{total}</strong></Table.Summary.Cell>
                                  <Table.Summary.Cell index={2} align="right"><strong>{formatCurrency(totalRev)}</strong></Table.Summary.Cell>
                                </Table.Summary.Row>
                              );
                            }}
                          />
                          <Statistic
                            style={{ marginTop: 16 }}
                            title="Tổng số đơn"
                            value={summaryData.totalOrders}
                          />
                        </Card>
                      </Col>
                    </Row>
                  )}
                </div>
              ),
            },
          ]}
        />
      </div>
    </MasterLayout>
  );
};

export default Reports;
