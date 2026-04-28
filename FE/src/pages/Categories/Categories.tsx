import React, { useEffect, useMemo, useState } from 'react';
import {
  Button, Form, Input, InputNumber, Modal,
  Popconfirm, Select, Space, Table, Tag, Typography, message,
} from 'antd';
import { EditOutlined, DeleteOutlined, PlusOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import classNames from 'classnames/bind';
import styles from './Categories.module.scss';
import { MasterLayout } from '@/components/Layout';
import { categoryService } from '@/api';
import { useAsync } from '@/hooks';
import type { Category, CategoryUpsertRequest } from '@/interfaces';

const cx = classNames.bind(styles);
const { Title, Text } = Typography;

type CategoryWithLevel = Category & { level: number };

const toSlug = (str: string) =>
  str
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd')
    .replace(/[^a-z0-9\s-]/g, '')
    .trim()
    .replace(/\s+/g, '-');

const flattenTree = (nodes: Category[], level = 0): CategoryWithLevel[] =>
  nodes.flatMap((node) => [
    { ...node, level },
    ...flattenTree(node.children ?? [], level + 1),
  ]);

const collectDescendantIds = (node: Category): number[] =>
  (node.children ?? []).flatMap((child) => [child.id, ...collectDescendantIds(child)]);

const countTreeNodes = (nodes: Category[]): number =>
  nodes.reduce((total, node) => total + 1 + countTreeNodes(node.children ?? []), 0);

const Categories: React.FC = () => {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editing, setEditing] = useState<Category | null>(null);
  const [expandedRowKeys, setExpandedRowKeys] = useState<React.Key[]>([]);
  const [form] = Form.useForm<CategoryUpsertRequest>();

  const { data: categories, loading, refetch } = useAsync<Category[]>({
    fetcher: () => categoryService.getTree(),
    errorMessage: 'Lỗi khi tải cây danh mục',
  });

  const flatCategories = useMemo(
    () => flattenTree(categories ?? []),
    [categories]
  );

  const totalCategories = useMemo(
    () => countTreeNodes(categories ?? []),
    [categories]
  );

  const childCategories = Math.max(totalCategories - (categories?.length ?? 0), 0);

  useEffect(() => {
    if (editing) {
      form.setFieldsValue({
        id: editing.id,
        categoryCode: editing.categoryCode,
        name: editing.name,
        slug: editing.slug,
        description: editing.description,
        parentId: editing.parentId ?? undefined,
        displayOrder: editing.displayOrder,
        status: editing.status,
      });
      return;
    }

    form.resetFields();
    form.setFieldsValue({ status: true, displayOrder: 1 });
  }, [editing, form]);

  const handleAdd = () => {
    setEditing(null);
    setIsModalOpen(true);
  };

  const handleEdit = (record: Category) => {
    setEditing(record);
    setIsModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    try {
      await categoryService.delete(id);
      message.success('Xóa danh mục thành công');
      refetch();
    } catch (err) {
      message.error((err as { message?: string }).message || 'Lỗi khi xóa');
    }
  };

  const handleSubmit = async (values: CategoryUpsertRequest) => {
    try {
      await categoryService.upsert({ ...values, id: editing?.id });
      message.success(editing ? 'Cập nhật thành công' : 'Thêm danh mục thành công');
      setIsModalOpen(false);
      refetch();
    } catch (err) {
      message.error((err as { message?: string }).message || 'Lỗi khi lưu');
    }
  };

  const columns: ColumnsType<Category> = [
    {
      title: 'Mã',
      dataIndex: 'categoryCode',
      key: 'categoryCode',
      width: 140,
      render: (value: string | undefined, record: Category) => (
        <div className={cx('codeCell')}>
          <Tag className={cx('codeTag', { parentTag: !record.parentId })}>
            {value || '—'}
          </Tag>
        </div>
      ),
    },
    {
      title: 'Danh mục',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record) => {
        const level = (record as CategoryWithLevel).level ?? 0;
        const hasChildren = (record.children?.length ?? 0) > 0;

        return (
          <div className={cx('nameCell', { childRow: level > 0 })}>
            <div className={cx('nameMain')}>
              {level > 0 && (
                <span
                  className={cx('treeBranch')}
                  style={{ width: 18 + Math.max(level - 1, 0) * 22 }}
                  aria-hidden="true"
                />
              )}
              <div className={cx('nameContent')}>
                <div className={cx('nameLine')}>
                  <span className={cx('nameText', { parentName: level === 0 })}>{name}</span>
                  <Tag className={cx('levelTag', { rootLevel: level === 0, childLevel: level > 0 })}>
                    {level === 0 ? 'Gốc' : `Con cấp ${level}`}
                  </Tag>
                  {hasChildren && (
                    <span className={cx('childCount')}>{record.children?.length} mục con</span>
                  )}
                </div>
                <Text className={cx('slugText')}>/{record.slug}</Text>
              </div>
            </div>
          </div>
        );
      },
    },
    {
      title: 'Thứ tự',
      dataIndex: 'displayOrder',
      key: 'displayOrder',
      width: 90,
      align: 'center',
      render: (value: number) => <span className={cx('orderBadge')}>#{value}</span>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      render: (status: boolean) => (
        <Tag className={cx('statusTag', { activeStatus: status, inactiveStatus: !status })}>
          {status ? 'Hiển thị' : 'Ẩn'}
        </Tag>
      ),
    },
    {
      title: 'Hành động',
      key: 'action',
      width: 116,
      fixed: 'right',
      render: (_, record) => (
        <Space size="small">
          <Button type="primary" size="small" icon={<EditOutlined />} onClick={() => handleEdit(record)} />
          <Popconfirm
            title="Xóa danh mục này?"
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

  const parentOptions = useMemo(() => {
    const blockedIds = editing ? new Set([editing.id, ...collectDescendantIds(editing)]) : new Set<number>();

    return flatCategories
      .filter((category) => !blockedIds.has(category.id))
      .map((category) => ({
        value: category.id,
        label: `${'— '.repeat(category.level)}${category.name}`,
      }));
  }, [editing, flatCategories]);

  return (
    <MasterLayout>
      <div className={cx('page')}>
        <div className={cx('pageHeader')}>
          <div>
            <Title level={3}>Danh mục sản phẩm</Title>
            {/* <Text className={cx('headerHint')}>
              Cấu trúc cây phân cấp với danh mục gốc và các nhánh con.
            </Text> */}
          </div>
          <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
            Thêm danh mục
          </Button>
        </div>

        <div className={cx('summaryStrip')}>
          <div className={cx('summaryCard')}>
            <span className={cx('summaryLabel')}>Danh mục gốc</span>
            <strong>{categories?.length ?? 0}</strong>
          </div>
          <div className={cx('summaryCard')}>
            <span className={cx('summaryLabel')}>Danh mục con</span>
            <strong>{childCategories}</strong>
          </div>
          <div className={cx('summaryCard')}>
            <span className={cx('summaryLabel')}>Tổng số mục</span>
            <strong>{totalCategories}</strong>
          </div>
        </div>

        <div className={cx('tableShell')}>
          <Table<Category>
            className={cx('categoryTable')}
            columns={columns}
            dataSource={categories ?? []}
            rowKey="id"
            loading={loading}
            scroll={{ x: 920 }}
            pagination={{ pageSize: 20, showTotal: () => `Tổng ${totalCategories} danh mục` }}
            rowClassName={(record) => cx({ parentTableRow: !record.parentId, childTableRow: !!record.parentId })}
            expandable={{
              expandedRowKeys,
              onExpandedRowsChange: (keys) => setExpandedRowKeys([...keys]),
              indentSize: 26,
            }}
          />
        </div>

        <Modal
          title={editing ? 'Sửa danh mục' : 'Thêm danh mục'}
          open={isModalOpen}
          onCancel={() => setIsModalOpen(false)}
          onOk={() => form.submit()}
          okText="Lưu"
          cancelText="Hủy"
          width={600}
          destroyOnClose
        >
          <Form<CategoryUpsertRequest>
            form={form}
            layout="vertical"
            onFinish={handleSubmit}
          >
            <Form.Item
              name="name"
              label="Tên danh mục"
              rules={[{ required: true, message: 'Vui lòng nhập tên!' }]}
            >
              <Input
                placeholder="Cá cảnh nhiệt đới"
                onChange={(e) => {
                  if (!editing) {
                    form.setFieldValue('slug', toSlug(e.target.value));
                  }
                }}
              />
            </Form.Item>

            <Space style={{ display: 'flex' }} size="large">
              <Form.Item name="categoryCode" label="Mã danh mục" style={{ flex: 1 }}>
                <Input placeholder="FISH" />
              </Form.Item>
              <Form.Item name="slug" label="Slug" style={{ flex: 1 }}>
                <Input placeholder="ca-canh-nhiet-doi" />
              </Form.Item>
            </Space>

            <Form.Item name="parentId" label="Danh mục cha">
              <Select placeholder="— Không có (danh mục gốc) —" allowClear options={parentOptions} />
            </Form.Item>

            <Form.Item name="description" label="Mô tả">
              <Input.TextArea rows={2} />
            </Form.Item>

            <Space style={{ display: 'flex' }} size="large">
              <Form.Item name="displayOrder" label="Thứ tự hiển thị" style={{ flex: 1 }}>
                <InputNumber style={{ width: '100%' }} min={1} />
              </Form.Item>
              <Form.Item name="status" label="Trạng thái" style={{ flex: 1 }}>
                <Select
                  options={[
                    { value: true, label: 'Hiển thị' },
                    { value: false, label: 'Ẩn' },
                  ]}
                />
              </Form.Item>
            </Space>
          </Form>
        </Modal>
      </div>
    </MasterLayout>
  );
};

export default Categories;
