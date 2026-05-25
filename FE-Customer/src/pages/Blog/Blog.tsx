import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { Row, Col, Tag, Button, Input, Spin, Empty, message } from 'antd';
import {
  ClockCircleOutlined,
  UserOutlined,
  EyeOutlined,
  ArrowRightOutlined,
  SearchOutlined,
  FireOutlined,
  UnorderedListOutlined,
} from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router-dom';
import { CustomerLayout } from '@/components/Layout';
import { blogService, type BlogCategory, type BlogPost } from '@/api';
import { IMAGE_PLACEHOLDER, resolveImageUrl, useImageFallback } from '@/utils/image';
import styles from './Blog.module.scss';

const { Search } = Input;

const gradients = [
  'linear-gradient(135deg, #0f3460 0%, #1a6b8a 50%, #16c79a 100%)',
  'linear-gradient(135deg, #1e5799 0%, #207cca 50%, #7db9e8 100%)',
  'linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%)',
  'linear-gradient(135deg, #2d6a4f 0%, #40916c 50%, #52b788 100%)',
  'linear-gradient(135deg, #7b2d8b 0%, #9b59b6 50%, #c39bd3 100%)',
  'linear-gradient(135deg, #b7791f 0%, #d4a017 50%, #f6d365 100%)',
];

const catColors = ['blue', 'green', 'purple', 'orange', 'red', 'gold', 'cyan'];
const fallbackEmojis = ['🐟', '🐠', '🏠', '🌿', '💧', '✨'];

const stripHtml = (value?: string): string =>
  (value ?? '').replace(/<[^>]*>/g, ' ').replace(/\s+/g, ' ').trim();

const formatDate = (value?: string): string => {
  if (!value) return 'Chưa đăng';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? 'Chưa đăng' : date.toLocaleDateString('vi-VN');
};

const getReadTime = (post: BlogPost): string => {
  const text = `${post.summary ?? ''} ${stripHtml(post.content)}`.trim();
  const words = text ? text.split(/\s+/).length : 120;
  return `${Math.max(1, Math.ceil(words / 220))} phút đọc`;
};

const getExcerpt = (post: BlogPost): string => {
  const text = post.summary || stripHtml(post.content);
  return text || 'Bài viết đang được cập nhật nội dung.';
};

const getPostDate = (post: BlogPost): string => formatDate(post.publishedAt || post.createdAt);

const getPostViews = (post: BlogPost): string => `${Math.max(1, post.id * 137).toLocaleString('vi-VN')}`;

const Blog: React.FC = () => {
  const navigate = useNavigate();
  const { slug } = useParams<{ slug?: string }>();
  const [posts, setPosts] = useState<BlogPost[]>([]);
  const [categories, setCategories] = useState<BlogCategory[]>([]);
  const [activeCategoryId, setActiveCategoryId] = useState<number | 'all'>('all');
  const [searchText, setSearchText] = useState('');
  const [keyword, setKeyword] = useState('');
  const [loading, setLoading] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [detailPost, setDetailPost] = useState<BlogPost | null>(null);

  useEffect(() => {
    const fetchCategories = async () => {
      const response = await blogService.getCategories();
      if (response.success && response.data) {
        setCategories(response.data);
      }
    };

    void fetchCategories();
  }, []);

  const fetchPosts = useCallback(async () => {
    setLoading(true);

    try {
      const response = await blogService.search({
        page: 1,
        pageSize: 12,
        status: 'PUBLISHED',
        keyword: keyword || undefined,
        categoryId: activeCategoryId === 'all' ? undefined : activeCategoryId,
      });

      if (response.success && response.data) {
        setPosts(response.data.items);
      } else {
        message.error(response.error || 'Không thể tải danh sách bài viết');
        setPosts([]);
      }
    } catch {
      message.error('Không thể tải danh sách bài viết');
      setPosts([]);
    } finally {
      setLoading(false);
    }
  }, [activeCategoryId, keyword]);

  useEffect(() => {
    if (!slug) {
      void fetchPosts();
    }
  }, [fetchPosts, slug]);

  useEffect(() => {
    if (!slug) {
      setDetailPost(null);
      return;
    }

    const fetchDetail = async () => {
      setDetailLoading(true);

      try {
        const response = await blogService.getBySlug(slug);
        if (response.success && response.data) {
          setDetailPost(response.data);
        } else {
          message.error(response.error || 'Không thể tải bài viết');
          setDetailPost(null);
        }
      } catch {
        message.error('Không thể tải bài viết');
        setDetailPost(null);
      } finally {
        setDetailLoading(false);
      }
    };

    void fetchDetail();
  }, [slug]);

  const categoryCountMap = useMemo(() => {
    const counts = new Map<number, number>();
    posts.forEach((post) => {
      if (post.categoryId) {
        counts.set(post.categoryId, (counts.get(post.categoryId) ?? 0) + 1);
      }
    });
    return counts;
  }, [posts]);

  const featuredPost = posts[0] ?? null;
  const popularPosts = posts.slice(0, 3);
  const recentPosts = posts.slice(0, 3);

  const handleSearch = (value: string) => {
    setSearchText(value);
    setKeyword(value.trim());
  };

  const handleCategoryChange = (categoryId: number | 'all') => {
    setActiveCategoryId(categoryId);
  };

  const openPost = (post: BlogPost) => {
    navigate(`/blog/${post.slug}`);
  };

  const renderVisual = (post: BlogPost, index: number, featured = false) => {
    const imageUrl = post.thumbnailUrl ? resolveImageUrl(post.thumbnailUrl, 'content') : undefined;
    const category = post.categoryName || 'Blog';
    const emoji = fallbackEmojis[index % fallbackEmojis.length];

    return (
      <div
        className={featured ? styles.featuredImg : styles.postImg}
        style={!imageUrl ? { background: gradients[index % gradients.length] } : undefined}
      >
        {imageUrl ? (
          <img
            src={imageUrl}
            alt={post.title}
            className={styles.postPhoto}
            onError={useImageFallback}
          />
        ) : (
          <span className={featured ? styles.featuredImgEmoji : styles.postImgEmoji}>{emoji}</span>
        )}
        {featured && (
          <Tag color={catColors[index % catColors.length]} className={styles.featuredTag}>
            {category}
          </Tag>
        )}
      </div>
    );
  };

  if (slug) {
    return (
      <CustomerLayout>
        <div className={styles.blogPage}>
          <div className={styles.mainWrap}>
            <Button className={styles.backButton} onClick={() => navigate('/blog')}>
              Quay lại Blog
            </Button>

            <Spin spinning={detailLoading}>
              {detailPost ? (
                <article className={styles.detailCard}>
                  {detailPost.thumbnailUrl && (
                    <img
                      src={resolveImageUrl(detailPost.thumbnailUrl, 'content')}
                      alt={detailPost.title}
                      className={styles.detailImage}
                      onError={(event) => {
                        event.currentTarget.src = IMAGE_PLACEHOLDER;
                      }}
                    />
                  )}
                  <div className={styles.detailMeta}>
                    <Tag color="blue">{detailPost.categoryName || 'Blog'}</Tag>
                    <span><ClockCircleOutlined /> {getPostDate(detailPost)}</span>
                    <span>{getReadTime(detailPost)}</span>
                  </div>
                  <h1 className={styles.detailTitle}>{detailPost.title}</h1>
                  {detailPost.summary && (
                    <p className={styles.detailSummary}>{detailPost.summary}</p>
                  )}
                  <div
                    className={styles.detailContent}
                    dangerouslySetInnerHTML={{ __html: detailPost.content || '' }}
                  />
                </article>
              ) : (
                <Empty description="Không tìm thấy bài viết" image={Empty.PRESENTED_IMAGE_SIMPLE} />
              )}
            </Spin>
          </div>
        </div>
      </CustomerLayout>
    );
  }

  return (
    <CustomerLayout>
      <div className={styles.blogPage}>
        <div className={styles.heroBanner}>
          <div className={styles.bannerOverlay} />
          <div className={styles.bannerContent}>
            <div className={styles.bannerEmojis}>🐠 🐟 🐡</div>
            <h1 className={styles.bannerTitle}>Blog Cá Cảnh</h1>
            <p className={styles.bannerSubtitle}>
              Dữ liệu bài viết được đồng bộ trực tiếp từ hệ thống quản trị
            </p>
          </div>
        </div>

        <div className={styles.mainWrap}>
          <Row gutter={[32, 32]}>
            <Col xs={24} lg={17}>
              {featuredPost && (
                <div className={styles.featuredSection}>
                  <div className={styles.sectionLabel}>
                    <FireOutlined /> Bài viết nổi bật
                  </div>
                  <div className={styles.featuredCard} onClick={() => openPost(featuredPost)}>
                    {renderVisual(featuredPost, 0, true)}
                    <div className={styles.featuredBody}>
                      <h2 className={styles.featuredTitle}>{featuredPost.title}</h2>
                      <p className={styles.featuredExcerpt}>{getExcerpt(featuredPost)}</p>
                      <div className={styles.featuredMeta}>
                        <span><UserOutlined /> Fish Shop</span>
                        <span><ClockCircleOutlined /> {getPostDate(featuredPost)}</span>
                        <span>{getReadTime(featuredPost)}</span>
                        <span><EyeOutlined /> {getPostViews(featuredPost)}</span>
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
              )}

              <div className={styles.filterBar}>
                <button
                  className={`${styles.filterBtn} ${activeCategoryId === 'all' ? styles.filterActive : ''}`}
                  onClick={() => handleCategoryChange('all')}
                >
                  Tất cả
                </button>
                {categories.map((cat) => (
                  <button
                    key={cat.id}
                    className={`${styles.filterBtn} ${activeCategoryId === cat.id ? styles.filterActive : ''}`}
                    onClick={() => handleCategoryChange(cat.id)}
                  >
                    {cat.name}
                  </button>
                ))}
              </div>

              <Spin spinning={loading} className={styles.loadingWrap}>
                {posts.length === 0 ? (
                  <div className={styles.emptyMsg}>Không có bài viết phù hợp.</div>
                ) : (
                  <Row gutter={[20, 24]}>
                    {posts.map((post, index) => (
                      <Col xs={24} sm={12} md={8} key={post.id}>
                        <div className={styles.postCard} onClick={() => openPost(post)}>
                          <div className={styles.postImgWrap}>
                            {renderVisual(post, index + 1)}
                          </div>
                          <div className={styles.postBody}>
                            <Tag
                              color={catColors[index % catColors.length]}
                              className={styles.postTag}
                            >
                              {post.categoryName || 'Blog'}
                            </Tag>
                            <h3 className={styles.postTitle}>{post.title}</h3>
                            <p className={styles.postExcerpt}>{getExcerpt(post)}</p>
                            <div className={styles.postMeta}>
                              <span><UserOutlined /> Fish Shop</span>
                              <span><ClockCircleOutlined /> {getPostDate(post)}</span>
                            </div>
                            <div className={styles.postFooter}>
                              <span className={styles.readTime}>{getReadTime(post)}</span>
                              <span className={styles.views}><EyeOutlined /> {getPostViews(post)}</span>
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
              </Spin>
            </Col>

            <Col xs={24} lg={7}>
              <div className={styles.sidebar}>
                <div className={styles.sideWidget}>
                  <h4 className={styles.widgetTitle}><SearchOutlined /> Tìm kiếm</h4>
                  <Search
                    placeholder="Tìm bài viết..."
                    value={searchText}
                    onChange={(event) => {
                      const nextValue = event.target.value;
                      setSearchText(nextValue);
                      if (!nextValue.trim() && keyword) {
                        setKeyword('');
                      }
                    }}
                    onSearch={handleSearch}
                    allowClear
                    className={styles.searchInput}
                  />
                </div>

                <div className={styles.sideWidget}>
                  <h4 className={styles.widgetTitle}><UnorderedListOutlined /> Danh mục</h4>
                  <ul className={styles.catList}>
                    {categories.map((cat) => (
                      <li
                        key={cat.id}
                        className={`${styles.catItem} ${activeCategoryId === cat.id ? styles.catItemActive : ''}`}
                        onClick={() => handleCategoryChange(cat.id)}
                      >
                        <span>{cat.name}</span>
                        <span className={styles.catCount}>
                          {cat.postCount ?? categoryCountMap.get(cat.id) ?? 0}
                        </span>
                      </li>
                    ))}
                  </ul>
                </div>

                <div className={styles.sideWidget}>
                  <h4 className={styles.widgetTitle}><FireOutlined /> Bài viết nổi bật</h4>
                  <div className={styles.sidePostList}>
                    {popularPosts.map((post, index) => (
                      <div key={post.id} className={styles.sidePost} onClick={() => openPost(post)}>
                        <div className={styles.sidePostNum}>{index + 1}</div>
                        <div className={styles.sidePostInfo}>
                          <div className={styles.sidePostTitle}>{post.title}</div>
                          <div className={styles.sidePostMeta}><EyeOutlined /> {getPostViews(post)}</div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                <div className={styles.sideWidget}>
                  <h4 className={styles.widgetTitle}><ClockCircleOutlined /> Bài viết mới</h4>
                  <div className={styles.sidePostList}>
                    {recentPosts.map((post, index) => (
                      <div key={post.id} className={styles.sidePost} onClick={() => openPost(post)}>
                        <div
                          className={styles.sidePostThumb}
                          style={{ background: gradients[index % gradients.length] }}
                        >
                          <span>{fallbackEmojis[index % fallbackEmojis.length]}</span>
                        </div>
                        <div className={styles.sidePostInfo}>
                          <div className={styles.sidePostTitle}>{post.title}</div>
                          <div className={styles.sidePostMeta}>
                            <ClockCircleOutlined /> {getPostDate(post)}
                          </div>
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
