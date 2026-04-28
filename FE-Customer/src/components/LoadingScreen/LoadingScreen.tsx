import React, { useEffect, useState } from 'react';
import styles from './LoadingScreen.module.scss';

interface LoadingScreenProps {
  onFinish: () => void;
}

const FISH_EMOJIS = ['🐟', '🐠', '🐡', '🦈', '🐬'];

const LoadingScreen: React.FC<LoadingScreenProps> = ({ onFinish }) => {
  const [fadeOut, setFadeOut] = useState(false);

  useEffect(() => {
    const showTimer = setTimeout(() => {
      setFadeOut(true);
      // Wait for CSS fade-out transition, then unmount
      setTimeout(onFinish, 480);
    }, 2200);

    return () => clearTimeout(showTimer);
  }, [onFinish]);

  return (
    <div className={`${styles.overlay} ${fadeOut ? styles.hidden : ''}`}>
      <div className={styles.stage}>
        {/* Bouncing fish row */}
        <div className={styles.fishRow}>
          {FISH_EMOJIS.map((emoji, i) => (
            <span
              key={i}
              className={styles.fish}
              style={{ animationDelay: `${i * 0.18}s` }}
            >
              {emoji}
            </span>
          ))}
        </div>

        {/* Brand */}
        <div className={styles.brand}>
          <span className={styles.brandIcon}>🐟</span>
          <span className={styles.brandName}>H&amp;H Fish Shop</span>
        </div>

        {/* Pulsing bubble dots (progress indicator) */}
        <div className={styles.dotsRow}>
          {Array.from({ length: 6 }, (_, i) => (
            <span
              key={i}
              className={styles.dot}
              style={{ animationDelay: `${i * 0.14}s` }}
            />
          ))}
        </div>

        <p className={styles.hint}>Đang tải dữ liệu...</p>
      </div>
    </div>
  );
};

export default LoadingScreen;
