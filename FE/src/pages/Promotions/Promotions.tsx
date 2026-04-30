import React, { useEffect, useState } from 'react';
import {
  Button, Form, Input, InputNumber, Modal,
  Popconfirm, Select, Space, Table, Tag, Typography, message,
} from 'antd';
import { EditOutlined, DeleteOutlined, PlusOutlined, GiftOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import classNames from 'classnames/bind';
import styles from './Promotions.module.scss';
import { MasterLayout } from '@/components/Layout';
import { promotionService } from '@/api';
import { usePagedQuery } from '@/hooks';
import { formatCurrency, formatDate } from '@/shared/utils/format';
import type { Promotion, PromotionUpsertRequest } from '@/interfaces';
import type { PromotionSearchParams } from '@/api';

const cx = classNames.bind(styles);
const { Title } = Typography;

const Promotions: React.FC = () => {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editing, setEditing] = useState<Promotion | null>(null);
  const [form] = Form.useForm<PromotionUpsertRequest>();

  const {
    items, total, page, pageSize, loading, setParams, refetch,
  } = usePagedQuery<Promotion, PromotionSearchParams>({
    fetcher: (p) => promotionService.search(p),
    initialParams: { page: 1, pageSize: 10 },
    errorMessage: 'Lỗi khi tải khuyến mãi',
  });

  useEffect(() => {
    if (editing) {
      form.setFieldsValue({
        ...editing,
        startAt: editing.startAt,
        endAt: editing.endAt,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ status: 1, discountType: 'PERCENT', minOrderValue: 0 });
    }
  }, [editing, form]);

  const handleAdd = () => { setEditing(null); setIsModalOpen(true); };
  const handleEdit = (r: Promotion) => { setEditing(r); setIsModalOpen(true); };

  const handleDelete = async (id: number) => {
    try {
      await promotionService.delete(id);
      message.success('Xóa khuyến mãi thành công');
      refetch();
    } catch (err) {
      message.error((err as { message?: string }).message || 'Lỗi khi xóa');
    }
  };

  const handleSubmit = async (values: PromotionUpsertRequest & { dateRange?: [dayjs.Dayjs, dayjs.Dayjs] }) => {
    try {
      const payload: PromotionUpsertRequest = {
        ...values,
        id: editing?.id,
      };
      await promotionService.upsert(payload);
      message.success(editing ? 'Cập nhật thành công' : 'Thêm khuyến mãi thành công');
      setIsModalOpen(false);
      refetch();
    } catch (err) {
      message.error((err as { message?: string }).message || 'Lỗi khi lưu');
    }
  };

  const now = dayjs();

  const columns: ColumnsType<Promotion> = [
    {
      title: 'Mã',
      dataIndex: 'promoCode',
      key: 'promoCode',
      width: 120,
      render: (v: string) => <Tag color="blue"><GiftOutlined /> {v}</Tag>,
    },
    { title: 'Tên', dataIndex: 'title', key: 'title' },
    {
      title: 'Giảm',
      key: 'discount',
      width: 130,
      render: (_, r) => (
        <span>
          {r.discountType === 'PERCENT' ? `${r.discountValue}%` : formatCurrency(r.discountValue)}
          {r.maxDiscountValue ? ` (tối đa ${formatCurrency(r.maxDiscountValue)})` : ''}
        </span>
      ),
    },
    {
      title: 'Đơn tối thiểu',
      dataIndex: 'minOrderValue',
      key: 'minOrderValue',
      width: 140,
      render: (v: number) => formatCurrency(v),
    },
    {
      title: 'Thời hạn',
      key: 'period',
      width: 200,
      render: (_, r) => `${formatDate(r.startAt)} → ${formatDate(r.endAt)}`,
    },
    {
      title: 'Đã dùng',
      key: 'usage',
      width: 100,
      align: 'center',
      render: (_, r) => (
        <span>
          {r.usedCount}/{r.usageLimit ?? '∞'}
        </span>
      ),
    },
    {
      title: 'Trạng thái',
      key: 'status',
      width: 110,
      render: (_, r) => {
        const start = dayjs(r.startAt);
        const end = dayjs(r.endAt);
        if (r.status === 0) return <Tag>Vô hiệu</Tag>;
        if (now.isBefore(start)) return <Tag color="gold">Sắp diễn ra</Tag>;
        if (now.isAfter(end)) return <Tag color="default">Hết hạn</Tag>;
        return <Tag color="green">Đang hoạt động</Tag>;
      },
    },
    {
      title: 'Hành động',
      key: 'action',
      width: 100,
      fixed: 'right',
      render: (_, record) => (
        <Space size="small">
          <Button type="primary" size="small" icon={<EditOutlined />} onClick={() => handleEdit(record)} />
          <Popconfirm
            title="Xóa khuyến mãi này?"
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

  return (
    <MasterLayout>
      <div className={cx('page')}>
        <div className={cx('pageHeader')}>
          <Title level={3}><GiftOutlined /> Khuyến mãi</Title>
          <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
            Thêm khuyến mãi
          </Button>
        </div>

        <Table<Promotion>
          columns={columns}
          dataSource={items}
          rowKey="id"
          loading={loading}
          scroll={{ x: 1100 }}
          pagination={{
            current: page,
            pageSize,
            total,
            showSizeChanger: true,
            showTotal: (t) => `Tổng ${t} mã`,
            onChange: (p, ps) => setParams({ page: p, pageSize: ps }),
          }}
        />

        <Modal
          title={editing ? 'Sửa khuyến mãi' : 'Thêm khuyến mãi'}
          open={isModalOpen}
          onCancel={() => setIsModalOpen(false)}
          onOk={() => form.submit()}
          okText="Lưu"
          cancelText="Hủy"
          width={660}
          destroyOnClose
        >
          <Form<PromotionUpsertRequest>
            form={form}
            layout="vertical"
            onFinish={handleSubmit}
          >
            <Space style={{ display: 'flex' }} size="large">
              <Form.Item
                name="promoCode"
                label="Mã giảm giá"
                rules={[{ required: true, message: 'Bắt buộc!' }]}
                style={{ flex: 1 }}
              >
                <Input placeholder="FISH10" style={{ textTransform: 'uppercase' }} />
              </Form.Item>
              <Form.Item
                name="title"
                label="Tên chương trình"
                rules={[{ required: true, message: 'Bắt buộc!' }]}
                style={{ flex: 2 }}
              >
                <Input placeholder="Giảm 10% cho đơn hàng..." />
              </Form.Item>
            </Space>

            <Space style={{ display: 'flex' }} size="large">
              <Form.Item name="discountType" label="Loại giảm" style={{ flex: 1 }}>
                <Select
                  options={[
                    { value: 'PERCENT', label: 'Phần trăm (%)' },
                    { value: 'AMOUNT', label: 'Số tiền (VNĐ)' },
                  ]}
                />
              </Form.Item>
              <Form.Item
                name="discountValue"
                label="Giá trị giảm"
                rules={[{ required: true, message: 'Bắt buộc!' }]}
                style={{ flex: 1 }}
              >
                <InputNumber style={{ width: '100%' }} min={0} />
              </Form.Item>
              <Form.Item name="maxDiscountValue" label="Giảm tối đa (VNĐ)" style={{ flex: 1 }}>
                <InputNumber style={{ width: '100%' }} min={0} />
              </Form.Item>
            </Space>

            <Space style={{ display: 'flex' }} size="large">
              <Form.Item
                name="minOrderValue"
                label="Đơn tối thiểu"
                rules={[{ required: true, message: 'Bắt buộc!' }]}
                style={{ flex: 1 }}
              >
                <InputNumber<number>
                  style={{ width: '100%' }}
                  min={0}
                  formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                  parser={(v) => Number((v || '').replace(/,/g, ''))}
                />
              </Form.Item>
              <Form.Item name="usageLimit" label="Giới hạn lượt dùng" style={{ flex: 1 }}>
                <InputNumber style={{ width: '100%' }} min={1} placeholder="Không giới hạn" />
              </Form.Item>
            </Space>

            <Space style={{ display: 'flex' }} size="large">
              <Form.Item
                name="startAt"
                label="Bắt đầu"
                rules={[{ required: true, message: 'Bắt buộc!' }]}
                style={{ flex: 1 }}
              >
                <Input type="datetime-local" />
              </Form.Item>
              <Form.Item
                name="endAt"
                label="Kết thúc"
                rules={[{ required: true, message: 'Bắt buộc!' }]}
                style={{ flex: 1 }}
              >
                <Input type="datetime-local" />
              </Form.Item>
            </Space>

            <Form.Item name="description" label="Mô tả">
              <Input.TextArea rows={2} />
            </Form.Item>

            <Form.Item name="status" label="Trạng thái">
              <Select
                options={[
                  { value: 1, label: 'Kích hoạt' },
                  { value: 0, label: 'Vô hiệu hóa' },
                ]}
              />
            </Form.Item>
          </Form>
        </Modal>
      </div>
    </MasterLayout>
  );
};

export default Promotions;
