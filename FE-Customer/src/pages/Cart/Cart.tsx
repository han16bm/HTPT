import React, { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
  Row, Col, Button, InputNumber, message, Empty,
  Checkbox, Input, Tag, Divider, Popconfirm, Breadcrumb, Spin, Space,
} from 'antd';
import {
  DeleteOutlined, ShoppingOutlined, TagOutlined,
  ArrowLeftOutlined, ShopOutlined, GiftOutlined,
} from '@ant-design/icons';
import { CustomerLayout } from '@/components/Layout';
import { cartService, promotionService, type CartItem, type PromotionDiscountType } from '@/api';
import { formatVND } from '@/utils/format';
import { FREE_SHIP_THRESHOLD, calcShipping } from '@/constants/shipping';
import styles from './Cart.module.scss';

const fmt = formatVND;

const Cart: React.FC = () => {
  const navigate = useNavigate();
  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [updatingId, setUpdatingId] = useState<number | null>(null);
  const [selectedKeys, setSelectedKeys] = useState<number[]>([]);
  const [promoCode, setPromoCode] = useState('');
  const [appliedPromo, setAppliedPromo] = useState<{
    code: string;
    discountType: PromotionDiscountType;
    discountValue: number;
  } | null>(null);

  useEffect(() => { fetchCart(); }, []);

  useEffect(() => {
    if (cartItems.length) setSelectedKeys(cartItems.map((i) => i.id));
  }, [cartItems]);

  const fetchCart = async () => {
    setLoading(true);
    try {
      const resp = await cartService.getCart();
      if (resp.success && resp.data) {
        setCartItems(resp.data.items);
        window.dispatchEvent(new CustomEvent('cart-updated', { detail: resp.data.totalItems }));
      }
    } catch {
      message.error('Không thể tải giỏ hàng');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateQty = async (id: number, qty: number) => {
    setUpdatingId(id);
    try {
      const resp = await cartService.updateQuantity(id, qty);
      if (resp.success && resp.data) setCartItems(resp.data.items);
    } catch {
      message.error('Không thể cập nhật');
    } finally {
      setUpdatingId(null);
    }
  };

  const handleRemove = async (id: number) => {
    try {
      const resp = await cartService.removeItem(id);
      if (resp.success && resp.data) {
        setCartItems(resp.data.items);
        setSelectedKeys((k) => k.filter((x) => x !== id));
        message.success('Đã xóa sản phẩm');
      }
    } catch {
      message.error('Không thể xóa sản phẩm');
    }
  };

  const handleRemoveSelected = async () => {
    for (const id of selectedKeys) await handleRemove(id);
  };

  const handleApplyPromo = async () => {
    const code = promoCode.trim().toUpperCase();
    if (!code) return;
    try {
      const resp = await promotionService.verify(code, subtotal);
      if (resp.success && resp.data?.isValid) {
        const { discountType, discountValue } = resp.data;
        setAppliedPromo({ code, discountType: discountType!, discountValue });
        const label = discountType === 'PERCENT'
          ? `giảm ${discountValue}%`
          : `giảm ${formatVND(discountValue)}`;
        message.success(`Áp dụng mã "${code}" – ${label}`);
      } else {
        message.error(resp.data?.message || resp.error || 'Mã giảm giá không hợp lệ');
      }
    } catch {
      message.error('Mã giảm giá không hợp lệ');
    }
  };

  const subtotal = cartItems
    .filter((i) => selectedKeys.includes(i.id))
    .reduce((s, i) => s + i.subTotal, 0);
  const shipping = calcShipping(subtotal);
  const discountAmt = appliedPromo
    ? appliedPromo.discountType === 'PERCENT'
      ? Math.round(subtotal * appliedPromo.discountValue / 100)
      : Math.min(appliedPromo.discountValue, subtotal)
    : 0;
  const grandTotal = subtotal + shipping - discountAmt;
  const allSelected = cartItems.length > 0 && selectedKeys.length === cartItems.length;

  return (
    <CustomerLayout>
      <div className={styles.cart}>
        <Breadcrumb
          className={styles.breadcrumb}
          items={[
            { title: <Link to="/">Trang chủ</Link> },
            { title: <Link to="/danhmuc">Sản phẩm</Link> },
            { title: 'Giỏ hàng' },
          ]}
        />

        <h1 className={styles.title}>
          <ShoppingOutlined /> Giỏ hàng
          {cartItems.length > 0 && <span className={styles.badge}>{cartItems.length}</span>}
        </h1>

        {loading ? (
          <div className={styles.loadingWrap}><Spin size="large" /></div>
        ) : cartItems.length > 0 ? (
          <Row gutter={[24, 24]} align="top">
            {/* ── Items list ── */}
            <Col xs={24} lg={16}>
              <div className={styles.itemsCard}>
                <div className={styles.tableHeader}>
                  <Checkbox
                    checked={allSelected}
                    indeterminate={selectedKeys.length > 0 && !allSelected}
                    onChange={(e) =>
                      setSelectedKeys(e.target.checked ? cartItems.map((i) => i.id) : [])
                    }
                  >
                    Chọn tất cả ({cartItems.length} sản phẩm)
                  </Checkbox>
                  <Popconfirm
                    title="Xóa các sản phẩm đã chọn?"
                    onConfirm={handleRemoveSelected}
                    disabled={selectedKeys.length === 0}
                    okText="Xóa"
                    cancelText="Hủy"
                  >
                    <Button
                      size="small"
                      danger
                      type="text"
                      icon={<DeleteOutlined />}
                      disabled={selectedKeys.length === 0}
                    >
                      Xóa đã chọn
                    </Button>
                  </Popconfirm>
                </div>

                {cartItems.map((item) => (
                  <div
                    key={item.id}
                    className={`${styles.itemRow} ${!selectedKeys.includes(item.id) ? styles.dimmed : ''}`}
                  >
                    <Checkbox
                      checked={selectedKeys.includes(item.id)}
                      onChange={(e) =>
                        setSelectedKeys((k) =>
                          e.target.checked ? [...k, item.id] : k.filter((x) => x !== item.id),
                        )
                      }
                    />
                    <img
                      src={item.imageUrl || '/assets/images/default-fish.png'}
                      alt={item.productName}
                      className={styles.itemImg}
                      onClick={() => item.productSlug ? navigate(`/products/${item.productSlug}`) : navigate('/danhmuc')}
                    />
                    <div className={styles.itemInfo}>
                      <span
                        className={styles.itemName}
                        onClick={() => item.productSlug ? navigate(`/products/${item.productSlug}`) : navigate('/danhmuc')}
                      >
                        {item.productName}
                      </span>
                      <span className={styles.itemPriceUnit}>{formatVND(item.unitPrice)}</span>
                    </div>

                    <div className={styles.itemQty}>
                      {updatingId === item.id ? (
                        <Spin size="small" />
                      ) : (
                        <InputNumber
                          min={1}
                          max={99}
                          value={item.quantity}
                          controls
                          size="middle"
                          onChange={(v) => handleUpdateQty(item.id, v || 1)}
                        />
                      )}
                    </div>

                    <div className={styles.itemSubtotal}>{formatVND(item.subTotal)}</div>

                    <Popconfirm
                      title="Xóa sản phẩm này?"
                      onConfirm={() => handleRemove(item.id)}
                      okText="Xóa"
                      cancelText="Hủy"
                    >
                      <button className={styles.removeBtn} title="Xóa">✕</button>
                    </Popconfirm>
                  </div>
                ))}
              </div>

              <Button
                icon={<ArrowLeftOutlined />}
                onClick={() => navigate('/danhmuc')}
                className={styles.continueShopping}
              >
                Tiếp tục mua hàng
              </Button>
            </Col>

            {/* ── Summary ── */}
            <Col xs={24} lg={8}>
              <div className={styles.summaryCard}>
                <h3 className={styles.summaryTitle}>Tóm tắt đơn hàng</h3>

                {/* Promo code */}
                <div className={styles.promoSection}>
                  <p className={styles.promoLabel}><TagOutlined /> Mã giảm giá</p>
                  {appliedPromo ? (
                    <div className={styles.appliedPromo}>
                      <Tag
                        color="green"
                        closable
                        onClose={() => { setAppliedPromo(null); setPromoCode(''); }}
                      >
                        {appliedPromo.code} - {appliedPromo.discountType === 'PERCENT'
                          ? `-${appliedPromo.discountValue}%`
                          : `-${formatVND(appliedPromo.discountValue)}`}
                      </Tag>
                    </div>
                  ) : (
                    <Space.Compact style={{ width: '100%' }}>
                      <Input
                        placeholder="Nhập mã giảm giá"
                        value={promoCode}
                        onChange={(e) => setPromoCode(e.target.value.toUpperCase())}
                        onPressEnter={handleApplyPromo}
                      />
                      <Button type="primary" onClick={handleApplyPromo}>Áp dụng</Button>
                    </Space.Compact>
                  )}
                  <div className={styles.promoHint}>
                    <GiftOutlined /> Nhập mã khuyến mãi để được giảm giá
                  </div>
                </div>

                <Divider />

                {/* Breakdown */}
                <div className={styles.breakdown}>
                  <div className={styles.breakRow}>
                    <span>Tạm tính ({selectedKeys.length} sản phẩm)</span>
                    <span>{formatVND(subtotal)}</span>
                  </div>
                  <div className={styles.breakRow}>
                    <span>Phí vận chuyển</span>
                    <span className={shipping === 0 && subtotal > 0 ? styles.freeShip : ''}>
                      {subtotal === 0 ? '—' : shipping === 0 ? 'Miễn phí' : fmt(shipping)}
                    </span>
                  </div>
                  {appliedPromo && (
                    <div className={`${styles.breakRow} ${styles.discount}`}>
                      <span>
                        Giảm giá ({appliedPromo.discountType === 'PERCENT'
                          ? `${appliedPromo.discountValue}%`
                          : formatVND(appliedPromo.discountValue)})
                      </span>
                      <span>-{formatVND(discountAmt)}</span>
                    </div>
                  )}
                  {subtotal > 0 && subtotal < FREE_SHIP_THRESHOLD && (
                    <div className={styles.freeShipHint}>
                      Mua thêm {formatVND(FREE_SHIP_THRESHOLD - subtotal)} để miễn phí ship
                    </div>
                  )}
                </div>

                <Divider />

                <div className={styles.totalRow}>
                  <span className={styles.totalLabel}>Tổng thanh toán</span>
                  <span className={styles.totalAmt}>{formatVND(grandTotal)}</span>
                </div>

                <Button
                  type="primary"
                  size="large"
                  block
                  icon={<ShoppingOutlined />}
                  disabled={selectedKeys.length === 0}
                  onClick={() => navigate('/checkout', { state: { appliedPromo } })}
                  className={styles.checkoutBtn}
                >
                  Tiến hành thanh toán
                </Button>

                <p className={styles.secureNote}>🔒 Thanh toán an toàn &amp; bảo mật</p>
              </div>
            </Col>
          </Row>
        ) : (
          <div className={styles.emptyWrap}>
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description={<span>Giỏ hàng của bạn đang trống</span>}
            >
              <Button
                type="primary"
                size="large"
                icon={<ShopOutlined />}
                onClick={() => navigate('/danhmuc')}
              >
                Mua sắm ngay
              </Button>
            </Empty>
          </div>
        )}
      </div>
    </CustomerLayout>
  );
};

export default Cart;
