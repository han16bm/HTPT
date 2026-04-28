import React from 'react';
import { Row, Col, Button } from 'antd';
import {
  CheckCircleOutlined,
  PhoneOutlined,
  MessageOutlined,
  PhoneFilled,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { CustomerLayout } from '@/components/Layout';
import styles from './Services.module.scss';

const mainServices = [
  {
    emoji: '🏠',
    title: 'Thiết Kế Hồ Cá Theo Yêu Cầu',
    description: 'Đội ngũ chuyên gia thiết kế hồ cá phù hợp với không gian và phong cách của bạn. Từ bể cá mini để bàn đến hồ cá sân vườn quy mô lớn.',
    features: ['Tư vấn thiết kế miễn phí', 'Thi công tại nhà', 'Bảo hành 1 năm công trình', 'Hỗ trợ kỹ thuật sau lắp đặt'],
    price: 'Liên hệ báo giá',
    color: '#1890ff',
    gradient: 'linear-gradient(135deg, #0f3460 0%, #1a6b8a 100%)',
  },
  {
    emoji: '🔧',
    title: 'Lắp Đặt & Bảo Trì Hệ Thống',
    description: 'Dịch vụ lắp đặt hệ thống lọc, đèn, máy sục khí chuyên nghiệp. Bảo trì định kỳ để bể cá luôn hoạt động tốt nhất.',
    features: ['Lắp đặt tận nhà', 'Bảo trì hàng tháng/quý', 'Thay thế thiết bị hỏng', 'Báo cáo tình trạng bể'],
    price: 'Từ 200.000đ/lần',
    color: '#52c41a',
    gradient: 'linear-gradient(135deg, #1a4731 0%, #2d8a5e 100%)',
  },
  {
    emoji: '👨‍🏫',
    title: 'Tư Vấn Chăm Sóc Cá',
    description: 'Chuyên gia với hơn 10 năm kinh nghiệm tư vấn trực tiếp về cách nuôi dưỡng, chăm sóc và điều trị bệnh cho cá cảnh.',
    features: ['Tư vấn online qua Zalo/Video', 'Khám bệnh cá tại nhà', 'Hướng dẫn chế độ ăn', 'Tư vấn phong thủy cá'],
    price: 'Miễn phí tư vấn cơ bản',
    color: '#fa8c16',
    gradient: 'linear-gradient(135deg, #6b3a1a 0%, #c47a35 100%)',
  },
  {
    emoji: '🎁',
    title: 'Dịch Vụ Quà Tặng Doanh Nghiệp',
    description: 'Bộ quà tặng cá cảnh cao cấp dành cho đối tác, khách hàng doanh nghiệp. Bể cá trang trí văn phòng, phòng tiếp khách.',
    features: ['Tùy chỉnh theo ngân sách', 'Giao hàng đúng hẹn', 'Hóa đơn VAT đầy đủ', 'Dịch vụ setup tại công ty'],
    price: 'Từ 1.500.000đ/bộ',
    color: '#722ed1',
    gradient: 'linear-gradient(135deg, #2d1b6b 0%, #6a3aad 100%)',
  },
  {
    emoji: '🌟',
    title: 'Chương Trình Thành Viên VIP',
    description: 'Đăng ký thành viên VIP để nhận ưu đãi độc quyền, tích điểm thưởng và được ưu tiên hỗ trợ 24/7.',
    features: ['Giảm 10-15% mỗi đơn hàng', 'Tích điểm đổi quà', 'Hỗ trợ 24/7 qua hotline VIP', 'Mời tham gia sự kiện độc quyền'],
    price: 'Miễn phí đăng ký',
    color: '#eb2f96',
    gradient: 'linear-gradient(135deg, #6b1a3a 0%, #c73a7a 100%)',
  },
  {
    emoji: '🚚',
    title: 'Giao Hàng Nhanh Trong Ngày',
    description: 'Dịch vụ giao hàng hỏa tốc trong vòng 2-4 giờ tại Hà Nội. Đảm bảo cá cảnh đến tay bạn trong tình trạng tốt nhất.',
    features: ['Giao trong 2-4 giờ tại Hà Nội', 'Đóng gói chuyên dụng cho cá', 'GPS tracking đơn hàng', 'Hoàn tiền nếu cá chết khi giao'],
    price: '50.000đ/đơn (nội thành)',
    color: '#13c2c2',
    gradient: 'linear-gradient(135deg, #0a3d3d 0%, #1a8a8a 100%)',
  },
];

const PROCESS = [
  { step: '01', title: 'Liên Hệ', desc: 'Gọi hotline hoặc nhắn Zalo để được tư vấn', emoji: '📞' },
  { step: '02', title: 'Khảo Sát', desc: 'Chuyên gia đến khảo sát hoặc tư vấn trực tuyến', emoji: '🔍' },
  { step: '03', title: 'Báo Giá', desc: 'Nhận báo giá chi tiết trong vòng 24 giờ', emoji: '📋' },
  { step: '04', title: 'Thi Công', desc: 'Thi công, lắp đặt đúng hẹn theo cam kết', emoji: '⚙️' },
  { step: '05', title: 'Bàn Giao', desc: 'Hướng dẫn sử dụng và bàn giao hoàn tất', emoji: '✅' },
];

const GALLERY = [
  { emoji: '🐠', label: 'Hồ Cá Thủy Sinh', gradient: 'linear-gradient(135deg, #020d1a 0%, #0a4a6a 100%)' },
  { emoji: '🐟', label: 'Cá Koi Nhật', gradient: 'linear-gradient(135deg, #1a0a2e 0%, #4a1a7a 100%)' },
  { emoji: '🏠', label: 'Setup Hồ Nano', gradient: 'linear-gradient(135deg, #0a2e1a 0%, #1a6a3a 100%)' },
  { emoji: '🐡', label: 'Bể Cá Biển', gradient: 'linear-gradient(135deg, #0a1a3a 0%, #1a4a8a 100%)' },
  { emoji: '🐉', label: 'Cá Rồng Arowana', gradient: 'linear-gradient(135deg, #2e1a0a 0%, #8a4a1a 100%)' },
  { emoji: '🌿', label: 'Hồ Thực Vật', gradient: 'linear-gradient(135deg, #0a2e10 0%, #1a7a30 100%)' },
];

const Services: React.FC = () => {
  const navigate = useNavigate();

  return (
    <CustomerLayout>
      <div className={styles.servicesPage}>

        {/* ── Hero Banner ── */}
        <div className={styles.heroBanner}>
          <div className={styles.bannerOverlay} />
          <div className={styles.bannerContent}>
            <div className={styles.bannerEmojis}>🐠 🐟 🐡</div>
            <h1 className={styles.bannerTitle}>Dịch Vụ Của Chúng Tôi</h1>
            <p className={styles.bannerSubtitle}>
              Cung cấp các dịch vụ chăm sóc, tư vấn và thiết kế hồ cá chuyên nghiệp
            </p>
            <div className={styles.bannerBadges}>
              <span className={styles.badge}>✔ Chuyên nghiệp</span>
              <span className={styles.badge}>✔ Uy tín 10+ năm</span>
              <span className={styles.badge}>✔ Bảo hành tận tâm</span>
            </div>
          </div>
        </div>

        {/* ── Services Grid ── */}
        <div className={styles.section}>
          <div className={styles.sectionHeader}>
            <h2 className={styles.sectionTitle}>Danh Sách Dịch Vụ</h2>
            <p className={styles.sectionSub}>Giải pháp toàn diện cho mọi nhu cầu về hồ cá cảnh</p>
          </div>
          <Row gutter={[24, 24]}>
            {mainServices.map((service, i) => (
              <Col xs={24} sm={12} lg={8} key={i}>
                <div className={styles.serviceCard}>
                  <div className={styles.cardTop} style={{ background: service.gradient }}>
                    <span className={styles.cardEmoji}>{service.emoji}</span>
                    <div className={styles.cardPriceBadge} style={{ color: service.color }}>
                      {service.price}
                    </div>
                  </div>
                  <div className={styles.cardBody}>
                    <h3 className={styles.serviceTitle} style={{ color: service.color }}>
                      {service.title}
                    </h3>
                    <p className={styles.serviceDesc}>{service.description}</p>
                    <ul className={styles.featureList}>
                      {service.features.map((f, j) => (
                        <li key={j}>
                          <CheckCircleOutlined style={{ color: service.color }} /> {f}
                        </li>
                      ))}
                    </ul>
                    <div className={styles.cardActions}>
                      <Button
                        type="primary"
                        block
                        onClick={() => navigate('/contact')}
                        style={{ background: service.gradient, border: 'none' }}
                        className={styles.btnService}
                      >
                        <PhoneFilled /> Đăng Ký Ngay
                      </Button>
                    </div>
                  </div>
                </div>
              </Col>
            ))}
          </Row>
        </div>

        {/* ── Process ── */}
        <div className={styles.processSection}>
          <div className={styles.sectionHeader}>
            <h2 className={styles.sectionTitle}>Quy Trình Làm Việc</h2>
            <p className={styles.sectionSub}>Minh bạch, nhanh chóng từ liên hệ đến bàn giao</p>
          </div>
          <div className={styles.processTrack}>
            {PROCESS.map((step, i) => (
              <React.Fragment key={i}>
                <div className={styles.processStep}>
                  <div className={styles.stepCircle}>
                    <span className={styles.stepEmoji}>{step.emoji}</span>
                  </div>
                  <div className={styles.stepNum}>{step.step}</div>
                  <h4 className={styles.stepTitle}>{step.title}</h4>
                  <p className={styles.stepDesc}>{step.desc}</p>
                </div>
                {i < PROCESS.length - 1 && <div className={styles.stepConnector}>→</div>}
              </React.Fragment>
            ))}
          </div>
        </div>

        {/* ── Gallery ── */}
        <div className={styles.section}>
          <div className={styles.sectionHeader}>
            <h2 className={styles.sectionTitle}>Hình Ảnh Thực Tế</h2>
            <p className={styles.sectionSub}>Những hồ cá đã được H&H Fish Shop hoàn thiện</p>
          </div>
          <div className={styles.galleryGrid}>
            {GALLERY.map((item, i) => (
              <div key={i} className={styles.galleryItem}>
                <div className={styles.galleryImg} style={{ background: item.gradient }}>
                  <span className={styles.galleryEmoji}>{item.emoji}</span>
                </div>
                <div className={styles.galleryLabel}>{item.label}</div>
              </div>
            ))}
          </div>
        </div>

        {/* ── CTA ── */}
        <div className={styles.ctaSection}>
          <div className={styles.ctaGlow} />
          <div className={styles.ctaContent}>
            <div className={styles.ctaEmoji}>🐠</div>
            <h2 className={styles.ctaTitle}>Bạn Cần Tư Vấn Về Hồ Cá?</h2>
            <p className={styles.ctaSub}>
              Đội ngũ chuyên gia H&H luôn sẵn sàng hỗ trợ bạn 24/7 — hoàn toàn miễn phí
            </p>
            <div className={styles.ctaButtons}>
              <Button
                type="primary"
                size="large"
                icon={<PhoneOutlined />}
                onClick={() => navigate('/contact')}
                className={styles.btnCta}
              >
                Liên Hệ Ngay
              </Button>
              <Button
                size="large"
                icon={<MessageOutlined />}
                className={styles.btnZalo}
                href="https://zalo.me/0901234567"
                target="_blank"
                rel="noopener noreferrer"
              >
                Nhắn Zalo
              </Button>
            </div>
          </div>
        </div>

      </div>
    </CustomerLayout>
  );
};

export default Services;
