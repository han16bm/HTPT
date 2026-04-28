import React, { useState } from 'react';
import { Row, Col, Form, Input, Button, message } from 'antd';
import {
  PhoneOutlined,
  MailOutlined,
  EnvironmentOutlined,
  ClockCircleOutlined,
  MessageOutlined,
  SendOutlined,
  FacebookOutlined,
  YoutubeOutlined,
  InstagramOutlined,
} from '@ant-design/icons';
import { CustomerLayout } from '@/components/Layout';
import styles from './Contact.module.scss';

const { TextArea } = Input;

const Contact: React.FC = () => {
  const [form] = Form.useForm();
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (_values: Record<string, string>) => {
    setSubmitting(true);
    await new Promise((r) => setTimeout(r, 1000));
    message.success('Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi trong vòng 24 giờ.');
    form.resetFields();
    setSubmitting(false);
  };

  const contactCards = [
    {
      icon: <EnvironmentOutlined />,
      label: 'Địa Chỉ',
      color: '#fa8c16',
      lines: ['123 Đường Nguyễn Trãi, Hà Đông Hà Nội', '122 Đường Trần Phú, Hà Đông, Hà Nội'],
    },
    {
      icon: <PhoneOutlined />,
      label: 'Điện Thoại',
      color: '#1890ff',
      lines: ['0853997698  (8:00 – 22:00)', '0909 123 456  (Zalo)'],
    },
    {
      icon: <MailOutlined />,
      label: 'Email',
      color: '#52c41a',
      lines: ['hhfishop@gmail.com', 'supporthhfishshop@gmail.com'],
    },
    {
      icon: <ClockCircleOutlined />,
      label: 'Giờ Làm Việc',
      color: '#722ed1',
      lines: ['Thứ 2 – Thứ 6: 8:00 – 21:00', 'Thứ 7 – Chủ nhật: 8:00 – 22:00'],
    },
  ];

  const quickContacts = [
    { icon: <PhoneOutlined />,   label: 'Gọi Điện',    color: '#1890ff', bg: 'rgba(24,144,255,0.14)',  href: 'tel:19002097' },
    { icon: <MessageOutlined />, label: 'Chat Zalo',   color: '#00b47e', bg: 'rgba(0,180,126,0.14)',   href: 'https://zalo.me/0909123456' },
    { icon: <FacebookOutlined />,label: 'Messenger',   color: '#0084ff', bg: 'rgba(0,132,255,0.14)',   href: 'https://m.me/hhfishshop' },
  ];

  return (
    <CustomerLayout>
      <div className={styles.contactPage}>

        {/* ── 1. Hero Banner ── */}
        <div className={styles.heroBanner}>
          <div className={styles.bannerOverlay} />
          <div className={styles.bannerContent}>
            <div className={styles.bannerEmoji}>📬</div>
            <h1 className={styles.bannerTitle}>Liên Hệ Với Chúng Tôi</h1>
            <p className={styles.bannerSub}>
              Chúng tôi luôn sẵn sàng hỗ trợ và tư vấn cho bạn về cá cảnh và hồ cá.
            </p>
          </div>
        </div>

        {/* ── 2. Contact Info Cards ── */}
        <section className={styles.section}>
          <Row gutter={[20, 20]}>
            {contactCards.map((c, i) => (
              <Col xs={24} sm={12} md={6} key={i}>
                <div className={styles.infoCard}>
                  <div className={styles.infoIcon} style={{ color: c.color, background: c.color + '1a' }}>
                    {c.icon}
                  </div>
                  <h3 className={styles.infoLabel}>{c.label}</h3>
                  {c.lines.map((l, j) => (
                    <p key={j} className={styles.infoLine}>{l}</p>
                  ))}
                </div>
              </Col>
            ))}
          </Row>
        </section>

        {/* ── 3. Form + Map ── */}
        <section className={styles.section}>
          <Row gutter={[32, 32]}>

            {/* Form */}
            <Col xs={24} lg={13}>
              <div className={styles.formCard}>
                <h2 className={styles.formTitle}>
                  <SendOutlined className={styles.formTitleIcon} /> Gửi Tin Nhắn Cho Chúng Tôi
                </h2>
                <Form form={form} layout="vertical" onFinish={handleSubmit} requiredMark={false}>
                  <Row gutter={[16, 0]}>
                    <Col xs={24} sm={12}>
                      <Form.Item
                        label={<span className={styles.fieldLabel}>Họ và Tên</span>}
                        name="name"
                        rules={[{ required: true, message: 'Vui lòng nhập họ tên' }]}
                      >
                        <Input size="large" placeholder="Nguyễn Văn A" className={styles.darkInput} />
                      </Form.Item>
                    </Col>
                    <Col xs={24} sm={12}>
                      <Form.Item
                        label={<span className={styles.fieldLabel}>Số Điện Thoại</span>}
                        name="phone"
                        rules={[
                          { required: true, message: 'Vui lòng nhập số điện thoại' },
                          { pattern: /^[0-9]{10}$/, message: 'Số điện thoại không hợp lệ' },
                        ]}
                      >
                        <Input size="large" placeholder="0909 xxx xxx" className={styles.darkInput} />
                      </Form.Item>
                    </Col>
                  </Row>

                  <Form.Item
                    label={<span className={styles.fieldLabel}>Email</span>}
                    name="email"
                    rules={[{ type: 'email', message: 'Email không hợp lệ' }]}
                  >
                    <Input size="large" placeholder="email@example.com" className={styles.darkInput} />
                  </Form.Item>

                  <Form.Item
                    label={<span className={styles.fieldLabel}>Nội Dung Tin Nhắn</span>}
                    name="message"
                    rules={[{ required: true, message: 'Vui lòng nhập nội dung' }]}
                  >
                    <TextArea
                      rows={5}
                      placeholder="Nhập nội dung bạn muốn liên hệ..."
                      size="large"
                      className={styles.darkInput}
                    />
                  </Form.Item>

                  <Form.Item style={{ marginBottom: 0 }}>
                    <Button
                      type="primary"
                      htmlType="submit"
                      size="large"
                      icon={<SendOutlined />}
                      loading={submitting}
                      block
                      className={styles.submitBtn}
                    >
                      Gửi Tin Nhắn
                    </Button>
                  </Form.Item>
                </Form>
              </div>
            </Col>

            {/* Map + Social */}
            <Col xs={24} lg={11}>
              <div className={styles.mapCard}>
                <h2 className={styles.mapTitle}>
                  <EnvironmentOutlined className={styles.mapTitleIcon} /> Tìm Chúng Tôi
                </h2>
                <div className={styles.mapEmbed}>
                  <iframe
                    title="H&H Fish Shop location"
                    src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3919.4464102741574!2d106.66808097486872!3d10.77588905931208!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x31752f1f89c64bdb%3A0xa1b3451bd979e5f1!2zMTIzIE5ndXnhu4VuIFRyw6NpLCBQaMaw4budbmcgQmVuIFRo4burYywgUXXhuq1uIDEsIFRow6BuaCBwaOG7kSBI4buTIENow60gTWluaA!5e0!3m2!1svi!2svn!4v1710000000000!5m2!1svi!2svn"
                    width="100%"
                    height="280"
                    style={{ border: 0, borderRadius: '10px' }}
                    loading="lazy"
                    referrerPolicy="no-referrer-when-downgrade"
                  />
                </div>
                <p className={styles.mapAddress}>
                  <EnvironmentOutlined /> 123 Đường Nguyễn Trãi, Quận 1, TP. Hồ Chí Minh
                </p>
              </div>

              {/* Social */}
              <div className={styles.socialCard}>
                <h3 className={styles.socialTitle}>Theo Dõi Chúng Tôi</h3>
                <div className={styles.socialRow}>
                  <a href="https://facebook.com" target="_blank" rel="noreferrer" className={styles.socialBtn} style={{ background: 'rgba(9,80,154,0.70)', borderColor: 'rgba(9,80,154,0.50)' }}>
                    <FacebookOutlined /> Facebook
                  </a>
                  <a href="https://instagram.com" target="_blank" rel="noreferrer" className={styles.socialBtn} style={{ background: 'rgba(131,58,180,0.55)', borderColor: 'rgba(131,58,180,0.45)' }}>
                    <InstagramOutlined /> Instagram
                  </a>
                  <a href="https://youtube.com" target="_blank" rel="noreferrer" className={styles.socialBtn} style={{ background: 'rgba(180,0,0,0.55)', borderColor: 'rgba(180,0,0,0.45)' }}>
                    <YoutubeOutlined /> YouTube
                  </a>
                </div>
              </div>
            </Col>
          </Row>
        </section>

        {/* ── 4. Quick Contact ── */}
        <section className={styles.quickSection}>
          <div className={styles.quickGlow} />
          <div className={styles.quickContent}>
            <h2 className={styles.quickTitle}>Liên Hệ Nhanh</h2>
            <p className={styles.quickSub}>Chọn kênh liên lạc phù hợp nhất với bạn</p>
            <div className={styles.quickRow}>
              {quickContacts.map((q, i) => (
                <a key={i} href={q.href} target="_blank" rel="noreferrer" className={styles.quickBtn}
                  style={{ '--qcolor': q.color, '--qbg': q.bg } as React.CSSProperties}>
                  <span className={styles.quickIcon}>{q.icon}</span>
                  <span className={styles.quickLabel}>{q.label}</span>
                </a>
              ))}
            </div>
          </div>
        </section>

      </div>
    </CustomerLayout>
  );
};

export default Contact;
