import React, { useState } from 'react';
import { Row, Col, Collapse } from 'antd';
import {
  CarOutlined,
  SwapOutlined,
  SafetyCertificateOutlined,
  LockOutlined,
  CreditCardOutlined,
  QuestionCircleOutlined,
  PhoneOutlined,
  MailOutlined,
  MessageOutlined,
} from '@ant-design/icons';
import { CustomerLayout } from '@/components/Layout';
import styles from './Policies.module.scss';

interface PolicySection {
  key: string;
  label: string;
  icon: React.ReactNode;
  emoji: string;
  title: string;
  intro: string;
  color: string;
  items: { q: string; a: string }[];
}

const POLICIES: PolicySection[] = [
  {
    key: 'shipping',
    label: 'Vận Chuyển',
    icon: <CarOutlined />,
    emoji: '🚚',
    title: 'Chính Sách Vận Chuyển',
    intro: 'H&H Fish Shop cam kết giao hàng nhanh chóng, an toàn đến tay khách hàng trên toàn quốc.',
    color: '#1890ff',
    items: [
      { q: 'Thời gian giao hàng là bao lâu?', a: 'Nội thành Hà Nội: 2-4 giờ (giao trong ngày). Các tỉnh thành khác: 1-3 ngày làm việc. Vùng sâu xa: 3-5 ngày.' },
      { q: 'Chi phí giao hàng như thế nào?', a: 'Miễn phí giao hàng toàn quốc cho đơn hàng từ 500.000đ. Đơn dưới 500.000đ: phí 30.000đ – 50.000đ tùy khu vực.' },
      { q: 'Cá cảnh được vận chuyển như thế nào?', a: 'Cá được đóng túi oxy chuyên dụng, kết hợp xốp cách nhiệt để đảm bảo cá sống khỏe khi đến tay khách hàng.' },
      { q: 'Có giao hàng vào ngày lễ không?', a: 'Chúng tôi giao hàng cả thứ 7, Chủ nhật. Ngày lễ sẽ thông báo trước trên website.' },
    ],
  },
  {
    key: 'return',
    label: 'Đổi Trả',
    icon: <SwapOutlined />,
    emoji: '🔄',
    title: 'Chính Sách Đổi Trả',
    intro: 'Chúng tôi cam kết đổi trả sản phẩm đảm bảo quyền lợi tốt nhất cho khách hàng.',
    color: '#52c41a',
    items: [
      { q: 'Điều kiện đổi trả như thế nào?', a: 'Đổi trả trong 7 ngày kể từ ngày nhận hàng nếu: cá chết, sản phẩm lỗi kỹ thuật, giao sai hàng. Cần video/ảnh làm bằng chứng.' },
      { q: 'Hàng nào không được đổi trả?', a: 'Sản phẩm thức ăn, thuốc đã mở. Cá chết do nuôi sai hướng dẫn. Hàng giảm giá trên 50%.' },
      { q: 'Quy trình đổi trả như thế nào?', a: 'Liên hệ hotline 0853997698 → Cung cấp đơn hàng + ảnh/video → Xác nhận trong 24h → Gửi hàng đổi trong 3 ngày.' },
      { q: 'Ai chịu phí vận chuyển đổi trả?', a: 'H&H chịu phí vận chuyển đổi trả nếu lỗi từ phía chúng tôi. Khách hàng chịu phí nếu lý do cá nhân.' },
    ],
  },
  {
    key: 'warranty',
    label: 'Bảo Hành',
    icon: <SafetyCertificateOutlined />,
    emoji: '🛡️',
    title: 'Chính Sách Bảo Hành',
    intro: 'H&H Fish Shop cung cấp bảo hành đầy đủ cho tất cả các sản phẩm phụ kiện.',
    color: '#fa8c16',
    items: [
      { q: 'Thời gian bảo hành sản phẩm?', a: 'Máy lọc nước: 12 tháng. Đèn LED: 6 tháng. Máy sục khí: 6 tháng. Bể cá kính: 3 tháng. Phụ kiện khác: 3 tháng.' },
      { q: 'Bảo hành bao gồm những gì?', a: 'Lỗi kỹ thuật từ nhà sản xuất, không hoạt động đúng chức năng. Không bao gồm hư hỏng do người dùng, rơi vỡ, nước vào.' },
      { q: 'Cách tiến hành bảo hành?', a: 'Mang sản phẩm đến cửa hàng hoặc gửi đến địa chỉ: 123 Nguyễn Trãi Q.1 kèm hóa đơn mua hàng.' },
    ],
  },
  {
    key: 'payment',
    label: 'Thanh Toán',
    icon: <CreditCardOutlined />,
    emoji: '💳',
    title: 'Chính Sách Thanh Toán',
    intro: 'H&H Fish Shop hỗ trợ nhiều hình thức thanh toán linh hoạt, an toàn và tiện lợi.',
    color: '#722ed1',
    items: [
      { q: 'Có những hình thức thanh toán nào?', a: 'Thanh toán khi nhận hàng (COD). Chuyển khoản ngân hàng.' },
      { q: 'Thanh toán COD có giới hạn không?', a: 'COD áp dụng cho đơn hàng dưới 5.000.000đ. Với đơn lớn hơn, vui lòng chuyển khoản hoặc thanh toán trước 30% đặt cọc.' },
      { q: 'Có được hoàn tiền không?', a: 'Hoàn tiền trong 3-5 ngày làm việc qua ngân hàng hoặc ví điện tử trong trường hợp đơn hàng bị hủy hoặc lỗi từ phía chúng tôi.' },
      { q: 'Thông tin thanh toán có an toàn không?', a: 'Chúng tôi sử dụng mã hóa SSL 256-bit. Không lưu trữ thông tin thẻ ngân hàng. Tuân thủ tiêu chuẩn bảo mật PCI DSS.' },
    ],
  },
  {
    key: 'privacy',
    label: 'Bảo Mật',
    icon: <LockOutlined />,
    emoji: '🔒',
    title: 'Chính Sách Bảo Mật',
    intro: 'Bảo mật thông tin khách hàng là ưu tiên hàng đầu của H&H Fish Shop.',
    color: '#13c2c2',
    items: [
      { q: 'Thông tin nào được thu thập?', a: 'Tên, số điện thoại, email, địa chỉ giao hàng. Lịch sử mua hàng để cải thiện trải nghiệm. Chúng tôi KHÔNG lưu thông tin thẻ ngân hàng.' },
      { q: 'Thông tin được sử dụng như thế nào?', a: 'Xử lý đơn hàng và giao hàng. Gửi thông báo đơn hàng và khuyến mãi (có thể hủy đăng ký). Cải thiện dịch vụ.' },
      { q: 'Thông tin có được chia sẻ không?', a: 'Chúng tôi không bán, cho thuê hoặc chia sẻ thông tin cá nhân. Chỉ chia sẻ với đơn vị vận chuyển để thực hiện giao hàng.' },
      { q: 'Làm sao để xóa tài khoản?', a: 'Liên hệ email support@hhfishshop.vn để yêu cầu xóa tài khoản và toàn bộ dữ liệu trong vòng 30 ngày.' },
    ],
  },
];

const Policies: React.FC = () => {
  const [activeKey, setActiveKey] = useState('shipping');

  const active = POLICIES.find((p) => p.key === activeKey)!;

  return (
    <CustomerLayout>
      <div className={styles.policiesPage}>

        {/* ── Hero Banner ── */}
        <div className={styles.heroBanner}>
          <div className={styles.bannerOverlay} />
          <div className={styles.bannerContent}>
            <div className={styles.bannerEmoji}>📋</div>
            <h1 className={styles.bannerTitle}>Chính Sách Cửa Hàng</h1>
            <p className={styles.bannerSubtitle}>
              Cam kết mang đến trải nghiệm mua cá cảnh tốt nhất cho khách hàng
            </p>
          </div>
        </div>

        {/* ── Quick-select cards ── */}
        <div className={styles.policyNav}>
          {POLICIES.map((p) => (
            <button
              key={p.key}
              className={`${styles.navCard} ${activeKey === p.key ? styles.navCardActive : ''}`}
              style={activeKey === p.key ? { borderColor: p.color, boxShadow: `0 4px 18px ${p.color}44` } : {}}
              onClick={() => setActiveKey(p.key)}
            >
              <span className={styles.navEmoji}>{p.emoji}</span>
              <span className={styles.navLabel} style={activeKey === p.key ? { color: p.color } : {}}>
                {p.label}
              </span>
            </button>
          ))}
        </div>

        {/* ── Policy detail ── */}
        <div className={styles.policyPanel}>
          <div className={styles.panelHeader} style={{ borderLeftColor: active.color }}>
            <div className={styles.panelIcon} style={{ background: `${active.color}22`, color: active.color }}>
              {active.icon}
            </div>
            <div>
              <h2 className={styles.panelTitle}>{active.title}</h2>
              <p className={styles.panelIntro}>{active.intro}</p>
            </div>
          </div>

          <Collapse
            defaultActiveKey={['0']}
            className={styles.faqCollapse}
            expandIconPosition="end"
            items={active.items.map((item, i) => ({
              key: String(i),
              label: (
                <span className={styles.faqQuestion}>
                  <QuestionCircleOutlined style={{ color: active.color }} /> {item.q}
                </span>
              ),
              children: <p className={styles.faqAnswer}>{item.a}</p>,
            }))}
          />
        </div>

        {/* ── Support section ── */}
        <div className={styles.supportSection}>
          <div className={styles.supportGlow} />
          <div className={styles.supportContent}>
            <h2 className={styles.supportTitle}>Bạn Cần Hỗ Trợ Thêm?</h2>
            <p className={styles.supportSub}>Đội ngũ H&H Fish Shop luôn sẵn sàng giải đáp mọi thắc mắc của bạn</p>
            <Row gutter={[20, 20]} justify="center" className={styles.contactRow}>
              <Col xs={24} sm={8}>
                <div className={styles.contactCard}>
                  <div className={styles.contactIcon} style={{ background: 'rgba(24,144,255,0.15)', color: '#40a9ff' }}>
                    <PhoneOutlined />
                  </div>
                  <div className={styles.contactLabel}>Hotline</div>
                  <div className={styles.contactValue}>0853997698</div>
                  <div className={styles.contactNote}>8:00 – 22:00 hàng ngày</div>
                </div>
              </Col>
              <Col xs={24} sm={8}>
                <div className={styles.contactCard}>
                  <div className={styles.contactIcon} style={{ background: 'rgba(0,168,75,0.15)', color: '#52e891' }}>
                    <MessageOutlined />
                  </div>
                  <div className={styles.contactLabel}>Zalo</div>
                  <div className={styles.contactValue}>0901 234 567</div>
                  <div className={styles.contactNote}>Phản hồi trong 30 phút</div>
                </div>
              </Col>
              <Col xs={24} sm={8}>
                <div className={styles.contactCard}>
                  <div className={styles.contactIcon} style={{ background: 'rgba(250,140,22,0.15)', color: '#ffa940' }}>
                    <MailOutlined />
                  </div>
                  <div className={styles.contactLabel}>Email</div>
                  <div className={styles.contactValue}>support@hhfishshop.vn</div>
                  <div className={styles.contactNote}>Trả lời trong 24 giờ</div>
                </div>
              </Col>
            </Row>
          </div>
        </div>

      </div>
    </CustomerLayout>
  );
};

export default Policies;
