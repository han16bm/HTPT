import React, { useState } from 'react';
import {
  Button, Form, InputNumber, Modal, Select, Space, Table,
  Tabs, Tag, Typography, message, Input,
} from 'antd';
import { PlusOutlined, InboxOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import classNames from 'classnames/bind';
import styles from './Inventory.module.scss';
import { MasterLayout } from '@/components/Layout';
import { inventoryService, productService } from '@/api';
import { usePagedQuery, useAsync } from '@/hooks';
import { formatCurrency, formatDate } from '@/shared/utils/format';
import { INVENTORY_TYPE_LABEL, LOW_STOCK_THRESHOLD } from '@/shared/utils/constants';
import type {
  InventoryImportRequest, InventoryTransaction, InventoryTransactionType, Product,
} from '@/interfaces';
import type { InventoryHistoryParams } from '@/api';

const cx = classNames.bind(styles);
const { Title } = Typography;

const INVENTORY_FILTER_OPTIONS: Array<{ value: InventoryTransactionType; label: string }> = [
  { value: 'IMPORT', label: 'Nhập kho' },
  { value: 'SALE', label: 'Bán hàng' },
  { value: 'RETURN', label: 'Trả hàng' },
];

const getInventoryTypeMeta = (transaction: InventoryTransaction) => {
  const type = transaction.transactionType;

  if (type === 'SALE' || (type === 'EXPORT' && transaction.note?.includes('Đơn'))) {
    return { label: 'Bán hàng', color: 'orange' as const, sign: '-' as const };
  }

  if (type === 'IMPORT') {
    return { label: INVENTORY_TYPE_LABEL.IMPORT, color: 'green' as const, sign: '+' as const };
  }

  if (type === 'RETURN') {
    return { label: INVENTORY_TYPE_LABEL.RETURN, color: 'blue' as const, sign: '+' as const };
  }

  if (type === 'ADJUST' || type === 'ADJUSTMENT') {
    return {
      label: 'Điều chỉnh',
      color: 'geekblue' as const,
      sign: transaction.quantity >= 0 ? '+' as const : '-' as const,
    };
  }

  if (type === 'EXPORT') {
    return { label: INVENTORY_TYPE_LABEL.EXPORT, color: 'volcano' as const, sign: '-' as const };
  }

  return {
    label: transaction.transactionType,
    color: 'default' as const,
    sign: transaction.quantity >= 0 ? '+' as const : '-' as const,
  };
};

const Inventory: React.FC = () => {
  const [activeTab, setActiveTab] = useState('stock');
  const [importModalOpen, setImportModalOpen] = useState(false);
  const [filterType, setFilterType] = useState<InventoryTransactionType | undefined>();
  const [form] = Form.useForm<InventoryImportRequest>();

  const { data: productsData, loading: stockLoading } = useAsync<{ items: Product[] }>({
    fetcher: () => productService.search({ pageSize: 200 }) as Promise<{ items: Product[] }>,
    errorMessage: 'Lỗi tải danh sách tồn kho',
  });
  const stockList = productsData?.items ?? [];

  const {
    items: history, total, page, pageSize, loading: histLoading, setParams, refetch,
  } = usePagedQuery<InventoryTransaction, InventoryHistoryParams>({
    fetcher: (p) => inventoryService.getHistory(p),
    initialParams: { page: 1, pageSize: 10 },
    errorMessage: 'Lỗi tải lịch sử kho',
  });

  const { data: allProducts } = useAsync<{ items: Product[] }>({
    fetcher: () => productService.search({ pageSize: 200 }) as Promise<{ items: Product[] }>,
    errorMessage: '',
  });

  const handleImport = async (values: InventoryImportRequest) => {
    try {
      await inventoryService.importStock(values);
      message.success('Nhập hàng thành công!');
      setImportModalOpen(false);
      form.resetFields();
      refetch();
    } catch (err) {
      message.error((err as { message?: string }).message || 'Lỗi khi nhập hàng');
    }
  };

  const stockCols: ColumnsType<Product> = [
    { title: 'Mã SP', dataIndex: 'productCode', key: 'productCode', width: 100 },
    { title: 'Tên sản phẩm', dataIndex: 'name', key: 'name' },
    { title: 'Danh mục', dataIndex: 'categoryName', key: 'categoryName', width: 150 },
    {
      title: 'Tồn kho',
      dataIndex: 'stockQuantity',
      key: 'stockQuantity',
      width: 120,
      sorter: (a, b) => a.stockQuantity - b.stockQuantity,
      render: (v: number) => (
        <Tag color={v <= 0 ? 'red' : v <= LOW_STOCK_THRESHOLD ? 'orange' : 'green'}>
          {v}
        </Tag>
      ),
    },
    {
      title: 'Đã bán',
      dataIndex: 'soldQuantity',
      key: 'soldQuantity',
      width: 90,
      align: 'right',
    },
    {
      title: 'Giá bán',
      dataIndex: 'salePrice',
      key: 'salePrice',
      width: 130,
      render: (v) => formatCurrency(v),
    },
  ];

  const historyCols: ColumnsType<InventoryTransaction> = [
    { title: 'Sản phẩm', dataIndex: 'productName', key: 'productName' },
    {
      title: 'Loại',
      dataIndex: 'transactionType',
      key: 'type',
      width: 120,
      render: (_: InventoryTransactionType, record) => {
        const meta = getInventoryTypeMeta(record);
        return <Tag color={meta.color}>{meta.label}</Tag>;
      },
    },
    {
      title: 'Số lượng',
      dataIndex: 'quantity',
      key: 'quantity',
      width: 100,
      align: 'right',
      render: (v: number, r) => {
        const meta = getInventoryTypeMeta(r);
        return (
          <span style={{ color: meta.sign === '+' ? '#198754' : '#dc3545', fontWeight: 600 }}>
            {meta.sign}{Math.abs(v)}
          </span>
        );
      },
    },
    {
      title: 'Đơn giá nhập',
      dataIndex: 'unitCost',
      key: 'unitCost',
      width: 130,
      render: (v?: number) => (v ? formatCurrency(v) : '—'),
    },
    { title: 'Ghi chú', dataIndex: 'note', key: 'note', render: (v?: string) => v || '—' },
    { title: 'Người tạo', dataIndex: 'createdBy', key: 'createdBy', width: 130 },
    {
      title: 'Thời gian',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 140,
      render: (v) => formatDate(v),
    },
  ];

  return (
    <MasterLayout>
      <div className={cx('page')}>
        <div className={cx('pageHeader')}>
          <Title level={3}><InboxOutlined /> Quản lý kho</Title>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => setImportModalOpen(true)}
          >
            Nhập hàng
          </Button>
        </div>

        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={[
            {
              key: 'stock',
              label: 'Tồn kho hiện tại',
              children: (
                <Table<Product>
                  className={cx('stockTable')}
                  columns={stockCols}
                  dataSource={stockList}
                  rowKey="id"
                  loading={stockLoading}
                  scroll={{ x: 900 }}
                  pagination={{ pageSize: 20, showTotal: (t) => `${t} sản phẩm` }}
                  rowClassName={(r) =>
                    r.stockQuantity <= 0 ? 'row-out-of-stock' : r.stockQuantity <= LOW_STOCK_THRESHOLD ? 'row-low-stock' : ''
                  }
                />
              ),
            },
            {
              key: 'history',
              label: 'Lịch sử nhập/xuất',
              children: (
                <>
                  <div className={cx('filterBar')}>
                    <Select
                      placeholder="Loại giao dịch"
                      value={filterType}
                      onChange={(v) => { setFilterType(v); setParams({ transactionType: v, page: 1 }); }}
                      allowClear
                      style={{ width: 180 }}
                      options={INVENTORY_FILTER_OPTIONS}
                    />
                  </div>
                  <Table<InventoryTransaction>
                    columns={historyCols}
                    dataSource={history}
                    rowKey="id"
                    loading={histLoading}
                    scroll={{ x: 1000 }}
                    pagination={{
                      current: page,
                      pageSize,
                      total,
                      showSizeChanger: true,
                      showTotal: (t) => `Tổng ${t} giao dịch`,
                      onChange: (p, ps) => setParams({ page: p, pageSize: ps }),
                    }}
                  />
                </>
              ),
            },
          ]}
        />

        <Modal
          title="Nhập hàng vào kho"
          open={importModalOpen}
          onCancel={() => { setImportModalOpen(false); form.resetFields(); }}
          onOk={() => form.submit()}
          okText="Nhập hàng"
          cancelText="Hủy"
          destroyOnClose
        >
          <Form<InventoryImportRequest>
            form={form}
            layout="vertical"
            onFinish={handleImport}
          >
            <Form.Item
              name="productId"
              label="Sản phẩm"
              rules={[{ required: true, message: 'Vui lòng chọn sản phẩm!' }]}
            >
              <Select
                showSearch
                placeholder="Chọn sản phẩm..."
                filterOption={(input, opt) =>
                  (opt?.label as string ?? '').toLowerCase().includes(input.toLowerCase())
                }
                options={(allProducts?.items ?? []).map((p) => ({
                  value: p.id,
                  label: `${p.productCode} — ${p.name} (Tồn: ${p.stockQuantity})`,
                }))}
              />
            </Form.Item>

            <Space style={{ display: 'flex' }} size="large">
              <Form.Item
                name="quantity"
                label="Số lượng nhập"
                rules={[{ required: true, message: 'Bắt buộc' }]}
                style={{ flex: 1 }}
              >
                <InputNumber style={{ width: '100%' }} min={1} />
              </Form.Item>
              <Form.Item
                name="unitCost"
                label="Đơn giá nhập (VNĐ)"
                rules={[{ required: true, message: 'Bắt buộc' }]}
                style={{ flex: 1 }}
              >
                <InputNumber<number>
                  style={{ width: '100%' }}
                  min={0}
                  formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                  parser={(v) => Number((v || '').replace(/,/g, ''))}
                />
              </Form.Item>
            </Space>

            <Form.Item name="note" label="Ghi chú">
              <Input.TextArea rows={2} placeholder="Nhập lô cá tháng 4..." />
            </Form.Item>
          </Form>
        </Modal>
      </div>
    </MasterLayout>
  );
};

export default Inventory;
