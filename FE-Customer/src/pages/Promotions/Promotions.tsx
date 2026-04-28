import React, { useEffect, useState } from 'react';
import { Row, Col, Button, Progress, Statistic, Card, Empty, Spin, Tag, message } from 'antd';
import { FireFilled, ClockCircleOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { CustomerLayout } from '@/components/Layout';
import { promotionService, type Promotion } from '@/api';
import { formatVND } from '@/utils/format';
import styles from './Promotions.module.scss';

const { Countdown } = Statistic;
const COLOR_PALETTE = ['#ff4d4f', '#52c41a', '#1890ff', '#722ed1', '#fa8c16', '#13c2c2'];
const EMOJI_PALETTE = ['⚡', '🎁', '🛒', '🐠', '💎', '🌊'];

type PromoPhase = 'live' | 'upcoming' | 'expired' | 'exhausted' | 'inactive';

const toDate = (value?: string | null): Date | null => {
  if (!value) return null;
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? null : parsed;
};

const getPromoPhase = (promo: Promotion, now: Date): PromoPhase => {
  if (promo.status !== 1) return 'inactive';

  const startAt = toDate(promo.startAt);
  const endAt = toDate(promo.endAt);

  if (promo.usageLimit && promo.usedCount >= promo.usageLimit) return 'exhausted';
  if (endAt && now > endAt) return 'expired';
  if (startAt && now < startAt) return 'upcoming';
  return 'live';
};

const formatPromoDiscount = (promo: Promotion): string => {
  if (promo.discountType === 'PERCENT') {
    return `${promo.discountValue}%`;
  }
  return formatVND(promo.discountValue);
};

const formatPromoBenefit = (promo: Promotion): string =>
  promo.discountType === 'PERCENT'
    ? `Giảm ${promo.discountValue}%`
    : `Giảm ${formatVND(promo.discountValue)}`;

const formatPromoDate = (value?: string | null): string => {
  const date = toDate(value);
  return date ? date.toLocaleDateString('vi-VN') : 'Không giới hạn';
};

const getUsageProgress = (promo: Promotion): number | undefined => {
  if (!promo.usageLimit || promo.usageLimit <= 0) return undefined;
  return Math.min(100, Math.round((promo.usedCount / promo.usageLimit) * 100));
};

const getStatusConfig = (phase: PromoPhase): { label: string; color: string } => {
  switch (phase) {
    case 'live':
      return { label: 'Đang hoạt động', color: 'green' };
    case 'upcoming':
      return { label: 'Sắp diễn ra', color: 'gold' };
    case 'exhausted':
      return { label: 'Hết lượt', color: 'volcano' };
    case 'expired':
      return { label: 'Hết hạn', color: 'default' };
    default:
      return { label: 'Vô hiệu', color: 'default' };
  }
};

const Promotions: React.FC = () => {
  const navigate = useNavigate();
  const [copied, setCopied] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [promotions, setPromotions] = useState<Promotion[]>([]);

  useEffect(() => {
    const fetchPromotions = async () => {
      setLoading(true);
      const response = await promotionService.search({ page: 1, pageSize: 100, status: 1 });

      if (response.success && response.data) {
        const now = new Date();
        const items = [...response.data.items]
          .filter((promo) => {
            const phase = getPromoPhase(promo, now);
            return phase !== 'inactive' && phase !== 'expired';
          })
          .sort((left, right) => {
            const leftPhase = getPromoPhase(left, now);
            const rightPhase = getPromoPhase(right, now);
            const phaseRank: Record<PromoPhase, number> = {
              live: 0,
              upcoming: 1,
              exhausted: 2,
              expired: 3,
              inactive: 4,
            };

            if (phaseRank[leftPhase] !== phaseRank[rightPhase]) {
              return phaseRank[leftPhase] - phaseRank[rightPhase];
            }

            const leftDate =
              toDate(leftPhase === 'upcoming' ? left.startAt : left.endAt)?.getTime() ??
              Number.MAX_SAFE_INTEGER;
            const rightDate =
              toDate(rightPhase === 'upcoming' ? right.startAt : right.endAt)?.getTime() ??
              Number.MAX_SAFE_INTEGER;

            if (leftDate !== rightDate) {
              return leftDate - rightDate;
            }

            return right.discountValue - left.discountValue;
          });

        setPromotions(items);
      } else {
        message.error(response.error || 'Không thể tải danh sách khuyến mãi');
        setPromotions([]);
      }

      setLoading(false);
    };

    void fetchPromotions();
  }, []);

  const now = new Date();
  const livePromotions = promotions.filter((promo) => getPromoPhase(promo, now) === 'live');
  const upcomingPromotions = promotions.filter((promo) => getPromoPhase(promo, now) === 'upcoming');
  const featuredPromotion = livePromotions[0] ?? upcomingPromotions[0] ?? promotions[0] ?? null;
  const featuredPhase = featuredPromotion ? getPromoPhase(featuredPromotion, now) : null;
  const countdownTarget = featuredPromotion
    ? toDate(featuredPhase === 'upcoming' ? featuredPromotion.startAt : featuredPromotion.endAt)?.getTime() ?? undefined
    : undefined;
  const topDiscountPromotion = promotions.reduce<Promotion | null>((best, current) => {
    if (!best) return current;
    if (current.discountType === 'PERCENT' && best.discountType !== 'PERCENT') return current;
    if (current.discountType !== 'PERCENT' && best.discountType === 'PERCENT') return best;
    return current.discountValue > best.discountValue ? current : best;
  }, null);

  const handleCopy = async (code: string) => {
    try {
      await navigator.clipboard.writeText(code);
      setCopied(code);
      window.setTimeout(() => setCopied(null), 2000);
    } catch {
      message.error('Không thể sao chép mã khuyến mãi');
    }
  };

  return (
    <CustomerLayout>
      <div className={styles.promoPage}>
        <div className={styles.pageHeader}>
          <FireFilled className={styles.headerIcon} />
          <h1 className={styles.pageTitle}>Khuyến Mãi & Ưu Đãi</h1>
          <p className={styles.pageSubtitle}>
            Các mã bên dưới được đồng bộ trực tiếp từ hệ thống quản trị, nên khi admin thêm, sửa hoặc xóa sẽ cập nhật tại đây.
          </p>
        </div>

        <div className={styles.heroBanner}>
          <span className={styles.bpA}>🔥</span>
          <span className={styles.bpB}>⚡</span>
          <span className={styles.bpC}>🎉</span>
          <span className={styles.bpD}>💥</span>
          <span className={styles.bpE}>✨</span>
          <span className={styles.bpF}>🎊</span>
          <span className={styles.bpG}>💰</span>

          <div className={styles.heroBannerInner}>
            <div className={styles.bannerLeft}>
              
              <h2 className={styles.bannerTitle}>
                <span className={styles.bannerTitleRainbow}>
                  {featuredPromotion ? featuredPromotion.title.toUpperCase() : 'ƯU ĐÃI MỚI NHẤT'}
                </span>
                <span className={styles.bannerBigNum}>
                  {topDiscountPromotion ? formatPromoDiscount(topDiscountPromotion) : '0%'}
                </span>
              </h2>
              <p className={styles.bannerSub}>
                {featuredPromotion?.description || 'Theo dõi các ưu đãi mới nhất để áp dụng ngay khi mua sắm.'}
              </p>
              <div className={styles.bannerBtns}>
                <Button
                  size="large"
                  className={styles.btnBuyNow}
                  onClick={() => navigate('/danhmuc')}
                >
                  🛒 Mua Ngay
                </Button>
                <Button
                  size="large"
                  className={styles.btnViewAll}
                  onClick={() => navigate('/danhmuc')}
                >
                  Xem Sản Phẩm
                </Button>
              </div>
            </div>

            <div className={styles.bannerRight}>
              <div className={styles.countdownBox}>
                <span className={styles.countdownLabel}>
                  <ClockCircleOutlined /> {featuredPhase === 'upcoming' ? 'Bắt đầu sau' : 'Kết thúc sau'}
                </span>
                {countdownTarget ? (
                  <>
                    <Countdown
                      value={countdownTarget}
                      format="DD : HH : mm : ss"
                      className={styles.countdown}
                    />
                    <div className={styles.countdownUnits}>
                      <span>Ngày</span>
                      <span>Giờ</span>
                      <span>Phút</span>
                      <span>Giây</span>
                    </div>
                  </>
                ) : (
                  <div className={styles.countdownFallback}>Chưa có thời gian áp dụng</div>
                )}
              </div>
            </div>
          </div>

          <div className={styles.bannerStats}>
            {[
              { icon: '🏷️', val: `${promotions.length}`, lbl: 'Mã đang hiển thị' },
              { icon: '⚡', val: `${livePromotions.length}`, lbl: 'Mã đang hoạt động' },
              { icon: '🎯', val: topDiscountPromotion ? formatPromoDiscount(topDiscountPromotion) : '0%', lbl: 'Ưu đãi nổi bật' },
              { icon: '🕒', val: `${upcomingPromotions.length}`, lbl: 'Sắp diễn ra' },
            ].map((item) => (
              <div className={styles.bStat} key={item.lbl}>
                <span className={styles.bStatIcon}>{item.icon}</span>
                <span className={styles.bStatVal}>{item.val}</span>
                <span className={styles.bStatLbl}>{item.lbl}</span>
              </div>
            ))}
          </div>
        </div>

        <h2 className={styles.sectionTitle}>Mã Giảm Giá Đang Có</h2>

        {loading ? (
          <div className={styles.loadingWrap}>
            <Spin size="large" />
          </div>
        ) : promotions.length === 0 ? (
          <div className={styles.emptyState}>
            <Empty
              description="Hiện chưa có mã khuyến mãi nào khả dụng."
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            />
          </div>
        ) : (
          <Row gutter={[24, 24]}>
            {promotions.map((promo, index) => {
              const phase = getPromoPhase(promo, now);
              const status = getStatusConfig(phase);
              const color = COLOR_PALETTE[index % COLOR_PALETTE.length];
              const emoji = EMOJI_PALETTE[index % EMOJI_PALETTE.length];
              const progress = getUsageProgress(promo);
              const usageText = promo.usageLimit
                ? `${promo.usedCount}/${promo.usageLimit} lượt`
                : `${promo.usedCount} lượt đã dùng`;

              return (
                <Col xs={24} key={promo.id}>
                  <Card
                    className={styles.promoCard}
                    bordered={false}
                    style={{ borderTop: `4px solid ${color}` }}
                  >
                    <div className={styles.statusRow}>
                      <Tag color={status.color}>{status.label}</Tag>
                      <span className={styles.usageText}>{usageText}</span>
                    </div>

                    <div className={styles.promoBody}>
                      <div className={styles.promoLead}>
                        <div className={styles.promoEmoji}>{emoji}</div>
                        <div className={styles.discount} style={{ color }}>
                          {formatPromoDiscount(promo)}
                        </div>
                      </div>

                      <div className={styles.promoMain}>
                        <h3 className={styles.promoTitle}>{promo.title}</h3>
                        <p className={styles.promoDesc}>
                          {promo.description || 'Ưu đãi đang được áp dụng trên hệ thống.'}
                        </p>

                        <div className={styles.promoMeta}>
                          <div className={styles.metaItem}>{formatPromoBenefit(promo)}</div>
                          {!!promo.maxDiscountValue && (
                            <div className={styles.metaItem}>Tối đa {formatVND(promo.maxDiscountValue)}</div>
                          )}
                          {!!promo.minOrderValue && promo.minOrderValue > 0 && (
                            <div className={styles.metaItem}>
                              Đơn tối thiểu: <strong>{formatVND(promo.minOrderValue)}</strong>
                            </div>
                          )}
                        </div>
                      </div>

                      <div className={styles.promoSide}>
                        {progress !== undefined && (
                          <div className={styles.progressWrapper}>
                            <Progress
                              percent={progress}
                              showInfo={false}
                              strokeColor={color}
                              size="small"
                            />
                            <span className={styles.progressLabel}>Đã dùng {progress}% giới hạn</span>
                          </div>
                        )}

                        <div className={styles.codeRow}>
                          <span className={styles.code}>{promo.promoCode}</span>
                          <Button
                            size="small"
                            type={copied === promo.promoCode ? 'primary' : 'default'}
                            onClick={() => handleCopy(promo.promoCode)}
                          >
                            {copied === promo.promoCode ? '✓ Đã sao chép' : 'Sao chép'}
                          </Button>
                        </div>

                        <div className={styles.deadline}>
                          <ClockCircleOutlined /> Hiệu lực: {formatPromoDate(promo.startAt)} - {formatPromoDate(promo.endAt)}
                        </div>
                      </div>
                    </div>
                  </Card>
                </Col>
              );
            })}
          </Row>
        )}

        {/* <div className={styles.loyaltySection}>
          <GiftFilled className={styles.loyaltyIcon} />
          <h2>Chương Trình Tích Điểm</h2>
          <p>Mỗi 10,000đ mua hàng = 1 điểm tích lũy. Đổi điểm lấy ưu đãi hấp dẫn!</p>
          <Row gutter={[24, 24]} justify="center" style={{ marginTop: 24 }}>
            {[
              { level: 'Bạc', points: '0 – 500 điểm', benefit: 'Giảm 5% mọi đơn', emoji: '🥈' },
              { level: 'Vàng', points: '500 – 2,000 điểm', benefit: 'Giảm 10% + quà tặng', emoji: '🥇' },
              { level: 'Kim Cương', points: '2,000+ điểm', benefit: 'Giảm 15% + ưu tiên tư vấn', emoji: '💎' },
            ].map((tier) => (
              <Col xs={24} sm={8} key={tier.level}>
                <div className={styles.tierCard}>
                  <div className={styles.tierEmoji}>{tier.emoji}</div>
                  <h3>{tier.level}</h3>
                  <div className={styles.tierPoints}>{tier.points}</div>
                  <p>{tier.benefit}</p>
                </div>
              </Col>
            ))}
          </Row>
        </div> */}
      </div>
    </CustomerLayout>
  );
};

export default Promotions;
