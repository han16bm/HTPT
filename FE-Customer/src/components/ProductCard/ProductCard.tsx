import React from 'react';
import { Button, Tag, Badge } from 'antd';
import {
  ShoppingCartOutlined,
  HeartOutlined,
  HeartFilled,
  EyeOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import type { Product } from '@/api';
import { formatVND } from '@/utils/format';
import { resolveImageUrl, useImageFallback } from '@/utils/image';
import styles from './ProductCard.module.scss';

interface ProductCardProps {
  product: Product;
  onAddToCart?: (product: Product) => void;
}

const ProductCard: React.FC<ProductCardProps> = ({ product, onAddToCart }) => {
  const navigate = useNavigate();
  const [isFavorite, setIsFavorite] = React.useState(false);

  const formatPrice = formatVND;

  const handleCardClick = () => {
    if (product.slug) navigate(`/products/${product.slug}`);
    else navigate(`/products`);
  };

  const handleAddToCart = (e: React.MouseEvent) => {
    e.stopPropagation();
    onAddToCart?.(product);
  };

  const handleFavorite = (e: React.MouseEvent) => {
    e.stopPropagation();
    setIsFavorite(!isFavorite);
  };

  const discount = 15; // Example discount percentage

  return (
    <div className={styles.productCard}>
      <div className={styles.imageWrapper}>
        {product.stockQuantity === 0 ? (
          <Badge.Ribbon text="Hết hàng" color="red" className={styles.ribbon}>
            <img
              alt={product.name}
              src={resolveImageUrl(product.imageUrl)}
              onError={useImageFallback}
              className={styles.image}
            />
          </Badge.Ribbon>
        ) : discount > 0 ? (
          <Badge.Ribbon
            text={`-${discount}%`}
            color="orangered"
            placement="start"
            className={styles.ribbon}
          >
            <img
              alt={product.name}
              src={resolveImageUrl(product.imageUrl)}
              onError={useImageFallback}
              className={styles.image}
            />
          </Badge.Ribbon>
        ) : (
          <img
            alt={product.name}
            src={resolveImageUrl(product.imageUrl)}
            onError={useImageFallback}
            className={styles.image}
          />
        )}

        <button
          className={styles.favoriteBtn}
          onClick={handleFavorite}
          title={isFavorite ? 'Xóa khỏi yêu thích' : 'Thêm vào yêu thích'}
        >
          {isFavorite ? (
            <HeartFilled style={{ color: '#ff4d4f' }} />
          ) : (
            <HeartOutlined />
          )}
        </button>

        <div className={styles.hoverOverlay}>
          <Button
            type="primary"
            size="large"
            icon={<ShoppingCartOutlined />}
            onClick={handleAddToCart}
            disabled={product.stockQuantity === 0}
            className={styles.quickAddBtn}
          >
            Thêm giỏ hàng
          </Button>
        </div>
      </div>

      <div className={styles.productContent} onClick={handleCardClick}>
        <h3 className={styles.productName} title={product.name}>
          {product.name}
        </h3>

        <div className={styles.priceSection}>
          <div className={styles.price}>{formatPrice(product.salePrice)}</div>
          {discount > 0 && (
            <div className={styles.originalPrice}>
              {formatPrice(Math.round(product.salePrice / (1 - discount / 100)))}
            </div>
          )}
        </div>

        <div className={styles.stock}>
          {product.stockQuantity > 0 ? (
            <Tag color="green">Còn {product.stockQuantity} sản phẩm</Tag>
          ) : (
            <Tag color="red">Hết hàng</Tag>
          )}
        </div>

        <div className={styles.footer}>
          <small className={styles.sold}>Đã bán: {product.soldQuantity}</small>
        </div>
      </div>

      <div className={styles.cardActions}>
        <Button
          icon={<EyeOutlined />}
          onClick={handleCardClick}
          className={styles.detailBtn}
        >
          Xem chi tiết
        </Button>
        <Button
          type="primary"
          icon={<ShoppingCartOutlined />}
          onClick={handleAddToCart}
          disabled={product.stockQuantity === 0}
          className={styles.addToCartBtn}
        >
          {product.stockQuantity === 0 ? 'Hết hàng' : 'Mua ngay'}
        </Button>
      </div>
    </div>
  );
};

export default ProductCard;
