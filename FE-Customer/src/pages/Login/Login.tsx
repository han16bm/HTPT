import React, { useEffect, useRef, useState } from 'react';
import { Form, Input, Button, Checkbox, message } from 'antd';
import { UserOutlined, LockOutlined, EyeInvisibleOutlined, EyeTwoTone } from '@ant-design/icons';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { authService } from '@/api';
import styles from './Login.module.scss';

// ── Bubble canvas background ──────────────────────────────────────────────────
const BubbleCanvas: React.FC = () => {
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    let animId: number;
    const bubbles: { x: number; y: number; r: number; speed: number; opacity: number; drift: number; phase: number }[] = [];

    const resize = () => {
      canvas.width = canvas.offsetWidth;
      canvas.height = canvas.offsetHeight;
    };
    resize();
    window.addEventListener('resize', resize);

    for (let i = 0; i < 28; i++) {
      bubbles.push({
        x: Math.random() * canvas.width,
        y: Math.random() * canvas.height,
        r: 3 + Math.random() * 14,
        speed: 0.25 + Math.random() * 0.6,
        opacity: 0.06 + Math.random() * 0.18,
        drift: (Math.random() - 0.5) * 0.4,
        phase: Math.random() * Math.PI * 2,
      });
    }

    const draw = () => {
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      bubbles.forEach((b) => {
        b.y -= b.speed;
        b.x += Math.sin(b.phase) * b.drift;
        b.phase += 0.018;
        if (b.y + b.r < 0) {
          b.y = canvas.height + b.r;
          b.x = Math.random() * canvas.width;
        }
        ctx.beginPath();
        ctx.arc(b.x, b.y, b.r, 0, Math.PI * 2);
        ctx.strokeStyle = `rgba(255,255,255,${b.opacity})`;
        ctx.lineWidth = 1.5;
        ctx.stroke();
        // inner highlight
        ctx.beginPath();
        ctx.arc(b.x - b.r * 0.28, b.y - b.r * 0.28, b.r * 0.28, 0, Math.PI * 2);
        ctx.fillStyle = `rgba(255,255,255,${b.opacity * 0.6})`;
        ctx.fill();
      });
      animId = requestAnimationFrame(draw);
    };
    draw();

    return () => {
      cancelAnimationFrame(animId);
      window.removeEventListener('resize', resize);
    };
  }, []);

  return <canvas ref={canvasRef} className={styles.bubbleCanvas} aria-hidden />;
};

// ── Main Login page ───────────────────────────────────────────────────────────
const Login: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [loading, setLoading] = useState(false);

  const handleLogin = async (values: { username: string; password: string; remember?: boolean }) => {
    setLoading(true);
    try {
      const response = await authService.login({ username: values.username, password: values.password });
      if (response.success) {
        if (values.remember) {
          localStorage.setItem('remember_user', values.username);
        } else {
          localStorage.removeItem('remember_user');
        }
        message.success('Đăng nhập thành công!');
        // Redirect về trang cũ nếu có returnUrl, không thì về trang chủ
        const returnUrl = searchParams.get('returnUrl') || '/';
        navigate(returnUrl, { replace: true });
      } else {
        message.error(response.error || 'Tên đăng nhập hoặc mật khẩu không đúng');
      }
    } catch {
      message.error('Đã xảy ra lỗi, vui lòng thử lại');
    } finally {
      setLoading(false);
    }
  };

  const savedUser = localStorage.getItem('remember_user') || '';

  return (
    <div className={styles.page}>
      {/* ── Left panel ── */}
      <div className={styles.leftPanel}>
        <BubbleCanvas />

        {/* Fish silhouette decorators */}
        <div className={styles.fishDeco1} aria-hidden>🐠</div>
        <div className={styles.fishDeco2} aria-hidden>🐟</div>
        <div className={styles.fishDeco3} aria-hidden>🐡</div>

        <div className={styles.brandBlock}>
          <div className={styles.brandLogo}>🐟</div>
          <h1 className={styles.brandName}>H&H Fish Shop</h1>
          <p className={styles.brandTagline}>Fish Shop</p>
          <p className={styles.brandDesc}>
            Thiên đường cá cảnh cao cấp — nơi mỗi chú cá là
            một tác phẩm nghệ thuật sống động.
          </p>
          <div className={styles.features}>
            <div className={styles.featureItem}><span>✦</span> Cá nhập khẩu chính hãng</div>
            <div className={styles.featureItem}><span>✦</span> Giao hàng toàn quốc</div>
            <div className={styles.featureItem}><span>✦</span> Tư vấn chuyên nghiệp 24/7</div>
          </div>
        </div>

        {/* Wave overlay at bottom */}
        <div className={styles.wave} aria-hidden>
          <svg viewBox="0 0 1200 120" preserveAspectRatio="none">
            <path d="M0,60 C300,120 900,0 1200,60 L1200,120 L0,120 Z" fill="rgba(255,255,255,0.08)" />
            <path d="M0,80 C400,20 800,140 1200,80 L1200,120 L0,120 Z" fill="rgba(255,255,255,0.05)" />
          </svg>
        </div>
      </div>

      {/* ── Right panel ── */}
      <div className={styles.rightPanel}>
        <div className={styles.formCard}>
          {/* Logo on mobile */}
          <div className={styles.mobileLogo} aria-hidden>🐟</div>

          <h2 className={styles.formTitle}>Đăng nhập</h2>
          <p className={styles.formSubtitle}>Chào mừng bạn trở lại!</p>

          <Form
            layout="vertical"
            onFinish={handleLogin}
            autoComplete="off"
            initialValues={{ username: savedUser, remember: !!savedUser }}
            className={styles.form}
          >
            <Form.Item
              name="username"
              label="Tên đăng nhập"
              rules={[{ required: true, message: 'Vui lòng nhập tên đăng nhập' }]}
            >
              <Input
                size="large"
                prefix={<UserOutlined className={styles.inputIcon} />}
                placeholder="Nhập tên đăng nhập hoặc email"
                className={styles.input}
              />
            </Form.Item>

            <Form.Item
              name="password"
              label="Mật khẩu"
              rules={[{ required: true, message: 'Vui lòng nhập mật khẩu' }]}
            >
              <Input.Password
                size="large"
                prefix={<LockOutlined className={styles.inputIcon} />}
                placeholder="Nhập mật khẩu"
                iconRender={(visible) => (visible ? <EyeTwoTone twoToneColor="#0e7490" /> : <EyeInvisibleOutlined />)}
                className={styles.input}
              />
            </Form.Item>

            <div className={styles.formRow}>
              <Form.Item name="remember" valuePropName="checked" noStyle>
                <Checkbox className={styles.remember}>Ghi nhớ đăng nhập</Checkbox>
              </Form.Item>
              <Link to="/forgot-password" className={styles.forgotLink}>Quên mật khẩu?</Link>
            </div>

            <Form.Item style={{ marginTop: 24 }}>
              <Button
                type="primary"
                htmlType="submit"
                size="large"
                loading={loading}
                block
                className={styles.loginBtn}
              >
                {loading ? 'Đang đăng nhập...' : 'Đăng nhập'}
              </Button>
            </Form.Item>
          </Form>

          <div className={styles.dividerRow}>
            <span className={styles.dividerLine} />
            <span className={styles.dividerText}>hoặc</span>
            <span className={styles.dividerLine} />
          </div>

          <p className={styles.registerPrompt}>
            Chưa có tài khoản?{' '}
            <Link to="/register" className={styles.registerLink}>
              Đăng ký ngay →
            </Link>
          </p>

          <p className={styles.backHome}>
            <Link to="/">← Quay về trang chủ</Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Login;

