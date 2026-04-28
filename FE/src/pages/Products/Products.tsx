import React, { useState } from 'react';
import {
  Table, Button, Space, Input, Select, message,
  Popconfirm, Tag, Image, Typography,
} from 'antd';
import {
  PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined, StarFilled,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import classNames from 'classnames/bind';
import styles from './Products.module.scss';
import { MasterLayout } from '@/components/Layout';
import { productService, categoryService } from '@/api';
import { usePagedQuery, useAsync } from '@/hooks';
import { formatCurrency, formatNumber } from '@/shared/utils/format';
import { LOW_STOCK_THRESHOLD } from '@/shared/utils/constants';
import { IMAGE_PLACEHOLDER, resolveProductImageUrl } from '@/shared/utils/image';
import type {
  Category, Product, ProductSearchParams, ProductUpsertRequest,
} from '@/interfaces';
import ProductFormModal from './ProductFormModal';

const cx = classNames.bind(styles);
const { Title } = Typography;

const Products: React.FC = () => {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editing, setEditing] = useState<Product | null>(null);
  const [editLoadingId, setEditLoadingId] = useState<number | null>(null);
  const [searchInput, setSearchInput] = useState('');

  const {
    items: products, total, page, pageSize, loading, setParams, refetch,
  } = usePagedQuery<Product, ProductSearchParams>({
    fetcher: (p) => productService.search(p),
    initialParams: { page: 1, pageSize: 10 },
    errorMessage: 'Lỗi khi tải danh sách sản phẩm',
  });

  const { data: categories } = useAsync<Category[]>({
    fetcher: () => categoryService.getAll(),
    errorMessage: 'Lỗi tải danh mục',
  });

  const flatCategories: Category[] = (categories ?? []).flatMap((c) => [
    c, ...(c.children ?? []),
  ]);

  const handleDelete = async (id: number) => {
    try {
      await productService.delete(id);
      message.success('Xóa sản phẩm thành công');
      refetch();
    } catch (err) {
      message.error((err as Error)?.message || 'Lỗi khi xóa sản phẩm');
    }
  };

  const handleSubmit = async (values: ProductUpsertRequest) => {
    try {
      await productService.upsert(values);
      message.success(editing ? 'Cập nhật thành công' : 'Thêm sản phẩm thành công');
      setIsModalOpen(false);
      setEditing(null);
      refetch();
    } catch (err) {
      message.error((err as Error)?.message || 'Lỗi khi lưu sản phẩm');
    }
  };

  const handleEdit = async (record: Product) => {
    setEditLoadingId(record.id);
    try {
      const detail = await productService.getById(record.id);
      setEditing(detail);
      setIsModalOpen(true);
    } catch (err) {
      message.error((err as Error)?.message || 'Lỗi khi tải chi tiết sản phẩm');
    } finally {
      setEditLoadingId(null);
    }
  };

  const columns: ColumnsType<Product> = [
    { title: 'Mã SP', dataIndex: 'productCode', key: 'productCode', width: 100 },
    {
      title: 'Ảnh', dataIndex: 'imageUrl', key: 'imageUrl', width: 80,
      render: (url?: string) => {
        const resolvedUrl = resolveProductImageUrl(url);
        return resolvedUrl ? (
          <Image
            src={resolvedUrl}
            fallback={IMAGE_PLACEHOLDER}
            alt=""
            width={48}
            height={48}
            style={{ objectFit: 'cover', borderRadius: 4 }}
          />
        ) : (
          <div style={{ width: 48, height: 48, background: '#f1f5f9', borderRadius: 4 }} />
        );
      },
    },
    {
      title: 'Tên sản phẩm', dataIndex: 'name', key: 'name',
      render: (name: string, record) => (
        <Space direction="vertical" size={0}>
          <span style={{ fontWeight: 500 }}>{name}</span>
          {record.isFeatured && (
            <Tag color="gold" icon={<StarFilled />} style={{ marginTop: 2 }}>Nổi bật</Tag>
          )}
        </Space>
      ),
    },
    { title: 'Danh mục', dataIndex: 'categoryName', key: 'categoryName', width: 140 },
    {
      title: 'Giá bán', dataIndex: 'salePrice', key: 'salePrice', width: 130, align: 'right',
      render: (v: number) => formatCurrency(v),
    },
    {
      title: 'Tồn kho', dataIndex: 'stockQuantity', key: 'stockQuantity', width: 100, align: 'right',
      render: (v: number) => (
        <Tag color={v <= 0 ? 'red' : v <= LOW_STOCK_THRESHOLD ? 'orange' : 'green'}>
          {formatNumber(v)}
        </Tag>
      ),
    },
    {
      title: 'Đã bán', dataIndex: 'soldQuantity', key: 'soldQuantity', width: 100, align: 'right',
      render: (v: number) => formatNumber(v),
    },
    {
      title: 'Hành động', key: 'action', width: 110, fixed: 'right',
      render: (_, record) => (
        <Space size="small" className={cx('tableActions')}>
          <Button type="primary" size="small" icon={<EditOutlined />}
            loading={editLoadingId === record.id}
            onClick={() => void handleEdit(record)} />
          <Popconfirm title="Xóa sản phẩm này?" onConfirm={() => handleDelete(record.id)}
            okText="Xóa" cancelText="Hủy">
            <Button danger size="small" icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <MasterLayout>
      <div className={cx('productsPage')}>
        <div className={cx('pageHeader')}>
          <Title level={3} className={cx('pageTitle')}>Sản phẩm</Title>
          <Space>
            <Input
              placeholder="Tìm theo tên..."
              prefix={<SearchOutlined />}
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              onPressEnter={() => setParams({ search: searchInput, page: 1 })}
              allowClear
              onClear={() => { setSearchInput(''); setParams({ search: undefined, page: 1 }); }}
              style={{ width: 260 }}
            />
            <Select
              placeholder="Lọc danh mục"
              allowClear
              style={{ width: 180 }}
              onChange={(val) => setParams({ categoryId: val, page: 1 })}
              options={flatCategories.map((c) => ({ value: c.id, label: c.name }))}
            />
            <Button type="primary" icon={<PlusOutlined />}
              onClick={() => { setEditing(null); setIsModalOpen(true); }}>
              Thêm sản phẩm
            </Button>
          </Space>
        </div>

        <Table<Product>
          columns={columns}
          dataSource={products}
          rowKey="id"
          loading={loading}
          scroll={{ x: 1100 }}
          pagination={{
            current: page, pageSize, total, showSizeChanger: true,
            showTotal: (t) => `Tổng ${t} sản phẩm`,
            onChange: (p, ps) => setParams({ page: p, pageSize: ps }),
          }}
        />

        <ProductFormModal
          open={isModalOpen}
          editing={editing}
          categories={flatCategories}
          onCancel={() => setIsModalOpen(false)}
          onSubmit={handleSubmit}
        />
      </div>
    </MasterLayout>
  );
};

export default Products;
