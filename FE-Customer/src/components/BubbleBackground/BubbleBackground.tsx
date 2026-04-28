import React, { useState } from 'react';
import styles from './BubbleBackground.module.scss';

interface Bubble {
  id: number;
  size: number;
  left: number;
  duration: number;
  delay: number;
  opacity: number;
}

interface BubbleBackgroundProps {
  count?: number;
}

const BubbleBackground: React.FC<BubbleBackgroundProps> = ({ count = 14 }) => {
  // Generate once on mount – stable across re-renders
  const [bubbles] = useState<Bubble[]>(() =>
    Array.from({ length: count }, (_, i) => ({
      id: i,
      size: Math.floor(Math.random() * 28) + 8,   // 8 – 36 px
      left: Math.floor(Math.random() * 94) + 1,   // 1 – 95 %
      duration: Math.floor(Math.random() * 8) + 7, // 7 – 15 s
      delay: Math.floor(Math.random() * 14),        // 0 – 14 s
      opacity: +(Math.random() * 0.35 + 0.1).toFixed(2), // 0.10 – 0.45
    })),
  );

  return (
    <div className={styles.container} aria-hidden="true">
      {bubbles.map((b) => (
        <span
          key={b.id}
          className={styles.bubble}
          style={{
            width: b.size,
            height: b.size,
            left: `${b.left}%`,
            animationDuration: `${b.duration}s`,
            animationDelay: `${b.delay}s`,
            opacity: b.opacity,
          }}
        />
      ))}
    </div>
  );
};

export default BubbleBackground;
