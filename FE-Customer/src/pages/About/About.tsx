import React from 'react';
import { Row, Col, Button, Timeline } from 'antd';
import {
  TrophyOutlined,
  HeartOutlined,
  GlobalOutlined,
  RocketOutlined,
  EyeOutlined,
  CompassOutlined,
  CarOutlined,
  CustomerServiceOutlined,
  ShopOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { CustomerLayout } from '@/components/Layout';
import styles from './About.module.scss';

const About: React.FC = () => {
  const navigate = useNavigate();

  const mission = [
    {
      icon: <RocketOutlined />,
      key: 'mission',
      label: 'Sứ Mệnh',
      color: '#1890ff',
      text:
        'Mang thế giới cá cảnh phong phú và đa dạng đến gần hơn với mọi người, giúp mỗi gia đình cảm nhận được vẻ đẹp và sự thư giãn từ thiên nhiên dưới nước.',
    },
    {
      icon: <EyeOutlined />,
      key: 'vision',
      label: 'Tầm Nhìn',
      color: '#52c41a',
      text:
        'Trở thành thương hiệu cá cảnh uy tín hàng đầu Việt Nam, kiến tạo cộng đồng người yêu cá cảnh sôi động và kết nối những trái tim đam mê thủy sinh.',
    },
    {
      icon: <CompassOutlined />,
      key: 'values',
      label: 'Giá Trị',
      color: '#fa8c16',
      text:
        'Chất lượng – Tận tâm – Uy tín. Mỗi sản phẩm đều được kiểm tra nghiêm ngặt, mỗi khách hàng đều được tư vấn chân thành và mỗi cam kết đều được thực hiện đúng hẹn.',
    },
  ];

  const highlights = [
    { icon: <TrophyOutlined />, title: 'Cá Cảnh Chất Lượng Cao', desc: 'Nguồn gốc rõ ràng, kiểm dịch đầy đủ, sức khỏe được đảm bảo trước khi giao tới tay bạn.', color: '#1890ff' },
    { icon: <GlobalOutlined />, title: 'Nhiều Loại Cá Đẹp', desc: 'Hơn 500 loài cá cảnh độc đáo từ khắp nơi: Nhật Bản, Thái Lan, Indonesia, Nam Mỹ.', color: '#13c2c2' },
    { icon: <CustomerServiceOutlined />, title: 'Tư Vấn Nhiệt Tình', desc: 'Đội ngũ chuyên gia sẵn sàng hỗ trợ bạn 24/7 từ chọn cá, setup bể đến cách chăm sóc.', color: '#52c41a' },
    { icon: <CarOutlined />, title: 'Giao Hàng Nhanh', desc: 'Đóng gói chuyên dụng bằng túi oxy, giao nội thành trong ngày, toàn quốc 1–3 ngày.', color: '#fa8c16' },
    { icon: <HeartOutlined />, title: 'Bảo Hành Uy Tín', desc: 'Cam kết đổi trả 7 ngày nếu cá không khỏe sau khi nhận hàng theo đúng hướng dẫn.', color: '#eb2f96' },
    { icon: <ShopOutlined />, title: 'Cửa Hàng Thực Tế', desc: 'Ghé thăm showroom tại Hà Đông - Hà Nội để tận mắt chọn lựa những chú cá ưa thích.', color: '#722ed1' },
  ];

  const gallery = [
    { emoji: '🐠', label: 'Cá Bướm' },
    { emoji: '🐡', label: 'Cá Nóc' },
    { emoji: '🦈', label: 'Cá Mập Mini' },
    { emoji: '🐟', label: 'Cá Vàng' },
    { emoji: '🦑', label: 'Bể Thủy Sinh' },
    { emoji: '🌊', label: 'Hồ San Hô' },
    { emoji: '🐙', label: 'Cá Arowana' },
    { emoji: '🦐', label: 'Tôm Cảnh' },
  ];
  void gallery;

  return (
    <CustomerLayout>
      <div className={styles.aboutPage}>

        {/* ── 1. Hero Banner ── */}
        <div className={styles.heroBanner}>
          <div className={styles.bannerBubbles}>
            {[...Array(8)].map((_, i) => (
              <span key={i} className={styles.bubble} style={{ '--i': i } as React.CSSProperties} />
            ))}
          </div>
          <div className={styles.bannerOverlay} />
          <div className={styles.bannerContent}>
            <div className={styles.bannerFish}>🐟</div>
            <h1 className={styles.bannerTitle}>Về Chúng Tôi</h1>
            <p className={styles.bannerSub}>Nơi mang thế giới cá cảnh đến gần hơn với mọi người.</p>
          </div>
        </div>

        {/* ── 2. Stats ── */}
        <div className={styles.statsRow}>
          {[
            { value: '10+', label: 'Năm kinh nghiệm' },
            { value: '500+', label: 'Loài cá cảnh' },
            { value: '50K+', label: 'Khách hàng tin dùng' },
            { value: '100%', label: 'Cá khỏe mạnh' },
          ].map((s, i) => (
            <div key={i} className={styles.statItem}>
              <div className={styles.statValue}>{s.value}</div>
              <div className={styles.statLabel}>{s.label}</div>
            </div>
          ))}
        </div>

        {/* ── 3. Store Introduction ── */}
        <section className={styles.section}>
          <Row gutter={[48, 32]} align="middle">
            <Col xs={24} md={12}>
              <div className={styles.storeImageWrap}>
                <div className={styles.storeImageInner}>
                  <div className={styles.storeImagePlaceholder}>
                    <span>🏪</span>
                    <p>H&H Fish Shop Showroom</p>
                  </div>
                </div>
              </div>
            </Col>
            <Col xs={24} md={12}>
              <div className={styles.storeInfo}>
                <h2 className={styles.sectionTitle}>Câu Chuyện Của Chúng Tôi</h2>
                <p>
                  H&H Fish Shop được thành lập vào năm 2016 bởi hai người bạn thân — Hân và Hiếu — những người đã cùng nhau lớn lên bên những chiếc bể cá nhỏ đặt góc phòng từ thời trung học. Điều bắt đầu như một sở thích chung dần trở thành một ước mơ: mang thế giới thủy sinh sống động đến gần hơn với mọi người.

                </p>
                <p>
                 Khởi đầu từ một cửa hàng nhỏ tại Dương Nội, Hà Nội, chúng tôi đã xây dựng được lòng tin của hàng nghìn khách hàng — không chỉ nhờ chất lượng sản phẩm, mà còn nhờ sự tận tâm trong từng buổi tư vấn, từng lần khách hàng gọi điện hỏi "con cá của tôi đang bị sao vậy?".
                </p>
                <p>
                 Ngày nay, H&H Fish Shop tự hào là một trong những địa chỉ cá cảnh uy tín tại Hà Nội, với đa dạng hơn 500 loài cá và hàng trăm loại phụ kiện bể cá cao cấp từ các thương hiệu uy tín trên thế giới. Mỗi sản phẩm chúng tôi chọn lựa đều được kiểm định kỹ càng — vì chúng tôi hiểu rằng với nhiều người, chiếc bể cá không chỉ là vật trang trí, mà là một góc bình yên riêng.
                </p>
                <p>
                 Dù bạn là người mới bắt đầu hay đã là tay chơi lâu năm, H&H Fish Shop luôn ở đây để đồng hành cùng bạn — từ chiếc bể đầu tiên cho đến những hệ thống thủy sinh phức tạp nhất.
                </p>
                <div className={styles.timelineBox}>
                  <Timeline
                    items={[
                      { color: 'blue',   children: <><strong>2016</strong> - Hân và Hiếu lần đầu gặp nhau, cùng chung niềm đam mê với thế giới cá cảnh. Từ những buổi trao đổi, học hỏi và chia sẻ, ý tưởng về H&H Fish Shop dần hình thành.</> },
                      { color: 'green',  children: <><strong>2018</strong> - Cả hai bắt đầu nghiên cứu chuyên sâu về các loài cá cảnh, thủy sinh và kỹ thuật thiết kế bể - nền tảng kiến thức cho những gì sẽ đến.</> },
                      { color: 'orange', children: <><strong>2020</strong> - Thử nghiệm mô hình kinh doanh nhỏ, bán cá cảnh và phụ kiện trong cộng đồng bạn bè, người thân. Phản hồi tích cực là động lực để tiến xa hơn.</> },
                      { color: 'purple', children: <><strong>2022</strong> - Hoàn thiện ý tưởng, lên kế hoạch kinh doanh và tìm kiếm mặt bằng phù hợp tại Dương Nội, Hà Nội.</> },
                      { color: 'red',    children: <><strong>2024</strong> - Xây dựng và hoàn thiện cửa hàng, chuẩn bị hệ thống sản phẩm với hơn 500 loài cá và hàng trăm phụ kiện cao cấp.</> },
                      { color: 'blue',   children: <><strong>2026</strong> - H&H Fish Shop chính thức khai trương tại Dương Nội, Hà Nội. Ước mơ 10 năm của Hân và Hiếu chính thức thành hiện thực.</> },
                    ]}
                  />
                </div>
              </div>
            </Col>
          </Row>
        </section>

        {/* ── 4. Mission / Vision / Values ── */}
        <section className={styles.missionSection}>
          <h2 className={styles.sectionTitle} style={{ textAlign: 'center', marginBottom: 32 }}>
            Sứ Mệnh & Tầm Nhìn
          </h2>
          <Row gutter={[24, 24]}>
            {mission.map((m) => (
              <Col xs={24} md={8} key={m.key}>
                <div className={styles.missionCard} style={{ '--accent': m.color } as React.CSSProperties}>
                  <div className={styles.missionIcon} style={{ color: m.color, background: m.color + '22' }}>
                    {m.icon}
                  </div>
                  <h3 className={styles.missionLabel}>{m.label}</h3>
                  <p className={styles.missionText}>{m.text}</p>
                </div>
              </Col>
            ))}
          </Row>
        </section>

        {/* ── 5. Highlights ── */}
        <section className={styles.section}>
          <h2 className={styles.sectionTitle} style={{ textAlign: 'center', marginBottom: 32 }}>
            Những Điểm Nổi Bật
          </h2>
          <Row gutter={[20, 20]}>
            {highlights.map((h, i) => (
              <Col xs={24} sm={12} md={8} key={i}>
                <div className={styles.highlightCard}>
                  <div className={styles.highlightIcon} style={{ color: h.color, background: h.color + '1a' }}>
                    {h.icon}
                  </div>
                  <div className={styles.highlightBody}>
                    <h3 className={styles.highlightTitle}>{h.title}</h3>
                    <p className={styles.highlightDesc}>{h.desc}</p>
                  </div>
                </div>
              </Col>
            ))}
          </Row>
        </section>

        {/* ── 6. Gallery ── */}
        {/* <section className={styles.section}>
          <h2 className={styles.sectionTitle} style={{ textAlign: 'center', marginBottom: 32 }}>
            Hình Ảnh Hoạt Động
          </h2>
          <div className={styles.gallery}>
            {gallery.map((g, i) => (
              <div key={i} className={styles.galleryItem}>
                <div className={styles.galleryEmoji}>{g.emoji}</div>
                <div className={styles.galleryLabel}>{g.label}</div>
              </div>
            ))}
          </div>
        </section> */}

        {/* ── 7. CTA ── */}
        <section className={styles.ctaSection}>
          <div className={styles.ctaGlow} />
          <div className={styles.ctaContent}>
            <h2 className={styles.ctaTitle}>Khám Phá Thế Giới Cá Cảnh Của Chúng Tôi</h2>
            <p className={styles.ctaSub}>
              Hơn 500 loài cá cảnh đang chờ bạn. Bắt đầu hành trình khám phá ngay hôm nay!
            </p>
            <div className={styles.ctaButtons}>
              <Button
                type="primary"
                size="large"
                className={styles.ctaBtnPrimary}
                onClick={() => navigate('/products')}
              >
                Xem Sản Phẩm
              </Button>
              <Button
                size="large"
                className={styles.ctaBtnOutline}
                onClick={() => navigate('/contact')}
              >
                Liên Hệ Ngay
              </Button>
            </div>
          </div>
        </section>

      </div>
    </CustomerLayout>
  );
};

export default About;
