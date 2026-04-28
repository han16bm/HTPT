import React, { useEffect, useState } from 'react';
import {
  Button, Drawer, Form, Image, Input, Modal, Popconfirm, Select,
  Space, Table, Tag, Typography, message,
} from 'antd';
import { EditOutlined, DeleteOutlined, PlusOutlined, ReadOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import classNames from 'classnames/bind';
import styles from './Blog.module.scss';
import { MasterLayout } from '@/components/Layout';
import { blogService } from '@/api';
import { usePagedQuery, useAsync } from '@/hooks';
import { formatDate } from '@/shared/utils/format';
import { BLOG_STATUS_LABEL, BLOG_STATUS_COLOR } from '@/shared/utils/constants';
import type { BlogPost, BlogPostUpsertRequest, BlogCategory, BlogStatus } from '@/interfaces';
import type { BlogSearchParams } from '@/api';

const cx = classNames.bind(styles);
const { Title } = Typography;

const Blog: React.FC = () => {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editing, setEditing] = useState<BlogPost | null>(null);
  const [previewPost, setPreviewPost] = useState<BlogPost | null>(null);
  const [form] = Form.useForm<BlogPostUpsertRequest>();

  const {
    items, total, page, pageSize, loading, setParams, refetch,
  } = usePagedQuery<BlogPost, BlogSearchParams>({
    fetcher: (p) => blogService.search(p),
    initialParams: { page: 1, pageSize: 10 },
    errorMessage: 'Lỗi khi tải bài viết',
  });

  const { data: blogCategories } = useAsync<BlogCategory[]>({
    fetcher: () => blogService.getCategories(),
    errorMessage: '',
  });

  useEffect(() => {
    if (editing) {
      form.setFieldsValue({
        id: editing.id,
        title: editing.title,
        summary: editing.summary,
        content: editing.content,
        thumbnailUrl: editing.thumbnailUrl,
        categoryId: editing.categoryId,
        status: editing.status,
      });
    } else {
      form.resetFields();
      form.setFieldsValue({ status: 'DRAFT' });
    }
  }, [editing, form]);

  const handleAdd = () => { setEditing(null); setIsModalOpen(true); };
  const handleEdit = (r: BlogPost) => { setEditing(r); setIsModalOpen(true); };

  const handleDelete = async (id: number) => {
    try {
      await blogService.delete(id);
      message.success('Xóa bài viết thành công');
      refetch();
    } catch (err) {
      message.error((err as { message?: string }).message || 'Lỗi khi xóa');
    }
  };

  const handleSubmit = async (values: BlogPostUpsertRequest) => {
    try {
      await blogService.upsert({ ...values, id: editing?.id });
      message.success(editing ? 'Cập nhật thành công' : 'Đăng bài viết thành công');
      setIsModalOpen(false);
      refetch();
    } catch (err) {
      message.error((err as { message?: string }).message || 'Lỗi khi lưu');
    }
  };

  const columns: ColumnsType<BlogPost> = [
    {
      title: 'Thumbnail',
      dataIndex: 'thumbnailUrl',
      key: 'thumbnail',
      width: 80,
      render: (url?: string) =>
        url ? (
          <Image src={url} width={60} height={40} style={{ objectFit: 'cover', borderRadius: 4 }} />
        ) : (
          <div style={{ width: 60, height: 40, background: '#f5f5f5', borderRadius: 4, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <ReadOutlined style={{ color: '#bbb' }} />
          </div>
        ),
    },
    {
      title: 'Tiêu đề',
      dataIndex: 'title',
      key: 'title',
      render: (v: string) => <span style={{ fontWeight: 500 }}>{v}</span>,
    },
    {
      title: 'Danh mục',
      dataIndex: 'categoryName',
      key: 'categoryName',
      width: 150,
      render: (v?: string) => v ? <Tag>{v}</Tag> : '—',
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: 110,
      render: (s: BlogStatus) => <Tag color={BLOG_STATUS_COLOR[s]}>{BLOG_STATUS_LABEL[s]}</Tag>,
    },
    {
      title: 'Tác giả',
      dataIndex: 'authorName',
      key: 'authorName',
      width: 130,
      render: (v?: string) => v || '—',
    },
    {
      title: 'Ngày đăng',
      dataIndex: 'publishedAt',
      key: 'publishedAt',
      width: 130,
      render: (v?: string) => v ? formatDate(v) : '—',
    },
    {
      title: 'Hành động',
      key: 'action',
      width: 130,
      fixed: 'right',
      render: (_, record) => (
        <Space size="small">
          <Button size="small" onClick={() => setPreviewPost(record)}>Xem</Button>
          <Button type="primary" size="small" icon={<EditOutlined />} onClick={() => handleEdit(record)} />
          <Popconfirm
            title="Xóa bài viết này?"
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
          <Title level={3}><ReadOutlined /> Bài viết Blog</Title>
          <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
            Viết bài mới
          </Button>
        </div>

        <Table<BlogPost>
          columns={columns}
          dataSource={items}
          rowKey="id"
          loading={loading}
          scroll={{ x: 1000 }}
          pagination={{
            current: page,
            pageSize,
            total,
            showSizeChanger: true,
            showTotal: (t) => `Tổng ${t} bài`,
            onChange: (p, ps) => setParams({ page: p, pageSize: ps }),
          }}
        />

        {/* Add / Edit Modal */}
        <Modal
          title={editing ? 'Sửa bài viết' : 'Viết bài mới'}
          open={isModalOpen}
          onCancel={() => setIsModalOpen(false)}
          onOk={() => form.submit()}
          okText="Lưu"
          cancelText="Hủy"
          width={800}
          destroyOnClose
        >
          <Form<BlogPostUpsertRequest>
            form={form}
            layout="vertical"
            onFinish={handleSubmit}
          >
            <Form.Item
              name="title"
              label="Tiêu đề"
              rules={[{ required: true, message: 'Vui lòng nhập tiêu đề!' }]}
            >
              <Input placeholder="Cách chăm sóc cá Betta..." />
            </Form.Item>

            <Space style={{ display: 'flex' }} size="large">
              <Form.Item name="categoryId" label="Danh mục blog" style={{ flex: 1 }}>
                <Select
                  placeholder="Chọn danh mục"
                  allowClear
                  options={(blogCategories ?? []).map((c) => ({ value: c.id, label: c.name }))}
                />
              </Form.Item>
              <Form.Item name="status" label="Trạng thái" style={{ flex: 1 }}>
                <Select
                  options={[
                    { value: 'DRAFT', label: 'Nháp' },
                    { value: 'PUBLISHED', label: 'Đăng ngay' },
                  ]}
                />
              </Form.Item>
            </Space>

            <Form.Item name="thumbnailUrl" label="URL ảnh thumbnail">
              <Input placeholder="https://..." />
            </Form.Item>

            <Form.Item name="summary" label="Tóm tắt">
              <Input.TextArea rows={2} placeholder="Tóm tắt ngắn về bài viết..." />
            </Form.Item>

            <Form.Item
              name="content"
              label="Nội dung"
              rules={[{ required: true, message: 'Vui lòng nhập nội dung!' }]}
            >
              <Input.TextArea rows={10} placeholder="Nội dung bài viết (hỗ trợ HTML)..." />
            </Form.Item>
          </Form>
        </Modal>

        {/* Preview Drawer */}
        <Drawer
          title={previewPost?.title}
          open={!!previewPost}
          onClose={() => setPreviewPost(null)}
          width={640}
        >
          {previewPost && (
            <div className={cx('preview')}>
              {previewPost.thumbnailUrl && (
                <img src={previewPost.thumbnailUrl} alt="" className={cx('previewImg')} />
              )}
              <div className={cx('previewMeta')}>
                <Tag color={BLOG_STATUS_COLOR[previewPost.status]}>{BLOG_STATUS_LABEL[previewPost.status]}</Tag>
                <span style={{ color: '#888', fontSize: 13 }}>
                  {previewPost.authorName} · {formatDate(previewPost.publishedAt || previewPost.createdAt)}
                </span>
              </div>
              {previewPost.summary && <p className={cx('previewSummary')}>{previewPost.summary}</p>}
              <div
                className={cx('previewContent')}
                dangerouslySetInnerHTML={{ __html: previewPost.content }}
              />
            </div>
          )}
        </Drawer>
      </div>
    </MasterLayout>
  );
};

export default Blog;
