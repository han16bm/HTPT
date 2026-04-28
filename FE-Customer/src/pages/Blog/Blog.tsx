import React, { useState } from 'react';
import { Row, Col, Tag, Button, Input } from 'antd';
import {
  ClockCircleOutlined,
  UserOutlined,
  EyeOutlined,
  ArrowRightOutlined,
  SearchOutlined,
  FireOutlined,
  UnorderedListOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { CustomerLayout } from '@/components/Layout';
import styles from './Blog.module.scss';

interface BlogPost {
  id: number;
  title: string;
  excerpt: string;
  category: string;
  author: string;
  date: string;
  readTime: string;
  views: string;
  emoji: string;
  gradient: string;
  featured?: boolean;
}

const posts: BlogPost[] = [
  {
    id: 1,
    title: 'Hướng Dẫn Chăm Sóc Cá Koi Cho Người Mới Bắt Đầu',
    excerpt: 'Cá Koi là một trong những loài cá phong thủy được yêu thích nhất. Bài viết này sẽ giúp bạn hiểu cách chăm sóc và nuôi dưỡng cá Koi khỏe mạnh từ những bước cơ bản nhất.',
    category: 'Cá Koi',
    author: 'Nguyễn Văn Hùng',
    date: '05/03/2026',
    readTime: '8 phút đọc',
    views: '1.2k',
    emoji: '🐟',
    gradient: 'linear-gradient(135deg, #0f3460 0%, #1a6b8a 50%, #16c79a 100%)',
    featured: true,
  },
  {
    id: 2,
    title: 'Top 10 Loài Cá Cảnh Đẹp Nhất Dễ Nuôi Tại Nhà',
    excerpt: 'Bạn đang tìm kiếm loài cá cảnh phù hợp để nuôi trong nhà? Hãy cùng khám phá 10 loài cá đẹp, dễ chăm sóc và phù hợp cho người mới bắt đầu.',
    category: 'Kinh Nghiệm',
    author: 'Trần Thị Lan',
    date: '28/02/2026',
    readTime: '6 phút đọc',
    views: '3.4k',
    emoji: '🐠',
    gradient: 'linear-gradient(135deg, #1e5799 0%, #207cca 50%, #7db9e8 100%)',
    featured: true,
  },
  {
    id: 3,
    title: 'Cách Thiết Kế Bể Cá Đẹp Cho Không Gian Sống',
    excerpt: 'Một bể cá được thiết kế đẹp không chỉ là nơi cư trú của cá mà còn là điểm nhấn trang trí tuyệt vời cho ngôi nhà của bạn.',
    category: 'Thiết Kế',
    author: 'Lê Minh Tuấn',
    date: '20/02/2026',
    readTime: '10 phút đọc',
    views: '2.1k',
    emoji: '🏠',
    gradient: 'linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%)',
  },
  {
    id: 4,
    title: 'Dinh Dưỡng Cho Cá Cảnh: Những Điều Bạn Cần Biết',
    excerpt: 'Chế độ ăn uống đóng vai trò quan trọng trong việc giữ cho cá cảnh khỏe mạnh và màu sắc rực rỡ. Tìm hiểu về các loại thức ăn phù hợp.',
    category: 'Dinh Dưỡng',
    author: 'Phạm Thu Hương',
    date: '15/02/2026',
    readTime: '5 phút đọc',
    views: '890',
    emoji: '🍖',
    gradient: 'linear-gradient(135deg, #2d6a4f 0%, #40916c 50%, #52b788 100%)',
  },
  {
    id: 5,
    title: 'Xử Lý Bệnh Thường Gặp Ở Cá Cảnh',
    excerpt: 'Cá cảnh cũng có thể mắc bệnh như bất kỳ sinh vật nào khác. Hãy tìm hiểu cách nhận biết và xử lý các bệnh phổ biến.',
    category: 'Sức Khỏe',
    author: 'Nguyễn Văn Hùng',
    date: '10/02/2026',
    readTime: '12 phút đọc',
    views: '4.2k',
    emoji: '💊',
    gradient: 'linear-gradient(135deg, #7b2d8b 0%, #9b59b6 50%, #c39bd3 100%)',
  },
  {
    id: 6,
    title: 'Cá Rồng – Biểu Tượng Phong Thủy Trong Nhà',
    excerpt: 'Cá Rồng (Arowana) từ lâu đã được coi là biểu tượng của sự thịnh vượng và may mắn trong văn hóa Á Đông. Tìm hiểu thêm về loài cá này.',
    category: 'Phong Thủy',
    author: 'Trần Thị Lan',
    date: '01/02/2026',
    readTime: '7 phút đọc',
    views: '5.6k',
    emoji: '🐉',
    gradient: 'linear-gradient(135deg, #b7791f 0%, #d4a017 50%, #f6d365 100%)',
  },
];

const CATS = ['Tất cả', 'Cá Koi', 'Kinh Nghiệm', 'Thiết Kế', 'Dinh Dưỡng', 'Sức Khỏe', 'Phong Thủy'];

const catColors: Record<string, string> = {
  'Cá Koi': 'blue',
  'Kinh Nghiệm': 'green',
  'Thiết Kế': 'purple',
  'Dinh Dưỡng': 'orange',
  'Sức Khỏe': 'red',
  'Phong Thủy': 'gold',
};

const parseViews = (v: string) => (v.endsWith('k') ? parseFloat(v) * 1000 : parseFloat(v));

const Blog: React.FC = () => {
  const navigate = useNavigate();
  const [activeCategory, setActiveCategory] = useState('Tất cả');
  const [searchText, setSearchText] = useState('');

  const filtered =
    activeCategory === 'Tất cả' ? posts : posts.filter((p) => p.category === activeCategory);

  const featuredPost = posts.find((p) => p.featured) || posts[0];
  const popularPosts = [...posts].sort((a, b) => parseViews(b.views) - parseViews(a.views)).slice(0, 3);
  const recentPosts = posts.slice(0, 3);
  const catCounts = CATS.filter((c) => c !== 'Tất cả').reduce<Record<string, number>>((acc, cat) => {
    acc[cat] = posts.filter((p) => p.category === cat).length;
    return acc;
  }, {});

  return (
    <CustomerLayout>
      <div className={styles.blogPage}>

        {/* ── Hero Banner ── */}
        <div className={styles.heroBanner}>
          <div className={styles.bannerOverlay} />
          <div className={styles.bannerContent}>
            <div className={styles.bannerEmojis}>🐠 🐟 🐡</div>
            <h1 className={styles.bannerTitle}>Blog Cá Cảnh</h1>
            <p className={styles.bannerSubtitle}>
              Chia sẻ kinh nghiệm nuôi cá và chăm sóc hồ cá
            </p>
          </div>
        </div>

        <div className={styles.mainWrap}>
          <Row gutter={[32, 32]}>

            {/* ── Main column ── */}
            <Col xs={24} lg={17}>

              {/* Featured Article */}
              <div className={styles.featuredSection}>
                <div className={styles.sectionLabel}>
                  <FireOutlined /> Bài viết nổi bật
                </div>
                <div
                  className={styles.featuredCard}
                  onClick={() => navigate(`/blog/${featuredPost.id}`)}
                >
                  <div className={styles.featuredImg} style={{ background: featuredPost.gradient }}>
                    <span className={styles.featuredImgEmoji}>{featuredPost.emoji}</span>
                    <Tag color={catColors[featuredPost.category] || 'default'} className={styles.featuredTag}>
                      {featuredPost.category}
                    </Tag>
                  </div>
                  <div className={styles.featuredBody}>
                    <h2 className={styles.featuredTitle}>{featuredPost.title}</h2>
                    <p className={styles.featuredExcerpt}>{featuredPost.excerpt}</p>
                    <div className={styles.featuredMeta}>
                      <span><UserOutlined /> {featuredPost.author}</span>
                      <span><ClockCircleOutlined /> {featuredPost.date}</span>
                      <span>{featuredPost.readTime}</span>
                      <span><EyeOutlined /> {featuredPost.views}</span>
                    </div>
                    <Button
                      type="primary"
                      className={styles.btnReadMore}
                      icon={<ArrowRightOutlined />}
                      iconPosition="end"
                    >
                      Đọc tiếp
                    </Button>
                  </div>
                </div>
              </div>

              {/* Category Filter */}
              <div className={styles.filterBar}>
                {CATS.map((cat) => (
                  <button
                    key={cat}
                    className={`${styles.filterBtn} ${activeCategory === cat ? styles.filterActive : ''}`}
                    onClick={() => setActiveCategory(cat)}
                  >
                    {cat}
                  </button>
                ))}
              </div>

              {/* Post Grid */}
              {filtered.length === 0 ? (
                <div className={styles.emptyMsg}>Không có bài viết nào trong danh mục này.</div>
              ) : (
                <Row gutter={[20, 24]}>
                  {filtered.map((post) => (
                    <Col xs={24} sm={12} md={8} key={post.id}>
                      <div
                        className={styles.postCard}
                        onClick={() => navigate(`/blog/${post.id}`)}
                      >
                        <div className={styles.postImgWrap}>
                          <div className={styles.postImg} style={{ background: post.gradient }}>
                            <span className={styles.postImgEmoji}>{post.emoji}</span>
                          </div>
                        </div>
                        <div className={styles.postBody}>
                          <Tag color={catColors[post.category] || 'default'} className={styles.postTag}>
                            {post.category}
                          </Tag>
                          <h3 className={styles.postTitle}>{post.title}</h3>
                          <p className={styles.postExcerpt}>{post.excerpt}</p>
                          <div className={styles.postMeta}>
                            <span><UserOutlined /> {post.author}</span>
                            <span><ClockCircleOutlined /> {post.date}</span>
                          </div>
                          <div className={styles.postFooter}>
                            <span className={styles.readTime}>{post.readTime}</span>
                            <span className={styles.views}><EyeOutlined /> {post.views}</span>
                          </div>
                          <div className={styles.postReadMore}>
                            Đọc tiếp <ArrowRightOutlined />
                          </div>
                        </div>
                      </div>
                    </Col>
                  ))}
                </Row>
              )}
            </Col>

            {/* ── Sidebar ── */}
            <Col xs={24} lg={7}>
              <div className={styles.sidebar}>

                {/* Search */}
                <div className={styles.sideWidget}>
                  <h4 className={styles.widgetTitle}><SearchOutlined /> Tìm kiếm</h4>
                  <Input
                    placeholder="Tìm bài viết..."
                    value={searchText}
                    onChange={(e) => setSearchText(e.target.value)}
                    className={styles.searchInput}
                  />
                </div>

                {/* Categories */}
                <div className={styles.sideWidget}>
                  <h4 className={styles.widgetTitle}><UnorderedListOutlined /> Danh mục</h4>
                  <ul className={styles.catList}>
                    {CATS.filter((c) => c !== 'Tất cả').map((cat) => (
                      <li
                        key={cat}
                        className={`${styles.catItem} ${activeCategory === cat ? styles.catItemActive : ''}`}
                        onClick={() => setActiveCategory(cat)}
                      >
                        <span>{cat}</span>
                        <span className={styles.catCount}>{catCounts[cat] ?? 0}</span>
                      </li>
                    ))}
                  </ul>
                </div>

                {/* Popular posts */}
                <div className={styles.sideWidget}>
                  <h4 className={styles.widgetTitle}><FireOutlined /> Bài viết phổ biến</h4>
                  <div className={styles.sidePostList}>
                    {popularPosts.map((p, i) => (
                      <div
                        key={p.id}
                        className={styles.sidePost}
                        onClick={() => navigate(`/blog/${p.id}`)}
                      >
                        <div className={styles.sidePostNum}>{i + 1}</div>
                        <div className={styles.sidePostInfo}>
                          <div className={styles.sidePostTitle}>{p.title}</div>
                          <div className={styles.sidePostMeta}><EyeOutlined /> {p.views}</div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Recent posts */}
                <div className={styles.sideWidget}>
                  <h4 className={styles.widgetTitle}><ClockCircleOutlined /> Bài viết mới</h4>
                  <div className={styles.sidePostList}>
                    {recentPosts.map((p) => (
                      <div
                        key={p.id}
                        className={styles.sidePost}
                        onClick={() => navigate(`/blog/${p.id}`)}
                      >
                        <div className={styles.sidePostThumb} style={{ background: p.gradient }}>
                          <span>{p.emoji}</span>
                        </div>
                        <div className={styles.sidePostInfo}>
                          <div className={styles.sidePostTitle}>{p.title}</div>
                          <div className={styles.sidePostMeta}><ClockCircleOutlined /> {p.date}</div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

              </div>
            </Col>

          </Row>
        </div>
      </div>
    </CustomerLayout>
  );
};

export default Blog;
