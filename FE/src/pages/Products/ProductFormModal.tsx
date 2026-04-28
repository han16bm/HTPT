import React, { useEffect, useRef, useState } from 'react';
import {
  Modal, Form, Input, InputNumber, Select, Switch, Space, Image, Button,
} from 'antd';
import {
  PlusOutlined, DeleteOutlined, EyeOutlined,
} from '@ant-design/icons';
import type {
  Category, Product, ProductUpsertRequest, ProductImageUpsertItem,
} from '@/interfaces';
import { IMAGE_PLACEHOLDER, resolveProductImageUrl } from '@/shared/utils/image';

/* ---------- Types cho local preview ---------- */
interface LocalImage {
  uid: string;
  file?: File;           // file mới chọn từ máy
  url: string;           // objectURL (local) hoặc remote URL
  existingId?: number;   // id nếu đã có trên server
  altText?: string;
  markedRemove?: boolean; // đánh dấu xoá khi lưu
}

let uidCounter = 0;
const nextUid = () => `local-${++uidCounter}`;
const parseNumberInput = (value?: string | null) => {
  const normalized = (value ?? '').replace(/,/g, '').trim();
  return normalized === '' ? 0 : Number(normalized);
};

/* ---------- Props ---------- */
interface ProductFormModalProps {
  open: boolean;
  editing: Product | null;
  categories: Category[];
  onCancel: () => void;
  onSubmit: (values: ProductUpsertRequest) => Promise<void>;
}

const ProductFormModal: React.FC<ProductFormModalProps> = ({
  open, editing, categories, onCancel, onSubmit,
}) => {
  const [form] = Form.useForm();

  // --- Ảnh đại diện (primary) ---
  const [primaryImage, setPrimaryImage] = useState<LocalImage | null>(null);
  const [removePrimary, setRemovePrimary] = useState(false);
  const primaryInputRef = useRef<HTMLInputElement>(null);

  // --- Gallery ---
  const [gallery, setGallery] = useState<LocalImage[]>([]);
  const galleryInputRef = useRef<HTMLInputElement>(null);

  // --- Preview lightbox ---
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  // --- Submitting ---
  const [submitting, setSubmitting] = useState(false);

  /* ====== Populate form khi mở ====== */
  useEffect(() => {
    if (!open) return;

    // Reset
    setRemovePrimary(false);
    setPreviewUrl(null);

    if (editing) {
      form.setFieldsValue({
        categoryId: editing.categoryId,
        name: editing.name,
        shortDescription: editing.shortDescription,
        description: editing.description,
        costPrice: editing.costPrice,
        salePrice: editing.salePrice,
        stockQuantity: editing.stockQuantity,
        weightGrams: editing.weightGrams,
        isFeatured: editing.isFeatured,
        status: editing.status ? 1 : 0,
      });

      // Primary image
      if (editing.imageUrl) {
        setPrimaryImage({ uid: nextUid(), url: editing.imageUrl });
      } else {
        setPrimaryImage(null);
      }

      // Gallery images
      setGallery(
        (editing.images ?? [])
          .filter((img) => !img.isPrimary)
          .map((img) => ({
            uid: nextUid(),
            url: img.imageUrl,
            existingId: img.id,
            altText: img.altText,
          })),
      );
    } else {
      form.resetFields();
      form.setFieldsValue({ status: 1, isFeatured: false });
      setPrimaryImage(null);
      setGallery([]);
    }
  }, [editing, open, form]);

  /* ====== Cleanup objectURLs khi unmount / close ====== */
  useEffect(() => {
    return () => {
      if (primaryImage?.file) URL.revokeObjectURL(primaryImage.url);
      gallery.forEach((g) => { if (g.file) URL.revokeObjectURL(g.url); });
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  /* ====== Handlers — Primary ====== */
  const handlePrimarySelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    // Revoke URL cũ nếu là local file
    if (primaryImage?.file) URL.revokeObjectURL(primaryImage.url);
    setPrimaryImage({ uid: nextUid(), file, url: URL.createObjectURL(file) });
    setRemovePrimary(false);
    // Reset input value để có thể chọn lại cùng file
    e.target.value = '';
  };

  const handlePrimaryRemove = () => {
    if (primaryImage?.file) URL.revokeObjectURL(primaryImage.url);
    setPrimaryImage(null);
    setRemovePrimary(true);
  };

  /* ====== Handlers — Gallery ====== */
  const handleGallerySelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files ?? []);
    if (!files.length) return;
    const newItems: LocalImage[] = files.map((f) => ({
      uid: nextUid(),
      file: f,
      url: URL.createObjectURL(f),
    }));
    setGallery((prev) => [...prev, ...newItems]);
    e.target.value = '';
  };

  const handleGalleryRemove = (uid: string) => {
    setGallery((prev) => prev.map((g) => {
      if (g.uid !== uid) return g;
      if (g.file) URL.revokeObjectURL(g.url);
      // Nếu là ảnh đã tồn tại trên server → đánh dấu remove
      if (g.existingId) return { ...g, markedRemove: true };
      // Nếu là ảnh local mới → xoá luôn
      return null as never;
    }).filter(Boolean));
  };

  /* ====== Submit → build payload ====== */
  const handleFinish = async (values: Record<string, unknown>) => {
    setSubmitting(true);
    try {
      const payload: ProductUpsertRequest = {
        id: editing?.id,
        categoryId: values.categoryId as number,
        name: values.name as string,
        shortDescription: values.shortDescription as string | undefined,
        description: values.description as string | undefined,
        costPrice: values.costPrice as number,
        salePrice: values.salePrice as number,
        stockQuantity: values.stockQuantity as number,
        weightGrams: values.weightGrams as number | undefined,
        isFeatured: values.isFeatured as boolean | undefined,
        status: values.status as number | undefined,
        removeImage: removePrimary,
      };

      // Primary image
      if (primaryImage?.file) {
        payload.imageFile = primaryImage.file;
      } else if (primaryImage && !primaryImage.file) {
        // Giữ nguyên URL cũ
        payload.imageUrl = primaryImage.url;
      }

      // Gallery images
      const galleryPayload: ProductImageUpsertItem[] = [];
      let order = 0;
      gallery.forEach((g) => {
        galleryPayload.push({
          id: g.existingId,
          imageFile: g.file,
          imageUrl: g.file ? undefined : g.url,
          altText: g.altText,
          displayOrder: order++,
          remove: g.markedRemove,
        });
      });
      // Ảnh trên server đã bị xoá khỏi gallery (không còn trong state)
      // → cần gửi remove flag cho các ảnh existing bị user xoá
      // (đã handle qua markedRemove ở trên)

      if (galleryPayload.length > 0) {
        payload.images = galleryPayload;
      }

      await onSubmit(payload);
    } finally {
      setSubmitting(false);
    }
  };

  /* ====== Render helpers ====== */
  const renderImageCard = (
    img: { url: string; uid: string },
    onRemove: () => void,
    size = 104,
  ) => {
    const resolvedUrl = resolveProductImageUrl(img.url) ?? IMAGE_PLACEHOLDER;

    return (
    <div
      key={img.uid}
      style={{
        position: 'relative',
        width: size,
        height: size,
        borderRadius: 8,
        overflow: 'hidden',
        border: '1px solid #d9d9d9',
        flexShrink: 0,
      }}
    >
      <img
        src={resolvedUrl}
        alt=""
        onError={(e) => { e.currentTarget.src = IMAGE_PLACEHOLDER; }}
        style={{ width: '100%', height: '100%', objectFit: 'cover' }}
      />
      <div
        style={{
          position: 'absolute',
          inset: 0,
          background: 'rgba(0,0,0,0.45)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          gap: 8,
          opacity: 0,
          transition: 'opacity 0.2s',
        }}
        onMouseEnter={(e) => { (e.currentTarget as HTMLDivElement).style.opacity = '1'; }}
        onMouseLeave={(e) => { (e.currentTarget as HTMLDivElement).style.opacity = '0'; }}
      >
        <EyeOutlined
          style={{ color: '#fff', fontSize: 18, cursor: 'pointer' }}
          onClick={() => setPreviewUrl(resolvedUrl)}
        />
        <DeleteOutlined
          style={{ color: '#ff4d4f', fontSize: 18, cursor: 'pointer' }}
          onClick={onRemove}
        />
      </div>
    </div>
    );
  };

  const renderUploadButton = (onClick: () => void, size = 104) => (
    <div
      onClick={onClick}
      style={{
        width: size,
        height: size,
        borderRadius: 8,
        border: '1px dashed #d9d9d9',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        cursor: 'pointer',
        color: '#999',
        transition: 'border-color 0.2s',
        flexShrink: 0,
      }}
      onMouseEnter={(e) => { (e.currentTarget as HTMLDivElement).style.borderColor = '#1677ff'; }}
      onMouseLeave={(e) => { (e.currentTarget as HTMLDivElement).style.borderColor = '#d9d9d9'; }}
    >
      <PlusOutlined style={{ fontSize: 20 }} />
      <span style={{ fontSize: 12, marginTop: 4 }}>Tải ảnh</span>
    </div>
  );

  return (
    <>
      <Modal
        title={editing ? 'Sửa sản phẩm' : 'Thêm sản phẩm'}
        open={open}
        onCancel={onCancel}
        onOk={() => form.submit()}
        okText="Lưu"
        cancelText="Hủy"
        width={720}
        destroyOnClose
        confirmLoading={submitting}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleFinish}
        >
          <Form.Item
            name="name"
            label="Tên sản phẩm"
            rules={[{ required: true, message: 'Vui lòng nhập tên!' }]}
          >
            <Input placeholder="Nhập tên sản phẩm" />
          </Form.Item>

          <Form.Item
            name="categoryId"
            label="Danh mục"
            rules={[{ required: true, message: 'Vui lòng chọn danh mục!' }]}
          >
            <Select
              placeholder="Chọn danh mục"
              options={categories.map((c) => ({
                value: c.id,
                label: c.name,
              }))}
            />
          </Form.Item>

          <Form.Item name="shortDescription" label="Mô tả ngắn">
            <Input.TextArea rows={2} />
          </Form.Item>

          <Form.Item name="description" label="Mô tả chi tiết">
            <Input.TextArea rows={4} />
          </Form.Item>

          {/* ===== Ảnh đại diện ===== */}
          <Form.Item label="Ảnh đại diện">
            <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
              {primaryImage && !primaryImage.markedRemove &&
                renderImageCard(primaryImage, handlePrimaryRemove)}
              {!primaryImage &&
                renderUploadButton(() => primaryInputRef.current?.click())}
            </div>
            <input
              ref={primaryInputRef}
              type="file"
              accept="image/jpeg,image/png,image/gif,image/webp"
              style={{ display: 'none' }}
              onChange={handlePrimarySelect}
            />
            {primaryImage && (
              <Button
                size="small"
                style={{ marginTop: 8 }}
                onClick={() => primaryInputRef.current?.click()}
              >
                Thay ảnh khác
              </Button>
            )}
          </Form.Item>

          {/* ===== Gallery ===== */}
          <Form.Item label="Ảnh bộ sưu tập">
            <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
              {gallery
                .filter((g) => !g.markedRemove)
                .map((g) => renderImageCard(g, () => handleGalleryRemove(g.uid)))}
              {renderUploadButton(() => galleryInputRef.current?.click())}
            </div>
            <input
              ref={galleryInputRef}
              type="file"
              accept="image/jpeg,image/png,image/gif,image/webp"
              multiple
              style={{ display: 'none' }}
              onChange={handleGallerySelect}
            />
          </Form.Item>

          <Space size="large" style={{ display: 'flex' }}>
            <Form.Item
              name="costPrice"
              label="Giá nhập"
              rules={[{ required: true, message: 'Bắt buộc' }]}
              style={{ flex: 1 }}
            >
              <InputNumber<number>
                style={{ width: '100%' }}
                min={0}
                formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                parser={parseNumberInput}
              />
            </Form.Item>
            <Form.Item
              name="salePrice"
              label="Giá bán"
              rules={[{ required: true, message: 'Bắt buộc' }]}
              style={{ flex: 1 }}
            >
              <InputNumber<number>
                style={{ width: '100%' }}
                min={0}
                formatter={(v) => `${v}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
                parser={parseNumberInput}
              />
            </Form.Item>
            <Form.Item
              name="stockQuantity"
              label="Tồn kho"
              rules={[{ required: true, message: 'Bắt buộc' }]}
              style={{ flex: 1 }}
            >
              <InputNumber<number> style={{ width: '100%' }} min={0} />
            </Form.Item>
          </Space>

          <Space size="large" style={{ display: 'flex' }}>
            <Form.Item name="weightGrams" label="Cân nặng (gram)" style={{ flex: 1 }}>
              <InputNumber<number> style={{ width: '100%' }} min={0} />
            </Form.Item>
            <Form.Item
              name="isFeatured"
              label="Sản phẩm nổi bật"
              valuePropName="checked"
              style={{ flex: 1 }}
            >
              <Switch />
            </Form.Item>
            <Form.Item
              name="status"
              label="Trạng thái"
              style={{ flex: 1 }}
            >
              <Select
                options={[
                  { value: 1, label: 'Đang bán' },
                  { value: 0, label: 'Ngừng bán' },
                ]}
              />
            </Form.Item>
          </Space>
        </Form>
      </Modal>

      {/* Lightbox preview */}
      <Image
        style={{ display: 'none' }}
        preview={{
          visible: !!previewUrl,
          src: previewUrl ?? '',
          onVisibleChange: (v) => { if (!v) setPreviewUrl(null); },
        }}
      />
    </>
  );
};

export default ProductFormModal;
