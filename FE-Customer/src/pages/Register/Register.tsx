import React, { useEffect, useRef, useState } from 'react';
import { Form, Input, Button, message } from 'antd';
import {
  UserOutlined,
  MailOutlined,
  PhoneOutlined,
  LockOutlined,
  EyeInvisibleOutlined,
  EyeTwoTone,
} from '@ant-design/icons';
import { Link, useNavigate } from 'react-router-dom';
import { authService } from '@/api';
import styles from './Register.module.scss';

// ── Bubble canvas (reuse same visual as Login) ────────────────────────────────
const BubbleCanvas: React.FC = () => {
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    let animId: number;
    const bubbles: {
      x: number; y: number; r: number; speed: number;
      opacity: number; drift: number; phase: number;
    }[] = [];

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

// ── Register page ─────────────────────────────────────────────────────────────
const Register: React.FC = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  const handleRegister = async (values: {
    username: string;
    fullName: string;
    email: string;
    phone: string;
    password: string;
    confirmPassword: string;
  }) => {
    if (values.password !== values.confirmPassword) {
      message.error('Mật khẩu xác nhận không khớp!');
      return;
    }

    setLoading(true);
    try {
      const response = await authService.register({
        username: values.username,
        fullName: values.fullName,
        email: values.email,
        phone: values.phone,
        password: values.password,
      });

      if (response.success) {
        message.success('Đăng ký thành công! Vui lòng đăng nhập.');
        navigate('/login');
      } else {
        message.error(response.error || 'Đăng ký thất bại, vui lòng thử lại');
      }
    } catch {
      message.error('Đã xảy ra lỗi, vui lòng thử lại');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={styles.page}>
      {/* ── Left panel ── */}
      <div className={styles.leftPanel}>
        <BubbleCanvas />

        <div className={styles.fishDeco1} aria-hidden>🐠</div>
        <div className={styles.fishDeco2} aria-hidden>🐟</div>
        <div className={styles.fishDeco3} aria-hidden>🐡</div>

        <div className={styles.brandBlock}>
          <div className={styles.brandLogo}>🐟</div>
          <h1 className={styles.brandName}>H&H Fish Shop</h1>
          <p className={styles.brandTagline}>H&H Fish Shop</p>
          <p className={styles.brandDesc}>
            Tham gia cộng đồng người yêu cá cảnh — nhận ưu đãi
            độc quyền và tư vấn chuyên nghiệp 24/7.
          </p>
          <div className={styles.features}>
            <div className={styles.featureItem}><span>✦</span> Đăng ký miễn phí</div>
            <div className={styles.featureItem}><span>✦</span> Ưu đãi thành viên hấp dẫn</div>
            <div className={styles.featureItem}><span>✦</span> Theo dõi đơn hàng dễ dàng</div>
          </div>
        </div>

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
          <div className={styles.mobileLogo} aria-hidden>🐟</div>

          <h2 className={styles.formTitle}>Đăng ký tài khoản</h2>
          <p className={styles.formSubtitle}>Tạo tài khoản để mua sắm dễ dàng hơn!</p>

          <Form
            layout="vertical"
            onFinish={handleRegister}
            autoComplete="off"
            className={styles.form}
          >
            <Form.Item
              name="username"
              label="Tên đăng nhập"
              rules={[
                { required: true, message: 'Vui lòng nhập tên đăng nhập' },
                { min: 4, message: 'Tên đăng nhập tối thiểu 4 ký tự' },
                { pattern: /^[a-zA-Z0-9_]+$/, message: 'Chỉ dùng chữ, số và dấu gạch dưới' },
              ]}
            >
              <Input
                size="large"
                prefix={<UserOutlined className={styles.inputIcon} />}
                placeholder="Nhập tên đăng nhập (vd: nguyenvan_a)"
                className={styles.input}
              />
            </Form.Item>

            <Form.Item
              name="fullName"
              label="Họ và tên"
              rules={[{ required: true, message: 'Vui lòng nhập họ tên' }]}
            >
              <Input
                size="large"
                prefix={<UserOutlined className={styles.inputIcon} />}
                placeholder="Nhập họ và tên"
                className={styles.input}
              />
            </Form.Item>

            <Form.Item
              name="email"
              label="Email"
              rules={[
                { required: true, message: 'Vui lòng nhập email' },
                { type: 'email', message: 'Email không đúng định dạng' },
              ]}
            >
              <Input
                size="large"
                prefix={<MailOutlined className={styles.inputIcon} />}
                placeholder="Nhập địa chỉ email"
                className={styles.input}
              />
            </Form.Item>

            <Form.Item
              name="phone"
              label="Số điện thoại"
              rules={[
                { required: true, message: 'Vui lòng nhập số điện thoại' },
                { pattern: /^[0-9]{9,11}$/, message: 'Số điện thoại không hợp lệ' },
              ]}
            >
              <Input
                size="large"
                prefix={<PhoneOutlined className={styles.inputIcon} />}
                placeholder="Nhập số điện thoại"
                className={styles.input}
              />
            </Form.Item>

            <Form.Item
              name="password"
              label="Mật khẩu"
              rules={[
                { required: true, message: 'Vui lòng nhập mật khẩu' },
                { min: 6, message: 'Mật khẩu phải có ít nhất 6 ký tự' },
              ]}
            >
              <Input.Password
                size="large"
                prefix={<LockOutlined className={styles.inputIcon} />}
                placeholder="Nhập mật khẩu (ít nhất 6 ký tự)"
                iconRender={(visible) =>
                  visible ? <EyeTwoTone twoToneColor="#0e7490" /> : <EyeInvisibleOutlined />
                }
                className={styles.input}
              />
            </Form.Item>

            <Form.Item
              name="confirmPassword"
              label="Xác nhận mật khẩu"
              dependencies={['password']}
              rules={[
                { required: true, message: 'Vui lòng xác nhận mật khẩu' },
                ({ getFieldValue }) => ({
                  validator(_, value) {
                    if (!value || getFieldValue('password') === value) {
                      return Promise.resolve();
                    }
                    return Promise.reject(new Error('Mật khẩu xác nhận không khớp!'));
                  },
                }),
              ]}
            >
              <Input.Password
                size="large"
                prefix={<LockOutlined className={styles.inputIcon} />}
                placeholder="Nhập lại mật khẩu"
                iconRender={(visible) =>
                  visible ? <EyeTwoTone twoToneColor="#0e7490" /> : <EyeInvisibleOutlined />
                }
                className={styles.input}
              />
            </Form.Item>

            <Form.Item style={{ marginTop: 24 }}>
              <Button
                type="primary"
                htmlType="submit"
                size="large"
                loading={loading}
                block
                className={styles.loginBtn}
              >
                {loading ? 'Đang đăng ký...' : 'Đăng ký ngay'}
              </Button>
            </Form.Item>
          </Form>

          <div className={styles.dividerRow}>
            <span className={styles.dividerLine} />
            <span className={styles.dividerText}>hoặc</span>
            <span className={styles.dividerLine} />
          </div>

          <p className={styles.registerPrompt}>
            Đã có tài khoản?{' '}
            <Link to="/login" className={styles.registerLink}>
              Đăng nhập →
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

export default Register;
