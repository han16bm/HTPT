import React from 'react';
import { Row, Col } from 'antd';
import { useNavigate } from 'react-router-dom';
import styles from './CategoryGrid.module.scss';

interface Category {
  slug: string;
  name: string;
  icon: string;
  count?: string;
  color: string;
}

interface CategoryGridProps {
  categories?: Category[];
  title?: string;
  subtitle?: string;
}

const defaultCategories: Category[] = [
  { slug: 'ca-vang', name: 'Cá Vàng', icon: '🐠', count: '120+ sản phẩm', color: '#fff7e6' },
  { slug: 'ca-koi', name: 'Cá Koi', icon: '🐟', count: '80+ sản phẩm', color: '#e6f7ff' },
  { slug: 'ca-bay-mau', name: 'Cá Bảy Màu', icon: '🌈', count: '60+ sản phẩm', color: '#f9f0ff' },
  { slug: 'ca-rong', name: 'Cá Rồng', icon: '🐉', count: '30+ sản phẩm', color: '#fff1f0' },
  { slug: 'ca-discus', name: 'Cá Dĩa', icon: '🫧', count: '45+ sản phẩm', color: '#f0f5ff' },
  { slug: 'ca-be', name: 'Cá Bê (Betta)', icon: '🐡', count: '90+ sản phẩm', color: '#fff0f6' },
  { slug: 'phu-kien', name: 'Phụ Kiện', icon: '🔧', count: '200+ sản phẩm', color: '#f6ffed' },
  { slug: 'thuc-an', name: 'Thức Ăn', icon: '🍖', count: '50+ sản phẩm', color: '#fffbe6' },
];

const CategoryGrid: React.FC<CategoryGridProps> = ({
  categories = defaultCategories,
  title = 'Danh Mục Sản Phẩm',
  subtitle = 'Khám phá hàng ngàn loài cá cảnh và phụ kiện hồ cá',
}) => {
  const navigate = useNavigate();

  return (
    <section className={styles.categoryGrid}>
      <div className={styles.sectionHeader}>
        <h2 className={styles.title}>{title}</h2>
        <p className={styles.subtitle}>{subtitle}</p>
      </div>
      <Row gutter={[16, 16]}>
        {categories.map((cat) => (
          <Col xs={12} sm={8} md={6} lg={3} key={cat.slug}>
            <div
              className={styles.categoryCard}
              style={{ background: cat.color }}
              onClick={() => navigate(`/categories/${cat.slug}`)}
            >
              <div className={styles.icon}>{cat.icon}</div>
              <h3 className={styles.name}>{cat.name}</h3>
              {cat.count && <span className={styles.count}>{cat.count}</span>}
            </div>
          </Col>
        ))}
      </Row>
    </section>
  );
};

export default CategoryGrid;
