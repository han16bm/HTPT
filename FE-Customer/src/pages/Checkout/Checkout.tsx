import React, { useEffect, useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import {
  Row, Col, Form, Input, Button, Radio, Steps,
  message, Divider, Breadcrumb, Result, Spin,
} from 'antd';
import {
  UserOutlined, PhoneOutlined, EnvironmentOutlined,
  CreditCardOutlined, CheckCircleOutlined, ShoppingOutlined,
  CarOutlined, BankOutlined,
} from '@ant-design/icons';
import { CustomerLayout } from '@/components/Layout';
import { cartService, orderService, type CartItem, type CreateOrderRequest, type PromotionDiscountType } from '@/api';
import { formatVND } from '@/utils/format';
import { resolveImageUrl, useImageFallback } from '@/utils/image';
import { calcShipping } from '@/constants/shipping';
import styles from './Checkout.module.scss';

const fmt = formatVND;

type PaymentMethod = 'cod' | 'bank';

interface AppliedPromo {
  code: string;
  discountType: PromotionDiscountType;
  discountValue: number;
}

const Checkout: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const appliedPromo = (location.state as { appliedPromo?: AppliedPromo } | null)?.appliedPromo ?? null;

  const [form] = Form.useForm();
  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [currentStep, setCurrentStep] = useState(0);
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('cod');
  const [orderId, setOrderId] = useState<number | null>(null);

  useEffect(() => { fetchCart(); }, []);

  const fetchCart = async () => {
    setLoading(true);
    try {
      const resp = await cartService.getCart();
      if (resp.success && resp.data) {
        if (resp.data.items.length === 0) {
          message.warning('Giỏ hàng trống');
          navigate('/cart');
          return;
        }
        setCartItems(resp.data.items);
        setTotal(resp.data.totalPrice);
        window.dispatchEvent(new CustomEvent('cart-updated', { detail: resp.data.totalItems }));
      }
    } catch {
      message.error('Không thể tải giỏ hàng');
    } finally {
      setLoading(false);
    }
  };

  const shipping = calcShipping(total);
  const discountAmt = appliedPromo
    ? appliedPromo.discountType === 'PERCENT'
      ? Math.round(total * appliedPromo.discountValue / 100)
      : Math.min(appliedPromo.discountValue, total)
    : 0;
  const grandTotal = total + shipping - discountAmt;

  const handleNextStep = async () => {
    try {
      await form.validateFields(['customerName', 'customerPhone', 'province', 'district', 'addressDetail']);
      setCurrentStep(1);
    } catch {
      // antd shows field-level errors
    }
  };

  const handleSubmit = async (values: {
    customerName: string;
    customerPhone: string;
    province: string;
    district: string;
    addressDetail: string;
    note?: string;
  }) => {
    setSubmitting(true);
    try {
      const orderData: CreateOrderRequest = {
        shippingAddress: `${values.customerName} | ${values.customerPhone} | ${values.addressDetail}, ${values.district}, ${values.province}`,
        paymentMethod: paymentMethod === 'cod' ? 'COD' : 'BANK_TRANSFER',
        note: values.note,
        shippingFee: shipping,
        promotionCode: appliedPromo?.code,
      };
      const resp = await orderService.createOrder(orderData);
      if (resp.success && resp.data) {
        setOrderId(resp.data.id);
        setCurrentStep(2);
        message.success('Đặt hàng thành công!');
      } else {
        message.error(resp.error || 'Không thể đặt hàng');
      }
    } catch {
      message.error('Đã xảy ra lỗi');
    } finally {
      setSubmitting(false);
    }
  };

  const steps = [
    { title: 'Thông tin', icon: <UserOutlined /> },
    { title: 'Thanh toán', icon: <CreditCardOutlined /> },
    { title: 'Hoàn thành', icon: <CheckCircleOutlined /> },
  ];

  if (loading) {
    return (
      <CustomerLayout>
        <div className={styles.loadingWrap}><Spin size="large" /></div>
      </CustomerLayout>
    );
  }

  return (
    <CustomerLayout>
      <div className={styles.checkout}>
        <Breadcrumb
          className={styles.breadcrumb}
          items={[
            { title: <Link to="/">Trang chủ</Link> },
            { title: <Link to="/cart">Giỏ hàng</Link> },
            { title: 'Thanh toán' },
          ]}
        />

        <h1 className={styles.title}>Thanh toán</h1>

        <Steps current={currentStep} items={steps} className={styles.steps} />

        {currentStep < 2 ? (
          <Form form={form} layout="vertical" onFinish={handleSubmit} autoComplete="off">
            <Row gutter={[24, 0]} align="top">
              {/* Left: form steps */}
              <Col xs={24} lg={15}>

                {/* Step 0: Delivery info */}
                <div className={currentStep !== 0 ? styles.hidden : ''}>
                  <div className={styles.formCard}>
                    <h3 className={styles.cardTitle}><UserOutlined /> Thông tin nhận hàng</h3>

                    <Row gutter={16}>
                      <Col xs={24} sm={12}>
                        <Form.Item
                          label="Họ và tên"
                          name="customerName"
                          rules={[{ required: true, message: 'Vui lòng nhập họ tên' }]}
                        >
                          <Input size="large" prefix={<UserOutlined />} placeholder="Nguyễn Văn A" />
                        </Form.Item>
                      </Col>
                      <Col xs={24} sm={12}>
                        <Form.Item
                          label="Số điện thoại"
                          name="customerPhone"
                          rules={[
                            { required: true, message: 'Vui lòng nhập số điện thoại' },
                            { pattern: /^(0[3|5|7|8|9])+([0-9]{8})\b/, message: 'Số điện thoại không hợp lệ' },
                          ]}
                        >
                          <Input size="large" prefix={<PhoneOutlined />} placeholder="0901234567" />
                        </Form.Item>
                      </Col>
                    </Row>

                    <Form.Item
                      label="Email (không bắt buộc)"
                      name="email"
                      rules={[{ type: 'email', message: 'Email không hợp lệ' }]}
                    >
                      <Input size="large" placeholder="example@email.com" />
                    </Form.Item>

                    <Row gutter={16}>
                      <Col xs={24} sm={12}>
                        <Form.Item
                          label="Tỉnh / Thành phố"
                          name="province"
                          rules={[{ required: true, message: 'Vui lòng nhập tỉnh/thành phố' }]}
                        >
                          <Input size="large" prefix={<EnvironmentOutlined />} placeholder="TP. Hồ Chí Minh" />
                        </Form.Item>
                      </Col>
                      <Col xs={24} sm={12}>
                        <Form.Item
                          label="Quận / Huyện"
                          name="district"
                          rules={[{ required: true, message: 'Vui lòng nhập quận/huyện' }]}
                        >
                          <Input size="large" placeholder="Quận 1" />
                        </Form.Item>
                      </Col>
                    </Row>

                    <Form.Item
                      label="Địa chỉ cụ thể"
                      name="addressDetail"
                      rules={[{ required: true, message: 'Vui lòng nhập địa chỉ' }]}
                    >
                      <Input size="large" prefix={<EnvironmentOutlined />} placeholder="Số nhà, tên đường, phường/xã" />
                    </Form.Item>

                    <Form.Item label="Ghi chú đơn hàng" name="note">
                      <Input.TextArea rows={2} placeholder="Giao giờ hành chính, để trước cửa..." />
                    </Form.Item>

                    <Button type="primary" size="large" block onClick={handleNextStep}>
                      Tiếp theo: Chọn thanh toán →
                    </Button>
                  </div>
                </div>

                {/* Step 1: Payment */}
                <div className={currentStep !== 1 ? styles.hidden : ''}>
                  <div className={styles.formCard}>
                    <h3 className={styles.cardTitle}><CreditCardOutlined /> Phương thức thanh toán</h3>

                    <Radio.Group
                      value={paymentMethod}
                      onChange={(e) => setPaymentMethod(e.target.value)}
                      className={styles.payRadioGroup}
                    >
                      <label
                        className={`${styles.payOption} ${paymentMethod === 'cod' ? styles.paySelected : ''}`}
                      >
                        <Radio value="cod" />
                        <span className={styles.payIcon}>💵</span>
                        <div className={styles.payText}>
                          <div className={styles.payName}>Thanh toán khi nhận hàng (COD)</div>
                          <div className={styles.payDesc}>Trả tiền mặt khi nhận hàng</div>
                        </div>
                      </label>

                      <label
                        className={`${styles.payOption} ${paymentMethod === 'bank' ? styles.paySelected : ''}`}
                      >
                        <Radio value="bank" />
                        <span className={styles.payIcon}>🏦</span>
                        <div className={styles.payText}>
                          <div className={styles.payName}>Chuyển khoản ngân hàng</div>
                          <div className={styles.payDesc}>Vietcombank / Techcombank / MBBank</div>
                        </div>
                      </label>
                    </Radio.Group>

                    {paymentMethod === 'bank' && (
                      <div className={styles.bankInfo}>
                        <p className={styles.bankInfoTitle}>
                          <BankOutlined /> Thông tin chuyển khoản
                        </p>

                        <div className={styles.bankBody}>
                          {/* Account details */}
                          <div className={styles.bankDetails}>
                            <div className={styles.bankRow}>
                              <span className={styles.bankKey}>Ngân hàng</span>
                              <strong className={styles.bankVal}>Vietcombank</strong>
                            </div>
                            <div className={styles.bankRow}>
                              <span className={styles.bankKey}>Số tài khoản</span>
                              <strong className={styles.bankVal}>1234 5678 9012</strong>
                            </div>
                            <div className={styles.bankRow}>
                              <span className={styles.bankKey}>Chủ tài khoản</span>
                              <strong className={styles.bankVal}>CONG TY TNHH H&amp;H FISH SHOP</strong>
                            </div>
                            <div className={`${styles.bankRow} ${styles.bankNote}`}>
                              <span className={styles.bankKey}>Nội dung CK</span>
                              <strong className={styles.bankVal}>Họ tên + Số điện thoại</strong>
                            </div>
                          </div>

                          {/* QR code */}
                          <div className={styles.qrWrap}>
                            <img
                              src="/assets/images/slides/qr-code.png"
                              alt="QR chuyển khoản"
                              className={styles.qrImg}
                              onError={e => {
                                (e.target as HTMLImageElement).style.display = 'none';
                              }}
                            />
                            <span className={styles.qrLabel}>Quét mã để chuyển khoản</span>
                          </div>
                        </div>
                      </div>
                    )}

                    <div className={styles.stepBtns}>
                      <Button size="large" onClick={() => setCurrentStep(0)}>← Quay lại</Button>
                      <Button
                        type="primary"
                        size="large"
                        htmlType="submit"
                        loading={submitting}
                        icon={<CheckCircleOutlined />}
                      >
                        Đặt hàng ngay
                      </Button>
                    </div>
                  </div>
                </div>
              </Col>

              {/* Right: Order summary */}
              <Col xs={24} lg={9}>
                <div className={styles.summaryCard}>
                  <h3 className={styles.cardTitle}><ShoppingOutlined /> Đơn hàng của bạn</h3>

                  <div className={styles.orderItems}>
                    {cartItems.map((item) => (
                      <div key={item.id} className={styles.orderItemRow}>
                        <img
                          src={resolveImageUrl(item.imageUrl)}
                          alt={item.productName}
                          className={styles.orderItemImg}
                          onError={useImageFallback}
                        />
                        <div className={styles.orderItemDetail}>
                          <span className={styles.orderItemName}>{item.productName}</span>
                          <span className={styles.orderItemQty}>x{item.quantity}</span>
                        </div>
                        <span className={styles.orderItemPrice}>{formatVND(item.subTotal)}</span>
                      </div>
                    ))}
                  </div>

                  <Divider style={{ margin: '14px 0' }} />

                  <div className={styles.summaryRows}>
                    <div className={styles.sumRow}>
                      <span>Tạm tính</span><span>{formatVND(total)}</span>
                    </div>
                    <div className={styles.sumRow}>
                      <span>Phí vận chuyển</span>
                      <span className={shipping === 0 ? styles.freeShip : ''}>
                        {shipping === 0 ? 'Miễn phí' : fmt(shipping)}
                      </span>
                    </div>
                    {appliedPromo && (
                      <div className={styles.sumRow}>
                        <span>
                          Giảm giá ({appliedPromo.discountType === 'PERCENT'
                            ? `${appliedPromo.discountValue}%`
                            : fmt(appliedPromo.discountValue)})
                        </span>
                        <span style={{ color: '#4ade80' }}>-{fmt(discountAmt)}</span>
                      </div>
                    )}
                    <div className={styles.sumRow}>
                      <span>Phương thức</span>
                      <span>{paymentMethod === 'cod' ? 'COD' : 'Chuyển khoản'}</span>
                    </div>
                  </div>

                  <Divider style={{ margin: '14px 0' }} />

                  <div className={styles.grandTotal}>
                    <span>Tổng cộng</span>
                    <span className={styles.grandAmt}>{formatVND(grandTotal)}</span>
                  </div>

                  <p className={styles.shippingNote}>
                    <CarOutlined /> Giao hàng trong 1-3 ngày làm việc
                  </p>
                </div>
              </Col>
            </Row>
          </Form>
        ) : (
          /* Step 2: Success */
          <div className={styles.successWrap}>
            <Result
              status="success"
              title="Đặt hàng thành công! 🎉"
              subTitle={
                <div>
                  <p>Mã đơn hàng: <strong>#{orderId}</strong></p>
                  <p>Chúng tôi sẽ liên hệ xác nhận trong vòng 30 phút.</p>
                </div>
              }
              extra={[
                <Button
                  key="orders"
                  type="primary"
                  size="large"
                  icon={<ShoppingOutlined />}
                  onClick={() => navigate('/my-orders')}
                >
                  Xem đơn hàng
                </Button>,
                <Button key="shop" size="large" onClick={() => navigate('/danhmuc')}>
                  Tiếp tục mua hàng
                </Button>,
              ]}
            />
          </div>
        )}
      </div>
    </CustomerLayout>
  );
};

export default Checkout;
