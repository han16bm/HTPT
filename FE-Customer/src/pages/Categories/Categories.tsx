import React, { useEffect, useState } from 'react';
import { useNavigate, useLocation, useSearchParams } from 'react-router-dom';
import {
  Row,
  Col,
  Select,
  Pagination,
  Empty,
  Breadcrumb,
  Spin,
  message,
  Slider,
  Button,
} from 'antd';
import { HomeOutlined, FilterOutlined, ReloadOutlined } from '@ant-design/icons';
import { CustomerLayout } from '@/components/Layout';
import { ProductCard } from '@/components/ProductCard';
import {
  productService,
  cartService,
  authService,
  categoryService,
  type Category,
  type Product,
} from '@/api';
import {
  dedupeProducts,
  getCategoryBranchIds,
  paginateProducts,
  sortProducts,
} from '@/utils/productCategory';
import styles from './Categories.module.scss';

const CHUNK_SIZE = 12;
const PAGE_SIZE = 12;
const MAX_PRICE = 1_000_000;

function chunkArray<T>(arr: T[], size: number): T[][] {
  const chunks: T[][] = [];
  for (let i = 0; i < arr.length; i += size) {
    chunks.push(arr.slice(i, i + size));
  }
  return chunks;
}

const { Option } = Select;

const Categories: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();

  const initialSlug =
    searchParams.get('category')
    ?? ((location.state as { slug?: string } | null)?.slug ?? null);

  const [navCategories, setNavCategories] = useState<Category[]>([]);
  const [selectedSlug, setSelectedSlug] = useState<string | null>(initialSlug);
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(false);
  const [sort, setSort] = useState('newest');
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const [priceRange, setPriceRange] = useState<[number, number]>([0, MAX_PRICE]);

  const currentCategory =
    navCategories.find((category) => category.slug === selectedSlug) ?? null;
  const selectedCategoryBranchIds = getCategoryBranchIds(
    navCategories,
    currentCategory?.id
  );
  const selectedCategoryBranchKey = selectedCategoryBranchIds.join(',');
  const selectedCategoryId = selectedCategoryBranchIds[0];
  const hasChildCategories = selectedCategoryBranchIds.length > 1;
  const isFiltered = priceRange[0] !== 0 || priceRange[1] !== MAX_PRICE;

  const formatPrice = (value: number) => {
    if (value === 0) {
      return '0';
    }
    if (value >= 1_000_000) {
      return `${(value / 1_000_000).toFixed(value % 1_000_000 === 0 ? 0 : 1)} triệu`;
    }
    return `${(value / 1_000).toFixed(0)}k`;
  };

  useEffect(() => {
    categoryService.getAll().then((response) => {
      if (response.success && response.data) {
        setNavCategories(response.data);
      }
    });
  }, []);

  useEffect(() => {
    const nextSlug =
      searchParams.get('category')
      ?? ((location.state as { slug?: string } | null)?.slug ?? null);
    setSelectedSlug(nextSlug);
    setPage(1);
  }, [location.state, searchParams]);

  useEffect(() => {
    if (selectedSlug && navCategories.length === 0) {
      return;
    }

    if (selectedSlug && !selectedCategoryId) {
      setProducts([]);
      setTotal(0);
      return;
    }

    const loadProducts = async () => {
      setLoading(true);

      try {
        if (!selectedSlug || !hasChildCategories) {
          const response = await productService.getAll({
            page,
            pageSize: PAGE_SIZE,
            categoryId: selectedCategoryId,
            minPrice: priceRange[0],
            maxPrice: priceRange[1],
            sort,
          });

          if (response.success && response.data) {
            setProducts(response.data.items);
            setTotal(response.data.totalCount);
          }

          return;
        }

        const nestedProductGroups = await Promise.all(
          selectedCategoryBranchIds.map((categoryId) =>
            productService.getAllPages({
              categoryId,
              minPrice: priceRange[0],
              maxPrice: priceRange[1],
              sort,
            })
          )
        );

        const mergedProducts = dedupeProducts(nestedProductGroups.flat());
        const sortedProducts = sortProducts(mergedProducts, sort);

        setProducts(paginateProducts(sortedProducts, page, PAGE_SIZE));
        setTotal(sortedProducts.length);
      } catch {
        message.error('Không thể tải sản phẩm');
      } finally {
        setLoading(false);
      }
    };

    void loadProducts();
  }, [
    hasChildCategories,
    navCategories.length,
    page,
    priceRange,
    selectedCategoryBranchKey,
    selectedCategoryId,
    selectedSlug,
    sort,
  ]);

  const handleCategoryChange = (value: string) => {
    setSelectedSlug(value === 'all' ? null : value);
    setPage(1);
  };

  const handleSortChange = (value: string) => {
    setSort(value);
    setPage(1);
  };

  const handlePriceChange = (value: number | number[]) => {
    setPriceRange(value as [number, number]);
    setPage(1);
  };

  const resetPrice = () => {
    setPriceRange([0, MAX_PRICE]);
    setPage(1);
  };

  const handleAddToCart = async (product: Product) => {
    if (!authService.isAuthenticated()) {
      navigate(
        `/login?returnUrl=${encodeURIComponent(
          window.location.pathname + window.location.search
        )}`
      );
      return;
    }

    try {
      const response = await cartService.addToCart(product.id, 1);
      if (response.success) {
        message.success('Đã thêm vào giỏ hàng');
        if (response.data) {
          window.dispatchEvent(
            new CustomEvent('cart-updated', { detail: response.data.totalItems })
          );
        }
      } else {
        message.error(response.error || 'Không thể thêm vào giỏ hàng');
      }
    } catch {
      message.error('Đã xảy ra lỗi');
    }
  };

  return (
    <CustomerLayout>
      <div className={styles.categoriesPage}>
        <Breadcrumb
          className={styles.breadcrumb}
          items={[
            {
              title: (
                <span onClick={() => navigate('/')} className={styles.breadLink}>
                  <HomeOutlined /> Trang chủ
                </span>
              ),
            },
            { title: 'Danh mục' },
            ...(currentCategory ? [{ title: currentCategory.name }] : []),
          ]}
        />

        <div className={styles.filterBar}>
          <div className={styles.filterColPrice}>
            <div className={styles.filterLabel}>
              <FilterOutlined /> Khoảng giá
              <span className={styles.priceFilterValue}>
                <span className={styles.pricePill}>{formatPrice(priceRange[0])} ₫</span>
                <span className={styles.priceSep}>→</span>
                <span className={styles.pricePill}>{formatPrice(priceRange[1])} ₫</span>
              </span>
              {isFiltered && (
                <Button
                  size="small"
                  icon={<ReloadOutlined />}
                  onClick={resetPrice}
                  className={styles.resetBtn}
                >
                  Reset
                </Button>
              )}
            </div>
            <Slider
              range
              min={0}
              max={MAX_PRICE}
              step={50_000}
              value={priceRange}
              onChange={handlePriceChange}
              tooltip={{ formatter: (value) => `${value?.toLocaleString('vi-VN')} ₫` }}
              className={styles.priceSlider}
            />
          </div>

          <div className={styles.filterCol}>
            <div className={styles.filterLabel}>Loại</div>
            <Select
              value={selectedSlug ?? 'all'}
              onChange={handleCategoryChange}
              className={styles.filterSelect}
              size="large"
            >
              <Option value="all">Tất cả</Option>
              {navCategories.map((category) => (
                <Option key={category.slug} value={category.slug}>
                  {category.name}
                </Option>
              ))}
            </Select>
          </div>

          <div className={styles.filterCol}>
            <div className={styles.filterLabel}>Sắp xếp</div>
            <Select
              value={sort}
              onChange={handleSortChange}
              className={styles.filterSelect}
              size="large"
            >
              <Option value="newest">Mới nhất</Option>
              <Option value="price-asc">Giá thấp đến cao</Option>
              <Option value="price-desc">Giá cao đến thấp</Option>
              <Option value="best-seller">Bán chạy nhất</Option>
            </Select>
          </div>
        </div>

        <Spin spinning={loading}>
          {products.length === 0 && !loading ? (
            <Empty
              description={
                isFiltered
                  ? 'Không có sản phẩm trong khoảng giá này'
                  : currentCategory
                    ? 'Không có sản phẩm trong danh mục này'
                    : 'Không có sản phẩm phù hợp'
              }
            />
          ) : (
            <>
              {chunkArray(products, CHUNK_SIZE).map((chunk, chunkIdx) => (
                <React.Fragment key={chunkIdx}>
                  <Row gutter={[16, 24]}>
                    {chunk.map((product) => (
                      <Col xs={12} sm={8} md={6} lg={4} key={product.id}>
                        <ProductCard product={product} onAddToCart={handleAddToCart} />
                      </Col>
                    ))}
                  </Row>
                </React.Fragment>
              ))}
              <div className={styles.pagination}>
                <Pagination
                  current={page}
                  pageSize={PAGE_SIZE}
                  total={total}
                  onChange={setPage}
                  showSizeChanger={false}
                />
              </div>
            </>
          )}
        </Spin>
      </div>
    </CustomerLayout>
  );
};

export default Categories;