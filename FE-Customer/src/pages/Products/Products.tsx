import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Row,
  Col,
  Input,
  Select,
  Pagination,
  Spin,
  message,
  Button,
  Space,
  Divider,
  Empty,
} from 'antd';
import {
  SearchOutlined,
  FilterOutlined,
  SortAscendingOutlined,
} from '@ant-design/icons';
import { CustomerLayout } from '@/components/Layout';
import { ProductCard } from '@/components/ProductCard';
import { PromoBanner } from '@/components/PromoBanner';
import {
  productService,
  cartService,
  authService,
  categoryService,
  type Product,
  type Category,
} from '@/api';
import {
  dedupeProducts,
  getCategoryBranchIds,
  paginateProducts,
  sortProducts,
} from '@/utils/productCategory';
import { PRODUCT_PAGE_BANNERS } from '@/data/bannerData';
import styles from './Products.module.scss';

const CHUNK_SIZE = 8;

function chunkArray<T>(arr: T[], size: number): T[][] {
  const chunks: T[][] = [];
  for (let i = 0; i < arr.length; i += size) {
    chunks.push(arr.slice(i, i + size));
  }
  return chunks;
}

const { Search } = Input;

const Products: React.FC = () => {
  const navigate = useNavigate();
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(false);
  const [total, setTotal] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(12);
  const [searchKeyword, setSearchKeyword] = useState('');
  const [searchInputValue, setSearchInputValue] = useState('');
  const [categoryId, setCategoryId] = useState<string>('all');
  const [sortBy, setSortBy] = useState<string>('newest');
  const [showFilters, setShowFilters] = useState(true);

  const currentCategory =
    categories.find((category) => String(category.id) === categoryId) ?? null;
  const selectedCategoryBranchIds = getCategoryBranchIds(
    categories,
    currentCategory?.id
  );
  const selectedCategoryBranchKey = selectedCategoryBranchIds.join(',');
  const selectedCategoryFilterId = selectedCategoryBranchIds[0];
  const hasChildCategories = selectedCategoryBranchIds.length > 1;

  useEffect(() => {
    categoryService.getAll().then((response) => {
      if (response.success && response.data) {
        setCategories(response.data);
      }
    });
  }, []);

  useEffect(() => {
    if (categoryId !== 'all' && !selectedCategoryFilterId) {
      setProducts([]);
      setTotal(0);
      return;
    }

    const fetchProducts = async () => {
      setLoading(true);

      try {
        if (categoryId === 'all' || !hasChildCategories) {
          const response = await productService.getAll({
            page: currentPage,
            pageSize,
            search: searchKeyword,
            categoryId: selectedCategoryFilterId,
            sort: sortBy,
          });

          if (response.success && response.data) {
            setProducts(response.data.items);
            setTotal(response.data.totalCount);
          }

          return;
        }

        const nestedProductGroups = await Promise.all(
          selectedCategoryBranchIds.map((nextCategoryId) =>
            productService.getAllPages({
              search: searchKeyword,
              categoryId: nextCategoryId,
              sort: sortBy,
            })
          )
        );

        const mergedProducts = dedupeProducts(nestedProductGroups.flat());
        const sortedProducts = sortProducts(mergedProducts, sortBy);

        setProducts(paginateProducts(sortedProducts, currentPage, pageSize));
        setTotal(sortedProducts.length);
      } catch {
        message.error('Không thể tải danh sách sản phẩm');
      } finally {
        setLoading(false);
      }
    };

    void fetchProducts();
  }, [
    categoryId,
    currentPage,
    pageSize,
    searchKeyword,
    selectedCategoryBranchKey,
    selectedCategoryFilterId,
    sortBy,
    hasChildCategories,
  ]);

  const handleSearch = (value: string) => {
    const nextKeyword = value.trim();
    setSearchInputValue(value);
    setSearchKeyword(nextKeyword);
    setCurrentPage(1);
  };

  const handleCategoryChange = (value: string) => {
    setCategoryId(value);
    setCurrentPage(1);
  };

  const handleSortChange = (value: string) => {
    setSortBy(value);
    setCurrentPage(1);
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
      const response = await cartService.addToCart(product.id, 1, product.name, product.salePrice, product.imageUrl);
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

  const resetFilters = () => {
    setSearchInputValue('');
    setSearchKeyword('');
    setCategoryId('all');
    setSortBy('newest');
    setCurrentPage(1);
  };

  const categoryNameMap = new Map(
    categories.map((category) => [category.id, category.name])
  );

  const categoryOptions = [
    { value: 'all', label: 'Tất cả danh mục' },
    ...categories.map((category) => ({
      value: String(category.id),
      label: category.parentId
        ? `${categoryNameMap.get(category.parentId) ?? 'Danh mục'} / ${category.name}`
        : category.name,
    })),
  ];

  const sortOptions = [
    { value: 'newest', label: 'Mới nhất' },
    { value: 'price-asc', label: 'Giá thấp đến cao' },
    { value: 'price-desc', label: 'Giá cao đến thấp' },
    { value: 'best-seller', label: 'Bán chạy nhất' },
  ];

  return (
    <CustomerLayout>
      <div className={styles.products}>
        <div className={styles.pageHeader}>
          <h1>Danh sách sản phẩm</h1>
          <p>Khám phá bộ sưu tập cá cảnh và phụ kiện tuyệt vời của chúng tôi</p>
        </div>

        {showFilters && (
          <>
            <div className={styles.filtersBar}>
              <Row gutter={[16, 16]} align="middle">
                <Col xs={24} sm={24} md={12}>
                  <Search
                    placeholder="Tìm kiếm sản phẩm..."
                    allowClear
                    enterButton={<SearchOutlined />}
                    size="large"
                    value={searchInputValue}
                    onChange={(event) => {
                      const nextValue = event.target.value;
                      setSearchInputValue(nextValue);
                      if (!nextValue.trim() && searchKeyword) {
                        setSearchKeyword('');
                        setCurrentPage(1);
                      }
                    }}
                    onSearch={handleSearch}
                    className={styles.searchBox}
                  />
                </Col>
                <Col xs={24} sm={24} md={12}>
                  <Space className={styles.sortControls} wrap>
                    <Select
                      value={categoryId}
                      onChange={handleCategoryChange}
                      className={styles.filterSelect}
                      options={categoryOptions}
                      suffixIcon={<FilterOutlined />}
                    />
                    <Select
                      value={sortBy}
                      onChange={handleSortChange}
                      className={styles.filterSelect}
                      options={sortOptions}
                      suffixIcon={<SortAscendingOutlined />}
                    />
                  </Space>
                </Col>
              </Row>
            </div>

            <Divider className={styles.divider} />
          </>
        )}

        <div className={styles.resultsInfo}>
          <span>
            Hiển thị <strong>{products.length}</strong> trong <strong>{total}</strong>{' '}
            sản phẩm
          </span>
          <Button
            type="text"
            size="small"
            onClick={() => setShowFilters((prev) => !prev)}
          >
            {showFilters ? 'Ẩn bộ lọc' : 'Hiện bộ lọc'}
          </Button>
        </div>

        <Spin spinning={loading} className={styles.spinner}>
          {products.length > 0 ? (
            <>
              {chunkArray(products, CHUNK_SIZE).map((chunk, chunkIdx, arr) => (
                <React.Fragment key={chunkIdx}>
                  <Row gutter={[16, 24]} className={styles.productsGrid}>
                    {chunk.map((product) => (
                      <Col xs={12} sm={8} md={6} lg={4} key={product.id}>
                        <ProductCard product={product} onAddToCart={handleAddToCart} />
                      </Col>
                    ))}
                  </Row>
                  {(arr.length === 1 || chunkIdx < arr.length - 1) && (
                    <PromoBanner
                      banner={
                        PRODUCT_PAGE_BANNERS[
                          chunkIdx % PRODUCT_PAGE_BANNERS.length
                        ]
                      }
                    />
                  )}
                </React.Fragment>
              ))}

              <div className={styles.paginationSection}>
                <Pagination
                  current={currentPage}
                  pageSize={pageSize}
                  total={total}
                  onChange={(page) => setCurrentPage(page)}
                  onShowSizeChange={(_, size) => {
                    setPageSize(size);
                    setCurrentPage(1);
                  }}
                  showSizeChanger
                  showQuickJumper
                  showTotal={(currentTotal) => `Tổng ${currentTotal} sản phẩm`}
                  pageSizeOptions={['12', '24', '48']}
                />
              </div>
            </>
          ) : (
            <Empty
              description="Không tìm thấy sản phẩm nào"
              className={styles.emptyState}
              style={{ marginTop: 60 }}
            >
              <Button type="primary" onClick={resetFilters}>
                Xem tất cả sản phẩm
              </Button>
            </Empty>
          )}
        </Spin>
      </div>
    </CustomerLayout>
  );
};

export default Products;