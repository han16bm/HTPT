import React, { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './PromoBanner.module.scss';

export interface BannerConfig {
  id: string;
  title: string;
  subtitle: string;
  ctaText: string;
  ctaLink: string;
  theme: 'koi' | 'promo' | 'dragon' | 'store';
  emoji: string;
  tag?: string;
}

interface Props {
  banner: BannerConfig;
  className?: string;
}

const PromoBanner: React.FC<Props> = ({ banner, className }) => {
  const navigate = useNavigate();
  const ref = useRef<HTMLDivElement>(null);
  const [visible, setVisible] = useState(false);
  const [parallax, setParallax] = useState(0);
  const rafRef = useRef<number | null>(null);

  // Fade-in when scrolled into view
  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) setVisible(true);
      },
      { threshold: 0.08 },
    );
    if (ref.current) observer.observe(ref.current);
    return () => observer.disconnect();
  }, []);

  // Subtle parallax on background layer
  useEffect(() => {
    const onScroll = () => {
      if (rafRef.current !== null) cancelAnimationFrame(rafRef.current);
      rafRef.current = requestAnimationFrame(() => {
        if (!ref.current) return;
        const { top } = ref.current.getBoundingClientRect();
        setParallax(top * 0.12);
      });
    };
    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
    return () => {
      window.removeEventListener('scroll', onScroll);
      if (rafRef.current !== null) cancelAnimationFrame(rafRef.current);
    };
  }, []);

  const handleCta = (e: React.MouseEvent) => {
    e.stopPropagation();
    navigate(banner.ctaLink);
  };

  return (
    <div
      ref={ref}
      className={[
        styles.banner,
        styles[banner.theme],
        visible ? styles.visible : '',
        className ?? '',
      ]
        .filter(Boolean)
        .join(' ')}
      onClick={handleCta}
      role="region"
      aria-label={banner.title}
    >
      {/* Parallax gradient background */}
      <div
        className={styles.bgLayer}
        style={{ transform: `translateY(${parallax}px)` }}
      />

      {/* Decorative circles */}
      <div className={styles.decorCircle1} aria-hidden />
      <div className={styles.decorCircle2} aria-hidden />
      <div className={styles.decorCircle3} aria-hidden />

      {/* Content */}
      <div className={styles.content}>
        {banner.tag && <span className={styles.tag}>{banner.tag}</span>}
        <span className={styles.emoji} aria-hidden="true">
          {banner.emoji}
        </span>
        <h2 className={styles.title}>{banner.title}</h2>
        <p className={styles.subtitle}>{banner.subtitle}</p>
        <button className={styles.ctaBtn} onClick={handleCta} type="button">
          {banner.ctaText}
          <span className={styles.ctaArrow} aria-hidden>
            →
          </span>
        </button>
      </div>
    </div>
  );
};

export default PromoBanner;
