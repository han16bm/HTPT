import React, { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Row, Col, Input, Select, Pagination, Empty, Spin, Tag, message } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import { CustomerLayout } from '@/components/Layout';
import { ProductCard } from '@/components/ProductCard';
import { productService, cartService, authService, type Product } from '@/api';
import styles from './SearchResults.module.scss';

const { Option } = Select;

const SearchResults: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const query = searchParams.get('q') || '';

  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(false);
  const [inputValue, setInputValue] = useState(query);
  const [sort, setSort] = useState('newest');
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState(0);
  const pageSize = 12;

  useEffect(() => {
    setInputValue(query);
    if (!query) {
      setProducts([]);
      setTotal(0);
      return;
    }

    void fetchResults(query);
  }, [query, sort, page]);

  const fetchResults = async (keyword: string) => {
    setLoading(true);
    try {
      const res = await productService.getAll({
        page,
        pageSize,
        search: keyword,
        sort,
      });
      if (res.success && res.data) {
        setProducts(res.data.items);
        setTotal(res.data.totalCount);
      }
    } catch {
      message.error('Không thể tìm kiếm');
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = () => {
    const nextQuery = inputValue.trim();
    setPage(1);

    if (nextQuery) {
      setSearchParams({ q: nextQuery });
      return;
    }

    setSearchParams({});
  };

  const handleSortChange = (value: string) => {
    setSort(value);
    setPage(1);
  };

  const applySuggestion = (value: string) => {
    setPage(1);
    setSearchParams({ q: value });
  };

  const handleAddToCart = async (product: Product) => {
    if (!authService.isAuthenticated()) {
      navigate(`/login?returnUrl=${encodeURIComponent(window.location.pathname + window.location.search)}`);
      return;
    }
    try {
      const res = await cartService.addToCart(product.id, 1);
      if (res.success) {
        message.success('Đã thêm vào giỏ hàng');
        if (res.data) {
          window.dispatchEvent(new CustomEvent('cart-updated', { detail: res.data.totalItems }));
        }
      } else {
        message.error(res.error || 'Không thể thêm vào giỏ hàng');
      }
    } catch {
      message.error('Đã xảy ra lỗi');
    }
  };

  const suggestions = ['Cá Koi', 'Cá Vàng', 'Bể cá mini', 'Máy lọc nước', 'Thức ăn cá', 'Đèn LED bể cá'];

  return (
    <CustomerLayout>
      <div className={styles.searchPage}>
        <div className={styles.searchBar}>
          <Input.Search
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onSearch={handleSearch}
            onPressEnter={handleSearch}
            placeholder="Tìm kiếm sản phẩm..."
            size="large"
            enterButton={<><SearchOutlined /> Tìm kiếm</>}
            className={styles.searchInput}
          />
        </div>

        {!query && (
          <div className={styles.suggestions}>
            <span className={styles.suggLabel}>Gợi ý tìm kiếm:</span>
            {suggestions.map((suggestion) => (
              <Tag
                key={suggestion}
                className={styles.suggTag}
                style={{ cursor: 'pointer' }}
                onClick={() => applySuggestion(suggestion)}
              >
                {suggestion}
              </Tag>
            ))}
          </div>
        )}

        {query && (
          <>
            <div className={styles.resultsHeader}>
              <span className={styles.resultCount}>
                {loading ? 'Đang tìm...' : `Tìm thấy ${total} kết quả cho `}
                {!loading && <strong>"{query}"</strong>}
              </span>
              <Select
                value={sort}
                onChange={handleSortChange}
                size="middle"
                className={styles.sortSelect}
              >
                <Option value="newest">Mới nhất</Option>
                <Option value="price-asc">Giá thấp → cao</Option>
                <Option value="price-desc">Giá cao → thấp</Option>
                <Option value="best-seller">Bán chạy</Option>
              </Select>
            </div>

            <Spin spinning={loading}>
              {products.length === 0 && !loading ? (
                <div className={styles.emptyState}>
                  <Empty
                    description={
                      <span>
                        Không tìm thấy kết quả cho <strong>"{query}"</strong>
                        <br />
                        Hãy thử từ khóa khác hoặc duyệt danh mục sản phẩm
                      </span>
                    }
                  />
                  <div className={styles.emptySuggestions}>
                    {suggestions.map((suggestion) => (
                      <Tag
                        key={suggestion}
                        style={{ cursor: 'pointer', fontSize: 14 }}
                        onClick={() => applySuggestion(suggestion)}
                      >
                        {suggestion}
                      </Tag>
                    ))}
                  </div>
                </div>
              ) : (
                <>
                  <Row gutter={[16, 24]}>
                    {products.map((product) => (
                      <Col xs={12} sm={8} md={6} lg={4} key={product.id}>
                        <ProductCard product={product} onAddToCart={handleAddToCart} />
                      </Col>
                    ))}
                  </Row>
                  <div className={styles.pagination}>
                    <Pagination
                      current={page}
                      pageSize={pageSize}
                      total={total}
                      onChange={setPage}
                      showSizeChanger={false}
                    />
                  </div>
                </>
              )}
            </Spin>
          </>
        )}
      </div>
    </CustomerLayout>
  );
};

export default SearchResults;
