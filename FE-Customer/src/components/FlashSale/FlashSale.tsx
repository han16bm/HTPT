import React, { useEffect, useState } from 'react';
import { Row, Col, Button } from 'antd';
import { ThunderboltFilled, FireFilled } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { ProductCard } from '@/components/ProductCard';
import { productService, cartService, type Product } from '@/api';
import styles from './FlashSale.module.scss';

interface FlashSaleProps {
  title?: string;
  endTime?: Date;
}

const FlashSale: React.FC<FlashSaleProps> = ({
  title = 'Flash Sale',
  endTime = new Date(Date.now() + 8 * 60 * 60 * 1000), // 8 hours from now
}) => {
  const navigate = useNavigate();
  const [products, setProducts] = useState<Product[]>([]);
  const [timeLeft, setTimeLeft] = useState({ hours: 0, minutes: 0, seconds: 0 });

  useEffect(() => {
    productService.getAll({ page: 1, pageSize: 6 }).then((res) => {
      if (res.success && res.data) setProducts(res.data.items.slice(0, 6));
    });
  }, []);

  useEffect(() => {
    const calc = () => {
      const diff = Math.max(0, endTime.getTime() - Date.now());
      const h = Math.floor(diff / 3600000);
      const m = Math.floor((diff % 3600000) / 60000);
      const s = Math.floor((diff % 60000) / 1000);
      setTimeLeft({ hours: h, minutes: m, seconds: s });
    };
    calc();
    const id = setInterval(calc, 1000);
    return () => clearInterval(id);
  }, [endTime]);

  const handleAddToCart = async (product: Product) => {
    await cartService.addToCart(product.id, 1);
  };

  const pad = (n: number) => String(n).padStart(2, '0');

  return (
    <section className={styles.flashSale}>
      <div className={styles.header}>
        <div className={styles.titleRow}>
          <ThunderboltFilled className={styles.icon} />
          <h2 className={styles.title}>{title}</h2>
          <FireFilled className={styles.icon} />
        </div>
        <div className={styles.countdown}>
          <span className={styles.countLabel}>Kết thúc sau:</span>
          <div className={styles.timer}>
            <span className={styles.timeBlock}>{pad(timeLeft.hours)}</span>
            <span className={styles.sep}>:</span>
            <span className={styles.timeBlock}>{pad(timeLeft.minutes)}</span>
            <span className={styles.sep}>:</span>
            <span className={styles.timeBlock}>{pad(timeLeft.seconds)}</span>
          </div>
        </div>
        <Button type="link" className={styles.viewAll} onClick={() => navigate('/promotions')}>
          Xem tất cả →
        </Button>
      </div>

      <Row gutter={[16, 16]}>
        {products.map((product) => (
          <Col xs={12} sm={8} md={6} lg={4} key={product.id}>
            <div className={styles.saleProductWrapper}>
              <div className={styles.discountBadge}>-20%</div>
              <ProductCard product={product} onAddToCart={handleAddToCart} />
            </div>
          </Col>
        ))}
      </Row>
    </section>
  );
};

export default FlashSale;
