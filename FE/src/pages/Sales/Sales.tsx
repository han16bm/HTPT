import React, { useCallback, useState } from 'react';
import {
  AutoComplete,
  Button,
  Card,
  Divider,
  Empty,
  Form,
  Input,
  InputNumber,
  Modal,
  Radio,
  Space,
  Tag,
  Typography,
  message,
} from 'antd';
import {
  DeleteOutlined,
  PlusOutlined,
  BankOutlined,
  SearchOutlined,
  ShoppingCartOutlined,
  TagOutlined,
  UserAddOutlined,
} from '@ant-design/icons';
import classNames from 'classnames/bind';
import styles from './Sales.module.scss';
import { MasterLayout } from '@/components/Layout';
import { customerService, orderService, productService, promotionService } from '@/api';
import { usePagedQuery } from '@/hooks';
import { formatCurrency } from '@/shared/utils/format';
import { IMAGE_PLACEHOLDER, resolveProductImageUrl } from '@/shared/utils/image';
import type {
  Customer,
  OrderCreateRequest,
  PaymentMethod,
  Product,
} from '@/interfaces';

const cx = classNames.bind(styles);
const { Text } = Typography;

interface CartEntry {
  product: Product;
  quantity: number;
}

const Sales: React.FC = () => {
  const [messageApi, contextHolder] = message.useMessage();

  const [searchInput, setSearchInput] = useState('');
  const {
    items: products,
    loading: productsLoading,
    setParams,
  } = usePagedQuery<Product, { search?: string; page?: number; pageSize?: number }>({
    fetcher: (params) => productService.search(params),
    initialParams: { page: 1, pageSize: 24 },
    errorMessage: 'Lỗi khi tải sản phẩm',
  });

  const [customerKeyword, setCustomerKeyword] = useState('');
  const [customerOptions, setCustomerOptions] = useState<
    { value: string; label: string; customer: Customer }[]
  >([]);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);

  // ── New customer modal ──
  const [newCustOpen, setNewCustOpen] = useState(false);
  const [newCustForm] = Form.useForm<{ fullName: string; phone: string; email?: string }>();
  const [creatingCust, setCreatingCust] = useState(false);

  const handleCreateCustomer = async () => {
    try {
      const values = await newCustForm.validateFields();
      setCreatingCust(true);
      const created = await customerService.createWalkIn(values);
      setSelectedCustomer(created);
      setCustomerKeyword(`${created.fullName} — ${created.phone}`);
      setNewCustOpen(false);
      newCustForm.resetFields();
      messageApi.success(`Đã tạo khách hàng: ${created.fullName}`);
    } catch (err: unknown) {
      const e = err as { message?: string; errorFields?: unknown[] };
      if (!e.errorFields) {
        messageApi.error(e.message || 'Không thể tạo khách hàng');
      }
    } finally {
      setCreatingCust(false);
    }
  };

  const searchCustomers = useCallback(async (keyword: string) => {
    if (!keyword.trim()) {
      setCustomerOptions([]);
      return;
    }

    try {
      const data = await customerService.search({ keyword, pageSize: 10 });
      setCustomerOptions(
        (data.items ?? []).map((customer) => ({
          value: customer.id.toString(),
          label: `${customer.fullName} — ${customer.phone}`,
          customer,
        }))
      );
    } catch {
      setCustomerOptions([]);
    }
  }, []);

  const [cart, setCart] = useState<CartEntry[]>([]);
  const [promoCode, setPromoCode] = useState('');
  const [promoDiscount, setPromoDiscount] = useState(0);
  const [promoApplied, setPromoApplied] = useState(false);
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('CASH');
  const [note, setNote] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const subtotal = cart.reduce((sum, item) => sum + item.product.salePrice * item.quantity, 0);
  const grandTotal = Math.max(subtotal - promoDiscount, 0);

  const addToCart = (product: Product) => {
    if (product.stockQuantity <= 0) {
      messageApi.warning('Sản phẩm đã hết hàng');
      return;
    }

    setCart((prev) => {
      const found = prev.find((item) => item.product.id === product.id);
      if (found) {
        if (found.quantity >= product.stockQuantity) {
          messageApi.warning('Không đủ tồn kho');
          return prev;
        }

        return prev.map((item) =>
          item.product.id === product.id ? { ...item, quantity: item.quantity + 1 } : item
        );
      }

      return [...prev, { product, quantity: 1 }];
    });
  };

  const updateQty = (productId: number, qty: number) => {
    if (qty <= 0) {
      setCart((prev) => prev.filter((item) => item.product.id !== productId));
      return;
    }

    setCart((prev) =>
      prev.map((item) => (item.product.id === productId ? { ...item, quantity: qty } : item))
    );
  };

  const removeFromCart = (productId: number) =>
    setCart((prev) => prev.filter((item) => item.product.id !== productId));

  const handleVerifyPromo = async () => {
    if (!promoCode.trim()) {
      messageApi.warning('Vui lòng nhập mã giảm giá');
      return;
    }

    if (cart.length === 0) {
      messageApi.warning('Hãy thêm sản phẩm trước khi áp mã');
      return;
    }

    try {
      const result = await promotionService.verify(promoCode.trim(), subtotal);
      setPromoDiscount(result.discountAmount);
      setPromoApplied(true);
      messageApi.success(`Áp dụng thành công! Giảm ${formatCurrency(result.discountAmount)}`);
    } catch (err) {
      messageApi.error((err as { message?: string }).message || 'Mã không hợp lệ');
      setPromoDiscount(0);
      setPromoApplied(false);
    }
  };

  const removePromo = () => {
    setPromoCode('');
    setPromoDiscount(0);
    setPromoApplied(false);
  };

  const [bankPayModalOpen, setBankPayModalOpen] = useState(false);

  // Build the order body (shared between CASH and bank-confirm paths)
  const buildOrderBody = (): OrderCreateRequest => ({
    customerId: selectedCustomer!.id,
    customerName: selectedCustomer!.fullName,
    customerPhone: selectedCustomer!.phone,
    customerAddress: 'Tại quầy',
    note: note || undefined,
    paymentMethod,
    source: 'POS',
    promoCode: promoApplied ? promoCode : undefined,
    items: cart.map((item) => ({
      productId: item.product.id,
      quantity: item.quantity,
      unitPrice: item.product.salePrice,
    })),
  });

  const resetOrder = () => {
    setCart([]);
    setSelectedCustomer(null);
    setCustomerKeyword('');
    removePromo();
    setNote('');
  };

  const handleCheckout = async () => {
    if (!selectedCustomer) { messageApi.error('Vui lòng chọn khách hàng'); return; }
    if (cart.length === 0) { messageApi.error('Giỏ hàng đang trống'); return; }

    // Bank transfer: show QR modal instead of creating order immediately
    if (paymentMethod === 'BANK_TRANSFER') {
      setBankPayModalOpen(true);
      return;
    }

    // Cash: create order directly
    setSubmitting(true);
    try {
      await orderService.createDirect(buildOrderBody());
      messageApi.success('Thanh toán thành công');
      resetOrder();
    } catch (err) {
      messageApi.error((err as { message?: string }).message || 'Thanh toán thất bại');
    } finally {
      setSubmitting(false);
    }
  };

  const handleConfirmBankTransfer = async () => {
    setSubmitting(true);
    try {
      await orderService.createDirect(buildOrderBody());
      setBankPayModalOpen(false);
      messageApi.success('Xác nhận chuyển khoản thành công!');
      resetOrder();
    } catch (err) {
      messageApi.error((err as { message?: string }).message || 'Tạo đơn hàng thất bại');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <MasterLayout>
      {contextHolder}
      <div className={cx('salesPage')}>
        <div className={cx('salesContainer')}>
          <Card
            className={cx('productsSection')}
            title={(
              <span>
                <ShoppingCartOutlined /> Chọn sản phẩm
              </span>
            )}
            loading={productsLoading}
          >
            <Input
              placeholder="Tìm sản phẩm theo tên..."
              prefix={<SearchOutlined />}
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              onPressEnter={() => setParams({ search: searchInput, page: 1 })}
              allowClear
              onClear={() => {
                setSearchInput('');
                setParams({ search: undefined, page: 1 });
              }}
              style={{ marginBottom: 16 }}
            />

            <div className={cx('productGrid')}>
              {products.length === 0 && !productsLoading && (
                <Empty description="Không có sản phẩm" />
              )}

              {products.map((product) => (
                <div
                  key={product.id}
                  className={cx('productCard', { outOfStock: product.stockQuantity <= 0 })}
                  onClick={() => addToCart(product)}
                  role="button"
                  tabIndex={0}
                  onKeyDown={(e) => e.key === 'Enter' && addToCart(product)}
                >
                  <img
                    src={resolveProductImageUrl(product.imageUrl) ?? IMAGE_PLACEHOLDER}
                    alt={product.name}
                    className={cx('productImg')}
                    onError={(e) => { e.currentTarget.src = IMAGE_PLACEHOLDER; }}
                  />
                  <div className={cx('productName')}>{product.name}</div>
                  <div className={cx('productPrice')}>{formatCurrency(product.salePrice)}</div>
                  <Tag
                    color={product.stockQuantity <= 0 ? 'red' : product.stockQuantity <= 10 ? 'orange' : 'green'}
                    className={cx('stockTag')}
                  >
                    Tồn: {product.stockQuantity}
                  </Tag>
                </div>
              ))}
            </div>
          </Card>

          <Card className={cx('cartSection')} title="Hóa đơn">
            <div className={cx('fieldLabel')} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <span>Khách hàng *</span>
              <Button
                size="small"
                type="link"
                icon={<PlusOutlined />}
                onClick={() => setNewCustOpen(true)}
                style={{ padding: 0, fontSize: 12 }}
              >
                Thêm khách mới
              </Button>
            </div>
            <AutoComplete
              style={{ width: '100%', marginBottom: 12 }}
              options={customerOptions}
              value={customerKeyword}
              onChange={(value) => {
                setCustomerKeyword(value);
                searchCustomers(value);
              }}
              onSelect={(_, option) => {
                const customerOption = option as { customer: Customer; label: string };
                setSelectedCustomer(customerOption.customer);
                setCustomerKeyword(customerOption.label);
              }}
              placeholder="Tìm theo tên hoặc SDT..."
              allowClear
              onClear={() => {
                setSelectedCustomer(null);
                setCustomerKeyword('');
              }}
            />

            {selectedCustomer && (
              <div className={cx('customerBadge')}>
                <Text strong>{selectedCustomer.fullName}</Text>
                <Text type="secondary"> — {selectedCustomer.phone}</Text>
                {selectedCustomer.customerCode && (
                  <Tag style={{ marginLeft: 6 }}>{selectedCustomer.customerCode}</Tag>
                )}
              </div>
            )}

            <Divider style={{ margin: '12px 0' }} />

            <div className={cx('cartItems')}>
              {cart.length === 0 ? (
                <Empty
                  description="Click vào sản phẩm bên trái để thêm"
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                />
              ) : (
                cart.map((item) => (
                  <div key={item.product.id} className={cx('cartItem')}>
                    <div className={cx('itemInfo')}>
                      <span className={cx('itemName')}>{item.product.name}</span>
                      <span className={cx('itemUnit')}>
                        {formatCurrency(item.product.salePrice)} / sp
                      </span>
                    </div>
                    <div className={cx('itemControls')}>
                      <InputNumber<number>
                        min={1}
                        max={item.product.stockQuantity}
                        value={item.quantity}
                        onChange={(value) => updateQty(item.product.id, value || 1)}
                        size="small"
                        style={{ width: 70 }}
                      />
                      <Button
                        type="text"
                        danger
                        size="small"
                        icon={<DeleteOutlined />}
                        onClick={() => removeFromCart(item.product.id)}
                      />
                    </div>
                    <div className={cx('itemSubtotal')}>
                      {formatCurrency(item.product.salePrice * item.quantity)}
                    </div>
                  </div>
                ))
              )}
            </div>

            <Divider style={{ margin: '12px 0' }} />

            <div className={cx('fieldLabel')}>
              <TagOutlined /> Mã giảm giá
            </div>
            {promoApplied ? (
              <Tag
                color="green"
                closable
                onClose={removePromo}
                style={{ marginBottom: 8, fontSize: 13 }}
              >
                {promoCode} — -{formatCurrency(promoDiscount)}
              </Tag>
            ) : (
              <Space.Compact style={{ width: '100%', marginBottom: 8 }}>
                <Input
                  placeholder="Nhập mã..."
                  value={promoCode}
                  onChange={(e) => setPromoCode(e.target.value.toUpperCase())}
                  onPressEnter={handleVerifyPromo}
                />
                <Button onClick={handleVerifyPromo}>Áp dụng</Button>
              </Space.Compact>
            )}

            <div className={cx('fieldLabel')}>Thanh toán</div>
            <Radio.Group
              value={paymentMethod}
              onChange={(e) => setPaymentMethod(e.target.value as PaymentMethod)}
              style={{ marginBottom: 8 }}
            >
              <Radio.Button value="CASH">Tiền mặt</Radio.Button>
              <Radio.Button value="BANK_TRANSFER">Chuyển khoản</Radio.Button>
            </Radio.Group>


            <Input.TextArea
              placeholder="Ghi chú đơn hàng..."
              value={note}
              onChange={(e) => setNote(e.target.value)}
              rows={2}
              style={{ marginBottom: 12 }}
            />

            <div className={cx('summaryBlock')}>
              <div className={cx('sumRow')}>
                <span>Tạm tính ({cart.length} SP):</span>
                <span>{formatCurrency(subtotal)}</span>
              </div>
              {promoApplied && (
                <div className={cx('sumRow', 'discount')}>
                  <span>Giảm giá:</span>
                  <span>-{formatCurrency(promoDiscount)}</span>
                </div>
              )}
              <div className={cx('sumRow', 'grand')}>
                <span>Tổng cộng:</span>
                <strong>{formatCurrency(grandTotal)}</strong>
              </div>
            </div>

            <Button
              type="primary"
              size="large"
              block
              onClick={handleCheckout}
              loading={submitting}
              disabled={cart.length === 0 || !selectedCustomer}
              className={cx('checkoutBtn')}
            >
              THANH TOÁN
            </Button>
          </Card>
        </div>
      </div>

      {/* ── Bank transfer QR confirmation modal ── */}
      <Modal
        open={bankPayModalOpen}
        onCancel={() => setBankPayModalOpen(false)}
        footer={null}
        width={480}
        centered
        destroyOnClose
        title={(
          <span style={{ display: 'flex', alignItems: 'center', gap: 8, color: '#0369a1' }}>
            <BankOutlined style={{ fontSize: 18 }} />
            Chuyển khoản
          </span>
        )}
      >
        <div className={cx('qrModal')}>
          {/* Amount */}
          <div className={cx('qrAmount')}>
            <span className={cx('qrAmountLabel')}>Số tiền cần chuyển</span>
            <span className={cx('qrAmountValue')}>{formatCurrency(grandTotal)}</span>
          </div>

          {/* QR code */}
          <div className={cx('qrCodeWrap')}>
            <img
              src="/assets/images/qr-code.png"
              alt="QR chuyển khoản"
              className={cx('qrCodeImg')}
              onError={(e) => { (e.target as HTMLImageElement).style.display = 'none'; }}
            />
            <p className={cx('qrScanHint')}>Quét mã QR để chuyển khoản</p>
          </div>

          {/* Bank details */}
          <div className={cx('qrBankDetails')}>
            <div className={cx('qrBankRow')}>
              <span>Ngân hàng</span>
              <strong>Vietcombank</strong>
            </div>
            <div className={cx('qrBankRow')}>
              <span>Số tài khoản</span>
              <strong>1234 5678 9012</strong>
            </div>
            <div className={cx('qrBankRow')}>
              <span>Chủ tài khoản</span>
              <strong>CONG TY TNHH H&amp;H FISH SHOP</strong>
            </div>
            <div className={cx('qrBankRow', 'qrBankRowHighlight')}>
              <span>Nội dung CK</span>
              <strong>
                {selectedCustomer?.fullName} {selectedCustomer?.phone}
              </strong>
            </div>
          </div>

          {/* Action buttons */}
          <div className={cx('qrActions')}>
            <Button
              size="large"
              onClick={() => setBankPayModalOpen(false)}
              style={{ flex: 1 }}
            >
              ❌ Huỷ 
            </Button>
            <Button
              type="primary"
              size="large"
              loading={submitting}
              onClick={handleConfirmBankTransfer}
              style={{ flex: 2, background: '#16a34a', borderColor: '#16a34a' }}
            >
              ✅ Xác nhận thanh toán
            </Button>
          </div>
        </div>
      </Modal>

      {/* ── Quick-create customer modal ── */}
      <Modal
        title={(
          <span><UserAddOutlined style={{ marginRight: 8 }} />Thêm khách hàng mới</span>
        )}
        open={newCustOpen}
        onCancel={() => { setNewCustOpen(false); newCustForm.resetFields(); }}
        onOk={handleCreateCustomer}
        okText="Tạo khách hàng"
        cancelText="Huỷ"
        confirmLoading={creatingCust}
        destroyOnClose
      >
        <Form form={newCustForm} layout="vertical" style={{ marginTop: 12 }}>
          <Form.Item
            label="Họ và tên"
            name="fullName"
            rules={[{ required: true, message: 'Vui lòng nhập họ tên' }]}
          >
            <Input placeholder="Nguyen Van A" autoFocus />
          </Form.Item>

          <Form.Item
            label="Số điện thoại"
            name="phone"
            rules={[
              { required: true, message: 'Vui lòng nhập số điện thoại' },
              { pattern: /^[0-9]{9,11}$/, message: 'SĐT không hợp lệ' },
            ]}
          >
            <Input placeholder="0912345678" maxLength={11} />
          </Form.Item>

          <Form.Item
            label="Email (tuỳ chọn)"
            name="email"
            rules={[{ type: 'email', message: 'Email không hợp lệ' }]}
          >
            <Input placeholder="example@gmail.com" />
          </Form.Item>
        </Form>
      </Modal>
    </MasterLayout>
  );
};

export default Sales;
