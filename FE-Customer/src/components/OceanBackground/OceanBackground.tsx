import React, { useEffect, useRef } from 'react';
import styles from './OceanBackground.module.scss';

// ── Types ──────────────────────────────────────────────────────────────────────
interface Bubble {
  x: number;
  y: number;
  r: number;
  speed: number;
  opacity: number;
  drift: number;
  phase: number;
  wobble: number;
}

interface Caustic {
  x: number;
  y: number;
  w: number;
  h: number;
  opacity: number;
  speed: number;
  phase: number;
  scale: number;
}

// ── Canvas renderer ────────────────────────────────────────────────────────────
const OceanBackground: React.FC = () => {
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    let animId: number;
    let W = 0;
    let H = 0;
    const bubbles: Bubble[] = [];
    const caustics: Caustic[] = [];

    // ── resize ──────────────────────────────────────────────────────────────
    const resize = () => {
      W = window.innerWidth;
      H = window.innerHeight;
      canvas.width = W;
      canvas.height = H;
    };
    resize();
    window.addEventListener('resize', resize, { passive: true });

    // ── seed bubbles ────────────────────────────────────────────────────────
    const BUBBLE_COUNT = Math.min(55, Math.floor(window.innerWidth / 22));
    for (let i = 0; i < BUBBLE_COUNT; i++) {
      bubbles.push({
        x: Math.random() * W,
        y: Math.random() * H,
        r: 2.5 + Math.random() * 11,
        speed: 0.18 + Math.random() * 0.52,
        opacity: 0.05 + Math.random() * 0.16,
        drift: (Math.random() - 0.5) * 0.38,
        phase: Math.random() * Math.PI * 2,
        wobble: 0.012 + Math.random() * 0.022,
      });
    }

    // ── seed caustics (light rays) ───────────────────────────────────────────
    for (let i = 0; i < 9; i++) {
      caustics.push({
        x: (i / 9) * W + Math.random() * (W / 9),
        y: -80,
        w: 60 + Math.random() * 140,
        h: H * (0.55 + Math.random() * 0.45),
        opacity: 0.022 + Math.random() * 0.038,
        speed: 0.0006 + Math.random() * 0.0008,
        phase: Math.random() * Math.PI * 2,
        scale: 0.85 + Math.random() * 0.3,
      });
    }

    // ── draw frame ──────────────────────────────────────────────────────────
    const draw = (_t: number) => {
      ctx.clearRect(0, 0, W, H);

      // --- caustic light shafts ---
      caustics.forEach((c) => {
        c.phase += c.speed;
        const sway = Math.sin(c.phase) * 28;
        const alpha = c.opacity * (0.7 + 0.3 * Math.sin(c.phase * 1.7));
        const grad = ctx.createLinearGradient(c.x + sway, c.y, c.x + sway, c.y + c.h);
        grad.addColorStop(0, `rgba(120, 220, 255, ${alpha})`);
        grad.addColorStop(0.5, `rgba(80, 200, 230, ${alpha * 0.55})`);
        grad.addColorStop(1, `rgba(30, 120, 180, 0)`);

        ctx.save();
        ctx.beginPath();
        const halfW = (c.w * c.scale) / 2;
        ctx.moveTo(c.x + sway - halfW * 0.3, c.y);
        ctx.lineTo(c.x + sway + halfW * 0.3, c.y);
        ctx.lineTo(c.x + sway + halfW, c.y + c.h);
        ctx.lineTo(c.x + sway - halfW, c.y + c.h);
        ctx.closePath();
        ctx.fillStyle = grad;
        ctx.fill();
        ctx.restore();
      });

      // --- bubbles ---
      bubbles.forEach((b) => {
        b.phase += b.wobble;
        b.y -= b.speed;
        b.x += Math.sin(b.phase) * b.drift;

        if (b.y + b.r < 0) {
          b.y = H + b.r;
          b.x = Math.random() * W;
        }

        // outer ring
        ctx.beginPath();
        ctx.arc(b.x, b.y, b.r, 0, Math.PI * 2);
        ctx.strokeStyle = `rgba(180, 240, 255, ${b.opacity})`;
        ctx.lineWidth = 1;
        ctx.stroke();

        // inner specular highlight
        ctx.beginPath();
        ctx.arc(b.x - b.r * 0.3, b.y - b.r * 0.3, b.r * 0.25, 0, Math.PI * 2);
        ctx.fillStyle = `rgba(255, 255, 255, ${b.opacity * 0.7})`;
        ctx.fill();
      });

      animId = requestAnimationFrame(draw);
    };

    animId = requestAnimationFrame(draw);

    return () => {
      cancelAnimationFrame(animId);
      window.removeEventListener('resize', resize);
    };
  }, []);

  return (
    <div className={styles.ocean} aria-hidden="true">
      {/* Pure CSS gradient base layer */}
      <div className={styles.gradientBase} />

      {/* Animated gradient overlay (shimmer) */}
      <div className={styles.gradientShimmer} />

      {/* Canvas: bubbles + caustic rays */}
      <canvas ref={canvasRef} className={styles.canvas} />

      {/* Seaweed silhouettes */}
      <div className={styles.seaFloor}>
        <div className={styles.seaweed} style={{ left: '3%',  animationDuration: '4.2s', animationDelay: '0s',    height: 110 }} />
        <div className={styles.seaweed} style={{ left: '8%',  animationDuration: '3.6s', animationDelay: '0.7s',  height: 80  }} />
        <div className={styles.seaweed} style={{ left: '15%', animationDuration: '5.1s', animationDelay: '0.3s',  height: 130 }} />
        <div className={styles.seaweed} style={{ left: '22%', animationDuration: '3.9s', animationDelay: '1.1s',  height: 90  }} />
        <div className={styles.coral}   style={{ left: '30%' }} />
        <div className={styles.seaweed} style={{ left: '38%', animationDuration: '4.5s', animationDelay: '0.5s',  height: 115 }} />
        <div className={styles.seaweed} style={{ left: '46%', animationDuration: '3.3s', animationDelay: '1.4s',  height: 75  }} />
        <div className={styles.coral}   style={{ left: '54%' }} />
        <div className={styles.seaweed} style={{ left: '61%', animationDuration: '4.8s', animationDelay: '0.2s',  height: 125 }} />
        <div className={styles.seaweed} style={{ left: '70%', animationDuration: '3.7s', animationDelay: '0.9s',  height: 95  }} />
        <div className={styles.coral}   style={{ left: '76%' }} />
        <div className={styles.seaweed} style={{ left: '83%', animationDuration: '4.0s', animationDelay: '0.4s',  height: 105 }} />
        <div className={styles.seaweed} style={{ left: '90%', animationDuration: '5.3s', animationDelay: '1.2s',  height: 140 }} />
        <div className={styles.seaweed} style={{ left: '96%', animationDuration: '3.5s', animationDelay: '0.6s',  height: 85  }} />
      </div>
    </div>
  );
};

export default OceanBackground;
