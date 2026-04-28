import React, { useEffect, useState } from 'react';
import { Row, Col, Typography, message, Divider } from 'antd';
import {
  CheckCircleOutlined,
  ThunderboltOutlined,
  SafetyOutlined,
  HeartOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { CustomerLayout } from '@/components/Layout';
import { ProductCard } from '@/components/ProductCard';
import { productService, cartService, authService, categoryService, type Product, type Category } from '@/api';
import { useScrollAnimation } from '@/hooks/useScrollAnimation';
import { dedupeProducts, getCategoryBranchIds, sortProducts } from '@/utils/productCategory';
import styles from './Home.module.scss';

const { Title } = Typography;

// ─── Slide data ───────────────────────────────────────────────────────────────
const heroSlides = [
  {
    id: 1,
    tag: 'NGHỆ THUẬT SỐNG',
    title: 'Cá Betta',
    subtitle: 'Halfmoon · Crowntail · Galaxy · Dumbo',
    description: 'Màu sắc rực rỡ, vây đuôi tuyệt mỹ như tác phẩm nghệ thuật sống. Hàng trăm dòng cá đẹp nhất.',
    features: ['Màu sắc rực rỡ đặc biệt', 'Hàng trăm dòng quý hiếm', 'Nhập khẩu trực tiếp'],
    cta: 'Xem bộ sưu tập',
    href: '/danhmuc?category=ca-betta',
    image: '/assets/images/slides/ca-betta.png',
    bgGradient: 'linear-gradient(135deg, #0d0520 0%, #1a083a 60%, #200a45 100%)',
    accent: '#a855f7',
    glowColor: 'rgba(168,85,247,0.28)',
    tagColor: '#a855f7',
    tagBg: 'rgba(168,85,247,0.12)',
    tagBorder: 'rgba(168,85,247,0.30)',
  },
  {
    id: 2,
    tag: 'CÁ CẢNH PHỔ BIẾN',
    title: 'Cá Guppy',
    subtitle: 'Moscow Blue · Dumbo Ear · Full Red · Tuxedo',
    description: 'Loài cá dễ nuôi, đẻ sai, màu sắc phong phú. Lựa chọn hoàn hảo cho người mới bắt đầu.',
    features: ['Dễ nuôi, đẻ sai con', 'Đủ màu sắc phong phú', 'Giá thành hợp lý'],
    cta: 'Khám phá ngay',
    href: '/danhmuc?category=ca-guppy',
    image: '/assets/images/slides/ca-guppy.png',
    bgGradient: 'linear-gradient(135deg, #021a10 0%, #04301c 60%, #063d22 100%)',
    accent: '#10b981',
    glowColor: 'rgba(16,185,129,0.26)',
    tagColor: '#10b981',
    tagBg: 'rgba(16,185,129,0.12)',
    tagBorder: 'rgba(16,185,129,0.28)',
  },
  {
    id: 3,
    tag: 'LUNG LINH SẮC MÀU',
    title: 'Cá Neon',
    subtitle: 'Neon Tetra · Cardinal · Green Neon',
    description: 'Ánh sáng xanh đỏ lấp lánh giữa bể tối — loài cá tạo hiệu ứng thị giác ấn tượng nhất.',
    features: ['Màu neon rực rỡ dưới đèn', 'Bơi đàn cực đẹp', 'Phù hợp bể thủy sinh'],
    cta: 'Tìm hiểu thêm',
    href: '/danhmuc?category=ca-neon',
    image: '/assets/images/slides/ca-neon.png',
    bgGradient: 'linear-gradient(135deg, #020d1e 0%, #041a35 60%, #051f40 100%)',
    accent: '#38bdf8',
    glowColor: 'rgba(56,189,248,0.28)',
    tagColor: '#38bdf8',
    tagBg: 'rgba(56,189,248,0.12)',
    tagBorder: 'rgba(56,189,248,0.30)',
  },
  {
    id: 4,
    tag: 'THANH LỊCH & QUÝ PHÁI',
    title: 'Cá Thần Tiên',
    subtitle: 'Angelfish · Koi Angel · Marble · Altum',
    description: 'Dáng bơi thướt tha, vây dài uyển chuyển. Loài cá mang vẻ đẹp hoàng gia cho bể của bạn.',
    features: ['Dáng bơi thanh lịch', 'Nhiều màu sắc quý hiếm', 'Phù hợp bể lớn'],
    cta: 'Xem ngay',
    href: '/danhmuc?category=ca-than-tien',
    image: '/assets/images/slides/ca-than-tien.png',
    bgGradient: 'linear-gradient(135deg, #0f1a05 0%, #1a2e08 60%, #213808 100%)',
    accent: '#84cc16',
    glowColor: 'rgba(132,204,22,0.24)',
    tagColor: '#84cc16',
    tagBg: 'rgba(132,204,22,0.12)',
    tagBorder: 'rgba(132,204,22,0.28)',
  },
  {
    id: 5,
    tag: 'MAY MẮN & PHONG THỦY',
    title: 'Cá Vàng',
    subtitle: 'Oranda · Ryukin · Telescope · Ranchu',
    description: 'Biểu tượng may mắn, tài lộc trong văn hóa phương Đông. Nuôi cá vàng để chiêu phúc lành.',
    features: ['Biểu tượng may mắn', 'Dễ nuôi, sống lâu', 'Nhiều dòng thuần chủng'],
    cta: 'Chọn cá ngay',
    href: '/danhmuc?category=ca-vang',
    image: '/assets/images/slides/ca-vang.png',
    bgGradient: 'linear-gradient(135deg, #1a0e00 0%, #2e1a00 60%, #3d2200 100%)',
    accent: '#f59e0b',
    glowColor: 'rgba(245,158,11,0.28)',
    tagColor: '#f59e0b',
    tagBg: 'rgba(245,158,11,0.12)',
    tagBorder: 'rgba(245,158,11,0.30)',
  },
];

const SLIDE_DURATION = 3000;

// ─── HeroSlider ───────────────────────────────────────────────────────────────
const HeroSlider: React.FC = () => {
  const navigate = useNavigate();
  const [current, setCurrent] = React.useState(0);
  const touchStartX = React.useRef<number>(0);

  const goNext = () => setCurrent(prev => (prev + 1) % heroSlides.length);
  const goPrev = () => setCurrent(prev => (prev - 1 + heroSlides.length) % heroSlides.length);

  useEffect(() => {
    const t = setTimeout(goNext, SLIDE_DURATION);
    return () => clearTimeout(t);
  }, [current]);

  const slide = heroSlides[current];

  return (
    <div
      className={styles.heroSlider}
      onTouchStart={e => { touchStartX.current = e.touches[0].clientX; }}
      onTouchEnd={e => {
        const delta = touchStartX.current - e.changedTouches[0].clientX;
        if (Math.abs(delta) > 50) delta > 0 ? goNext() : goPrev();
      }}
    >
      {heroSlides.map((s, i) => (
        <div
          key={s.id}
          className={`${styles.slide} ${i === current ? styles.slideActive : ''}`}
          style={{ background: s.bgGradient }}
          aria-hidden={i !== current}
        >
          {/* Radial glow behind image */}
          <div
            className={styles.slideDecorGlow}
            style={{
              background: `radial-gradient(ellipse 65% 85% at 72% 50%, ${s.glowColor}, transparent 70%)`,
            }}
          />

          <div className={styles.slideInner}>
            {/* Left: text content */}
            <div className={styles.slideContent}>
              <span
                className={styles.slideTag}
                style={{ color: s.tagColor, background: s.tagBg, borderColor: s.tagBorder }}
              >
                {s.tag}
              </span>

              <h1
                className={styles.slideTitle}
                style={{ '--slide-accent': s.accent } as React.CSSProperties}
              >
                {s.title}
              </h1>
              <p className={styles.slideSubtitle}>{s.subtitle}</p>
              <p className={styles.slideDesc}>{s.description}</p>

              <ul className={styles.slideFeatures}>
                {s.features.map((f, fi) => (
                  <li key={fi}>
                    <span className={styles.featureDot} style={{ background: s.accent }} />
                    {f}
                  </li>
                ))}
              </ul>

              <button
                className={styles.slideCta}
                style={{ background: s.accent, boxShadow: `0 4px 24px ${s.glowColor}` }}
                onClick={() => navigate(s.href)}
              >
                {s.cta} →
              </button>
            </div>

            {/* Right: image */}
            <div className={styles.slideImageWrap}>
              <img
                src={s.image}
                alt={s.title}
                className={styles.slideImage}
                onError={e => { (e.target as HTMLImageElement).style.opacity = '0'; }}
              />
            </div>
          </div>
        </div>
      ))}

      {/* Prev / Next arrows */}
      <button className={`${styles.sliderArrow} ${styles.arrowLeft}`} onClick={goPrev} aria-label="Trước">
        ‹
      </button>
      <button className={`${styles.sliderArrow} ${styles.arrowRight}`} onClick={goNext} aria-label="Sau">
        ›
      </button>

      {/* Dots */}
      <div className={styles.sliderDots}>
        {heroSlides.map((_, i) => (
          <button
            key={i}
            className={`${styles.sliderDot} ${i === current ? styles.sliderDotActive : ''}`}
            style={i === current ? { background: slide.accent, boxShadow: `0 0 8px ${slide.accent}` } : {}}
            onClick={() => setCurrent(i)}
            aria-label={`Slide ${i + 1}`}
          />
        ))}
      </div>

    </div>
  );
};

// ─── Types ────────────────────────────────────────────────────────────────────
interface CatSection {
  category: Category;
  products: Product[];
}

// ─── Home ─────────────────────────────────────────────────────────────────────
const Home: React.FC = () => {
  const navigate = useNavigate();
  const [featuredProducts, setFeaturedProducts] = useState<Product[]>([]);
  const [loadingFeatured, setLoadingFeatured] = useState(true);
  const [catSections, setCatSections] = useState<CatSection[]>([]);
  const [loadingCatSections, setLoadingCatSections] = useState(true);

  const benefitsRef     = useScrollAnimation<HTMLDivElement>();
  const productsRef     = useScrollAnimation<HTMLElement>();
  const catSectionsRef  = useScrollAnimation<HTMLDivElement>();
  const featuresRef     = useScrollAnimation<HTMLElement>();

  useEffect(() => {
    const loadFeatured = async () => {
      try {
        const res = await productService.getFeatured(12);
        if (res.success && res.data) {
          setFeaturedProducts(res.data);
        }
      } catch {
        message.error('Không thể tải sản phẩm nổi bật');
      } finally {
        setLoadingFeatured(false);
      }
    };
    loadFeatured();
  }, []);

  useEffect(() => {
    const loadCatSections = async () => {
      try {
        const catRes = await categoryService.getAll();
        if (!catRes.success || !catRes.data) {
          console.error('[Home] categoryService.getAll thất bại:', catRes.error);
          return;
        }
        const categories = catRes.data;
        const roots = categories.filter(c => !c.parentId);
        console.log('[Home] Root categories:', roots.map(c => c.name));

        const sections = await Promise.all(
          roots.map(async cat => {
            const branchIds = getCategoryBranchIds(categories, cat.id);
            const productGroups = await Promise.all(
              branchIds.map(async categoryId => {
                const res = await productService.getAll({ categoryId, sort: 'newest', page: 1, pageSize: 5 });
                if (!res.success) console.error(`[Home] Lấy sản phẩm categoryId=${categoryId} thất bại:`, res.error);
                return res.data?.items ?? [];
              })
            );
            const products = sortProducts(
              dedupeProducts(productGroups.flat()),
              'newest'
            ).slice(0, 5);
            console.log(`[Home] ${cat.name}: ${products.length} sản phẩm`);
            return { category: cat, products };
          })
        );

        setCatSections(sections.filter(s => s.products.length > 0));
      } catch (err) {
        console.error('[Home] loadCatSections lỗi:', err);
      } finally {
        setLoadingCatSections(false);
      }
    };
    loadCatSections();
  }, []);

  const handleAddToCart = async (product: Product) => {
    if (!authService.isAuthenticated()) {
      navigate('/login?returnUrl=/');
      return;
    }
    try {
      const response = await cartService.addToCart(product.id, 1);
      if (response.success) {
        message.success('Đã thêm vào giỏ hàng');
        if (response.data) window.dispatchEvent(new CustomEvent('cart-updated', { detail: response.data.totalItems }));
      } else {
        message.error(response.error || 'Không thể thêm vào giỏ hàng');
      }
    } catch {
      message.error('Đã xảy ra lỗi');
    }
  };

  const features = [
    {
      icon: <CheckCircleOutlined />,
      title: 'Chất lượng đảm bảo',
      description: 'Cá khỏe mạnh, được kiểm tra kỹ lưỡng',
    },
    {
      icon: <ThunderboltOutlined />,
      title: 'Giao hàng nhanh',
      description: 'Hỗ trợ giao hàng trong cùng ngày tại Hà Nội',
    },
    {
      icon: <SafetyOutlined />,
      title: 'Bảo hành uy tín',
      description: 'Đổi trả trong 7 ngày nếu có vấn đề',
    },
    {
      icon: <HeartOutlined />,
      title: 'Dịch vụ tư vấn',
      description: 'Tư vấn miễn phí cách chăm sóc cá cảnh',
    },
  ];

  return (
    <CustomerLayout>
      <div className={styles.home}>
        {/* Hero Slider */}
        <HeroSlider />

        {/* Benefits Bar */}
        <div ref={benefitsRef} className={`scroll-animate ${styles.benefitsBar}`}>
          <Row gutter={[32, 16]} justify="center">
            <Col xs={24} sm={12} md={6}>
              <div className={styles.benefit}>
                <span className={styles.benefitIcon}>🏆</span>
                <span className={styles.benefitText}>Chính hãng 100%</span>
              </div>
            </Col>
            <Col xs={24} sm={12} md={6}>
              <div className={styles.benefit}>
                <span className={styles.benefitIcon}>🚚</span>
                <span className={styles.benefitText}>Miễn phí ship từ 500K</span>
              </div>
            </Col>
            <Col xs={24} sm={12} md={6}>
              <div className={styles.benefit}>
                <span className={styles.benefitIcon}>💰</span>
                <span className={styles.benefitText}>Giá tốt nhất</span>
              </div>
            </Col>
            <Col xs={24} sm={12} md={6}>
              <div className={styles.benefit}>
                <span className={styles.benefitIcon}>🛡️</span>
                <span className={styles.benefitText}>Đổi trả 7 ngày</span>
              </div>
            </Col>
          </Row>
        </div>

        {/* Featured Products — admin-selected */}
        <section ref={productsRef} className={`scroll-animate ${styles.productsSection}`}>
          <div className={styles.sectionHeader}>
            <Title level={2}>Sản Phẩm Nổi Bật</Title>
          </div>

          {loadingFeatured ? (
            <div className={styles.skeletonWrap}>
              {[1, 2, 3, 4, 5, 6].map(i => (
                <div key={i} className={styles.skeletonBlock} />
              ))}
            </div>
          ) : featuredProducts.length > 0 ? (
            <Row gutter={[16, 16]}>
              {featuredProducts.map(product => (
                <Col xs={12} sm={8} md={6} key={product.id}>
                  <ProductCard product={product} onAddToCart={handleAddToCart} />
                </Col>
              ))}
            </Row>
          ) : (
            <div style={{ textAlign: 'center', padding: '40px 0', color: '#888' }}>
              Chưa có sản phẩm nổi bật
            </div>
          )}
        </section>

        {/* Category Sections — danh mục cha + sản phẩm cuộn ngang */}
        {(loadingCatSections || catSections.length > 0) && (
          <>
            <Divider />
            <div ref={catSectionsRef} className={`scroll-animate ${styles.catSectionsArea}`}>
              {loadingCatSections && (
                <div className={styles.catSkeletonWrap}>
                  {[1, 2, 3].map(i => (
                    <div key={i} className={styles.catSkeletonSection}>
                      <div className={styles.catSkeletonHeader} />
                      <div className={styles.catSkeletonRow}>
                        {[1, 2, 3, 4, 5].map(j => (
                          <div key={j} className={styles.catSkeletonCard} />
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              )}
              {catSections.map(section => (
                <section key={section.category.id} className={styles.catSection}>
                  <div className={styles.catSectionHeader}>
                    <div className={styles.catSectionTitle}>
                      {section.category.imageUrl && (
                        <img
                          src={section.category.imageUrl}
                          alt={section.category.name}
                          className={styles.catSectionIcon}
                        />
                      )}
                      <span>{section.category.name}</span>
                    </div>
                   
                  </div>

                  <div className={styles.catScrollTrack}>
                    {section.products.map(product => (
                      <div key={product.id} className={styles.catScrollItem}>
                        <ProductCard product={product} onAddToCart={handleAddToCart} />
                      </div>
                    ))}
                    <div
                      className={styles.catViewAllCard}
                      role="button"
                      tabIndex={0}
                      onClick={() => navigate(`/danhmuc?category=${section.category.slug}`)}
                      onKeyDown={e => e.key === 'Enter' && navigate(`/danhmuc?category=${section.category.slug}`)}
                    >
                      <div className={styles.catViewAllInner}>
                        <span className={styles.catViewAllArrow}>→</span>
                        <span>Xem tất cả sản phẩm</span>
                      </div>
                    </div>
                  </div>
                </section>
              ))}
            </div>
          </>
        )}

        <Divider />

        {/* Features Section */}
        <section ref={featuresRef} className={`scroll-animate ${styles.featuresSection}`}>
          <div className={styles.sectionHeader}>
            <Title level={2}>Tại Sao Chọn Chúng Tôi?</Title>
          </div>
          <Row gutter={[24, 24]}>
            {features.map((feature, index) => (
              <Col xs={24} sm={12} md={6} key={index}>
                <div className={styles.featureCard}>
                  <div className={styles.featureIcon}>{feature.icon}</div>
                  <h3>{feature.title}</h3>
                  <p>{feature.description}</p>
                </div>
              </Col>
            ))}
          </Row>
        </section>

        <Divider />
      </div>
    </CustomerLayout>
  );
};

export default Home;
