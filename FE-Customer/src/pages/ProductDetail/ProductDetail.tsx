import React, { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { formatVND } from '@/utils/format';
import {
  Row,
  Col,
  Breadcrumb,
  InputNumber,
  Button,
  Tabs,
  Tag,
  Spin,
  Empty,
  message,
  Divider,
} from 'antd';
import {
  HomeOutlined,
  ShoppingCartOutlined,
  ThunderboltOutlined,
} from '@ant-design/icons';
import { CustomerLayout } from '@/components/Layout';
import { ProductCard } from '@/components/ProductCard';
import {
  productService,
  cartService,
  authService,
  type Product,
} from '@/api';
import { resolveImageUrl, useImageFallback } from '@/utils/image';
import styles from './ProductDetail.module.scss';

const ProductDetail: React.FC = () => {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const [product, setProduct] = useState<Product | null>(null);
  const [related, setRelated] = useState<Product[]>([]);
  const [loading, setLoading] = useState(false);
  const [qty, setQty] = useState(1);

  useEffect(() => {
    if (!slug) return;
    setLoading(true);
    productService
      .getBySlug(slug)
      .then(async (res) => {
        if (res.success && res.data) {
          setProduct(res.data);
          setQty(1);
          const rel = await productService.getAll({ pageSize: 6 });
          if (rel.success && rel.data) {
            setRelated(
              rel.data.items
                .filter(
                  (p) =>
                    p.id !== res.data!.id &&
                    p.categoryId === res.data!.categoryId
                )
                .slice(0, 6)
            );
          }
        } else {
          setProduct(null);
        }
      })
      .catch(() => setProduct(null))
      .finally(() => setLoading(false));
  }, [slug]);

  const fmt = formatVND;

  const requireLogin = () => {
    if (!authService.isAuthenticated()) {
      navigate(
        `/login?returnUrl=${encodeURIComponent(window.location.pathname + window.location.search)}`
      );
      return false;
    }
    return true;
  };

  const handleAddToCart = async () => {
    if (!product) return;
    if (!requireLogin()) return;
    const res = await cartService.addToCart(product.id, qty, product.name, product.salePrice, product.imageUrl);
    if (res.success) {
      message.success('Đã thêm vào giỏ hàng');
      if (res.data) window.dispatchEvent(new CustomEvent('cart-updated', { detail: res.data.totalItems }));
    } else {
      message.error(res.error || 'Không thể thêm vào giỏ hàng');
    }
  };

  const handleBuyNow = async () => {
    if (!product) return;
    if (!requireLogin()) return;
    const res = await cartService.addToCart(product.id, qty, product.name, product.salePrice, product.imageUrl);
    if (res.success) {
      if (res.data) window.dispatchEvent(new CustomEvent('cart-updated', { detail: res.data.totalItems }));
      navigate('/checkout');
    } else {
      message.error(res.error || 'Không thể mua ngay');
    }
  };

  if (loading) {
    return (
      <CustomerLayout>
        <div className={styles.center}><Spin size="large" /></div>
      </CustomerLayout>
    );
  }

  if (!product) {
    return (
      <CustomerLayout>
        <div className={styles.center}>
          <Empty description="Không tìm thấy sản phẩm" />
        </div>
      </CustomerLayout>
    );
  }

  const displayDiscount = 15;
  const originalPrice = product.salePrice > 0
    ? Math.round(product.salePrice / (1 - displayDiscount / 100))
    : null;
  const showDiscount = Boolean(originalPrice && displayDiscount > 0);

  return (
    <CustomerLayout>
      <div className={styles.container}>
        <Breadcrumb
          className={styles.breadcrumb}
          items={[
            { title: <Link to="/"><HomeOutlined /> Trang chủ</Link> },
            { title: <Link to="/products">Sản phẩm</Link> },
            ...(product.categoryName
              ? [{ title: product.categoryName }]
              : []),
            { title: product.name },
          ]}
        />

        <Row gutter={[32, 32]} className={styles.mainRow}>
          <Col xs={24} md={10}>
            <div className={styles.imageWrap}>
              <img
                src={resolveImageUrl(product.imageUrl)}
                alt={product.name}
                onError={useImageFallback}
                className={styles.mainImage}
              />
            </div>
          </Col>
          <Col xs={24} md={14}>
            <div className={styles.info}>
              <h1 className={styles.name}>{product.name}</h1>
              <div className={styles.meta}>
                <span>Mã SP: <strong>{product.productCode}</strong></span>
                {product.categoryName && (
                  <span>Danh mục: <strong>{product.categoryName}</strong></span>
                )}
              </div>
              <div className={styles.ratingRow}>
                <span className={styles.sold}>Đã bán: {product.soldQuantity}</span>
              </div>
              <div className={styles.priceBox}>
                <div className={styles.priceMainRow}>
                  <span className={styles.price}>{fmt(product.salePrice)}</span>
                  {showDiscount && (
                    <div className={styles.priceMeta}>
                      <span className={styles.originalPrice}>{fmt(originalPrice ?? 0)}</span>
                      <span className={styles.discountBadge}>-{displayDiscount}%</span>
                    </div>
                  )}
                </div>
              </div>

              {product.shortDescription && (
                <p className={styles.shortDesc}>{product.shortDescription}</p>
              )}

              <div className={styles.stockRow}>
                {product.stockQuantity > 0 ? (
                  <Tag color="green">Còn {product.stockQuantity} sản phẩm</Tag>
                ) : (
                  <Tag color="red">Hết hàng</Tag>
                )}
              </div>

              <Divider />

              <div className={styles.qtyRow}>
                <span>Số lượng:</span>
                <InputNumber
                  min={1}
                  max={product.stockQuantity || 1}
                  value={qty}
                  onChange={(v) => setQty(Number(v) || 1)}
                  disabled={product.stockQuantity === 0}
                />
              </div>

              <div className={styles.actions}>
                <Button
                  type="default"
                  size="large"
                  icon={<ShoppingCartOutlined />}
                  onClick={handleAddToCart}
                  disabled={product.stockQuantity === 0}
                >
                  Thêm vào giỏ
                </Button>
                <Button
                  type="primary"
                  size="large"
                  icon={<ThunderboltOutlined />}
                  onClick={handleBuyNow}
                  disabled={product.stockQuantity === 0}
                >
                  Mua ngay
                </Button>
              </div>
            </div>
          </Col>
        </Row>

        <Tabs
          className={styles.tabs}
          defaultActiveKey="desc"
          items={[
            {
              key: 'desc',
              label: 'Mô tả sản phẩm',
              children: product.description ? (
                <div
                  className={styles.desc}
                  dangerouslySetInnerHTML={{ __html: product.description }}
                />
              ) : (
                <p className={styles.descEmpty}>Đang cập nhật mô tả...</p>
              ),
            },
            {
              key: 'spec',
              label: 'Thông tin kỹ thuật',
              children: (
                <ul className={styles.spec}>
                  <li>Mã sản phẩm: {product.productCode}</li>
                  <li>Danh mục: {product.categoryName || '—'}</li>
                  <li>Tồn kho: {product.stockQuantity}</li>
                  <li>Đã bán: {product.soldQuantity}</li>
                </ul>
              ),
            },
          ]}
        />

        {related.length > 0 && (
          <div className={styles.relatedSection}>
            <h2 className={styles.relatedTitle}>Sản phẩm liên quan</h2>
            <Row gutter={[16, 24]}>
              {related.map((p) => (
                <Col xs={12} sm={8} md={6} lg={4} key={p.id}>
                  <ProductCard product={p} />
                </Col>
              ))}
            </Row>
          </div>
        )}
      </div>
    </CustomerLayout>
  );
};

export default ProductDetail;
