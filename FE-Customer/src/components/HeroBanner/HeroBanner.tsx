import React from 'react';
import { Button, Carousel } from 'antd';
import { useNavigate } from 'react-router-dom';
import styles from './HeroBanner.module.scss';

interface BannerSlide {
  title: string;
  description: string;
  tag: string;
  bgColor: string;
  ctaText?: string;
  ctaLink?: string;
}

interface HeroBannerProps {
  slides?: BannerSlide[];
}

const defaultSlides: BannerSlide[] = [
  {
    tag: 'KHUYẾN MÃI HOT',
    title: 'Cá Koi Nhật Bản Chính Hãng',
    description: 'Giảm đến 30% cho toàn bộ dòng cá Koi nhập khẩu trực tiếp từ Nhật Bản',
    bgColor: 'linear-gradient(135deg, #1890ff 0%, #096dd9 100%)',
    ctaText: 'Mua ngay',
    ctaLink: '/categories/ca-koi',
  },
  {
    tag: 'SẢN PHẨM MỚI',
    title: 'Phụ Kiện Bể Cá Cao Cấp',
    description: 'Hệ thống lọc, đèn LED, máy sục khí chuyên dụng – tất cả trong một nơi',
    bgColor: 'linear-gradient(135deg, #52c41a 0%, #237804 100%)',
    ctaText: 'Khám phá',
    ctaLink: '/categories/phu-kien',
  },
  {
    tag: 'MIỄN PHÍ GIAO HÀNG',
    title: 'Giao Hàng Trong Ngày',
    description: 'Miễn phí giao hàng toàn quốc cho đơn hàng từ 500,000đ. Giao nhanh trong ngày tại Hà Nội',
    bgColor: 'linear-gradient(135deg, #fa8c16 0%, #d46b08 100%)',
    ctaText: 'Xem ưu đãi',
    ctaLink: '/promotions',
  },
];

const HeroBanner: React.FC<HeroBannerProps> = ({ slides = defaultSlides }) => {
  const navigate = useNavigate();

  return (
    <div className={styles.heroBanner}>
      <Carousel autoplay autoplaySpeed={4000} dots={{ className: styles.dots }}>
        {slides.map((slide, index) => (
          <div key={index}>
            <div className={styles.slide} style={{ background: slide.bgColor }}>
              <div className={styles.slideContent}>
                <span className={styles.tag}>{slide.tag}</span>
                <h1 className={styles.title}>{slide.title}</h1>
                <p className={styles.description}>{slide.description}</p>
                {slide.ctaText && (
                  <Button
                    type="primary"
                    size="large"
                    ghost
                    className={styles.ctaBtn}
                    onClick={() => navigate(slide.ctaLink || '/categories')}
                  >
                    {slide.ctaText}
                  </Button>
                )}
              </div>
              <div className={styles.decorEmoji}>
                {index === 0 ? '🐟' : index === 1 ? '🔧' : '🚚'}
              </div>
            </div>
          </div>
        ))}
      </Carousel>
    </div>
  );
};

export default HeroBanner;
