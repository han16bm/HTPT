import type { BannerConfig } from '@/components/PromoBanner';

// ── Products page banners (cycle through these between product chunks) ─────────
export const PRODUCT_PAGE_BANNERS: BannerConfig[] = [
  {
    id: 'koi-premium',
    theme: 'koi',
    emoji: '🐟',
    tag: 'Nổi Bật',
    title: 'Cá Koi Nhật Bản Chính Hãng',
    subtitle:
      'Được tuyển chọn từ các trang trại uy tín tại Nhật Bản. Mang vẻ đẹp hoàng gia vào không gian sống của bạn.',
    ctaText: 'Khám phá ngay',
    ctaLink: '/categories/ca-koi',
  },
  {
    id: 'promo-flash',
    theme: 'promo',
    emoji: '🔥',
    tag: 'Ưu Đãi',
    title: 'Flash Sale – Giảm Đến 30%',
    subtitle:
      'Hàng trăm sản phẩm cá cảnh và phụ kiện được giảm giá trong thời gian có hạn. Đừng bỏ lỡ cơ hội này!',
    ctaText: 'Mua ngay',
    ctaLink: '/promotions',
  },
  {
    id: 'dragon-fish',
    theme: 'dragon',
    emoji: '🐉',
    tag: 'Cao Cấp',
    title: 'Cá Rồng – Biểu Tượng Phú Quý',
    subtitle:
      'Cá Rồng Kim, Bạch Kim và Châu Phi – loài cá quý mang ý nghĩa tài lộc và thịnh vượng cho mọi gia chủ.',
    ctaText: 'Xem bộ sưu tập',
    ctaLink: '/categories/ca-rong',
  },
  {
    id: 'store-intro',
    theme: 'store',
    emoji: '🏠',
    tag: 'Về Chúng Tôi',
    title: 'H&H Fish Shop',
    subtitle:
      'Hơn 10 năm kinh nghiệm trong ngành cá cảnh. Cam kết sản phẩm chất lượng cao, dịch vụ tận tâm chu đáo.',
    ctaText: 'Tìm hiểu thêm',
    ctaLink: '/about',
  },
];

// ── Category-specific banners (shown inside each category listing) ─────────────
export const CATEGORY_PAGE_BANNERS: Record<string, BannerConfig> = {
  'ca-koi': {
    id: 'cat-koi-care',
    theme: 'koi',
    emoji: '💧',
    tag: 'Mẹo Chăm Sóc',
    title: 'Bí Quyết Nuôi Cá Koi Khỏe Mạnh',
    subtitle:
      'Nước sạch, thức ăn dinh dưỡng và không gian thoáng là chìa khóa để cá Koi tỏa sắc quanh năm.',
    ctaText: 'Xem hướng dẫn',
    ctaLink: '/blog',
  },
  'ca-vang': {
    id: 'cat-vang-beginner',
    theme: 'store',
    emoji: '✨',
    tag: 'Dễ Nuôi',
    title: 'Cá Vàng – Nét Đẹp Mọi Không Gian',
    subtitle:
      'Cá vàng là lựa chọn lý tưởng cho người mới bắt đầu. Màu sắc rực rỡ, thân thiện và dễ chăm sóc.',
    ctaText: 'Khám phá thêm',
    ctaLink: '/categories/ca-bay-mau',
  },
  'ca-rong': {
    id: 'cat-rong-fengshui',
    theme: 'dragon',
    emoji: '⚡',
    tag: 'Phong Thủy',
    title: 'Cá Rồng Mang Tài Lộc Vào Nhà',
    subtitle:
      'Đặt bể cá Rồng đúng vị trí phong thủy để thu hút vận may và sự thịnh vượng cho gia đình bạn.',
    ctaText: 'Tìm hiểu ngay',
    ctaLink: '/blog',
  },
  'ca-discus': {
    id: 'cat-discus-beauty',
    theme: 'koi',
    emoji: '🫧',
    tag: 'Nhiệt Đới',
    title: 'Cá Dĩa – Hoàng Đế Hồ Nhiệt Đới',
    subtitle:
      'Màu sắc phong phú, dáng bơi uyển chuyển – cá dĩa là điểm nhấn hoàn hảo cho mọi bể thủy sinh.',
    ctaText: 'Xem bộ sưu tập',
    ctaLink: '/categories/ca-bay-mau',
  },
  'ca-be': {
    id: 'cat-betta-art',
    theme: 'promo',
    emoji: '🐡',
    tag: 'Đặc Sắc',
    title: 'Cá Betta – Chiến Binh Đầy Màu Sắc',
    subtitle:
      'Vây óng ánh, màu sắc rực rỡ như tranh vẽ – mỗi chú cá betta là một tác phẩm nghệ thuật độc đáo.',
    ctaText: 'Chọn ngay',
    ctaLink: '/categories/ca-be',
  },
  'phu-kien': {
    id: 'cat-phukhien-setup',
    theme: 'store',
    emoji: '🔧',
    tag: 'Trang Bị',
    title: 'Thiết Lập Bể Cá Hoàn Hảo',
    subtitle:
      'Từ máy lọc, đèn LED đến sỏi nền và trang trí – chúng tôi có đủ phụ kiện để hồ cá bạn trở nên lý tưởng.',
    ctaText: 'Xem phụ kiện',
    ctaLink: '/categories/thuc-an',
  },
  'thuc-an': {
    id: 'cat-thucan-nutrition',
    theme: 'store',
    emoji: '🌿',
    tag: 'Dinh Dưỡng',
    title: 'Thức Ăn Chuẩn Dinh Dưỡng',
    subtitle:
      'Chế độ ăn cân bằng giúp cá phát triển khỏe mạnh, màu sắc tươi sáng và sống lâu hơn.',
    ctaText: 'Chọn thức ăn',
    ctaLink: '/categories/thuc-an',
  },
  'ca-bay-mau': {
    id: 'cat-guppy-breed',
    theme: 'promo',
    emoji: '🌈',
    tag: 'Đa Dạng',
    title: 'Cá Bảy Màu – Ngàn Dáng Vẻ Rực Rỡ',
    subtitle:
      'Hơn 50 dòng cá bảy màu cao cấp. Dễ nuôi, dễ sinh sản, phù hợp mọi loại bể thủy sinh.',
    ctaText: 'Chọn ngay',
    ctaLink: '/categories',
  },
};
