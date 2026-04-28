import React, { useState } from 'react';
import {
  Button, Drawer, Select, Space, Table, Tag, Typography, message,
  Input,
} from 'antd';
import { MailOutlined, SearchOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import classNames from 'classnames/bind';
import styles from './Contacts.module.scss';
import { MasterLayout } from '@/components/Layout';
import { contactService } from '@/api';
import { usePagedQuery } from '@/hooks';
import { formatDate } from '@/shared/utils/format';
import { CONTACT_STATUS_COLOR, CONTACT_STATUS_LABEL } from '@/shared/utils/constants';
import type { Contact, ContactStatus } from '@/interfaces';
import type { ContactSearchParams } from '@/api';

const cx = classNames.bind(styles);
const { Title, Text } = Typography;

const STATUS_FLOW: Record<ContactStatus, ContactStatus | null> = {
  NEW: 'READ',
  READ: 'RESOLVED',
  RESOLVED: null,
};

const STATUS_BTN_LABEL: Partial<Record<ContactStatus, string>> = {
  NEW: 'Đánh dấu đã đọc',
  READ: 'Đánh dấu đã xử lý',
};

const Contacts: React.FC = () => {
  const [keyword, setKeyword] = useState('');
  const [filterStatus, setFilterStatus] = useState<ContactStatus | undefined>();
  const [detailContact, setDetailContact] = useState<Contact | null>(null);

  const {
    items, total, page, pageSize, loading, setParams, refetch,
  } = usePagedQuery<Contact, ContactSearchParams>({
    fetcher: (p) => contactService.search(p),
    initialParams: { page: 1, pageSize: 10 },
    errorMessage: 'Lỗi khi tải liên hệ',
  });

  const handleUpdateStatus = async (contact: Contact, newStatus: ContactStatus) => {
    try {
      await contactService.updateStatus(contact.id, newStatus);
      message.success('Cập nhật trạng thái thành công');
      refetch();
      // Update drawer if open
      if (detailContact?.id === contact.id) {
        setDetailContact({ ...contact, status: newStatus });
      }
    } catch (err) {
      message.error((err as { message?: string }).message || 'Lỗi khi cập nhật');
    }
  };

  const columns: ColumnsType<Contact> = [
    {
      title: 'Họ tên',
      dataIndex: 'fullName',
      key: 'fullName',
      render: (name: string) => <span style={{ fontWeight: 500 }}>{name}</span>,
    },
    {
      title: 'Email',
      dataIndex: 'email',
      key: 'email',
      width: 200,
    },
    {
      title: 'SĐT',
      dataIndex: 'phone',
      key: 'phone',
      width: 130,
    },
    {
      title: 'Chủ đề',
      dataIndex: 'subject',
      key: 'subject',
      width: 200,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: 130,
      render: (s: ContactStatus) => <Tag color={CONTACT_STATUS_COLOR[s]}>{CONTACT_STATUS_LABEL[s]}</Tag>,
    },
    {
      title: 'Thời gian',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 140,
      render: (v: string) => formatDate(v),
    },
    {
      title: 'Hành động',
      key: 'action',
      width: 160,
      fixed: 'right',
      render: (_, record) => {
        const nextStatus = STATUS_FLOW[record.status];
        return (
          <Space size="small">
            <Button size="small" onClick={() => setDetailContact(record)}>
              Xem
            </Button>
            {nextStatus && (
              <Button
                size="small"
                type="primary"
                ghost
                onClick={() => handleUpdateStatus(record, nextStatus)}
              >
                {STATUS_BTN_LABEL[record.status]}
              </Button>
            )}
          </Space>
        );
      },
    },
  ];

  return (
    <MasterLayout>
      <div className={cx('page')}>
        <div className={cx('pageHeader')}>
          <Title level={3}><MailOutlined /> Liên hệ khách hàng</Title>
        </div>

        {/* Filter bar */}
        <div className={cx('filterBar')}>
          <Input
            placeholder="Tìm theo tên / email / SĐT..."
            prefix={<SearchOutlined />}
            value={keyword}
            onChange={(e) => setKeyword(e.target.value)}
            onPressEnter={() => setParams({ keyword, page: 1 })}
            allowClear
            onClear={() => { setKeyword(''); setParams({ keyword: undefined, page: 1 }); }}
            style={{ width: 280 }}
          />
          <Select
            placeholder="Trạng thái"
            value={filterStatus}
            onChange={(v) => { setFilterStatus(v); setParams({ status: v, page: 1 }); }}
            allowClear
            style={{ width: 160 }}
            options={Object.entries(CONTACT_STATUS_LABEL).map(([k, v]) => ({ value: k, label: v }))}
          />
          <Button onClick={() => { setKeyword(''); setFilterStatus(undefined); setParams({ page: 1 }); }}>
            Đặt lại
          </Button>
        </div>

        <Table<Contact>
          columns={columns}
          dataSource={items}
          rowKey="id"
          loading={loading}
          scroll={{ x: 1000 }}
          rowClassName={(r) => r.status === 'NEW' ? cx('rowNew') : ''}
          pagination={{
            current: page,
            pageSize,
            total,
            showSizeChanger: true,
            showTotal: (t) => `Tổng ${t} liên hệ`,
            onChange: (p, ps) => setParams({ page: p, pageSize: ps }),
          }}
        />

        {/* Detail drawer */}
        <Drawer
          title={`Liên hệ — ${detailContact?.fullName}`}
          open={!!detailContact}
          onClose={() => setDetailContact(null)}
          width={520}
          extra={
            detailContact && STATUS_FLOW[detailContact.status] ? (
              <Button
                type="primary"
                onClick={() => handleUpdateStatus(detailContact, STATUS_FLOW[detailContact.status]!)}
              >
                {STATUS_BTN_LABEL[detailContact.status]}
              </Button>
            ) : null
          }
        >
          {detailContact && (
            <div className={cx('detail')}>
              <div className={cx('detailRow')}>
                <span>Họ tên:</span>
                <strong>{detailContact.fullName}</strong>
              </div>
              <div className={cx('detailRow')}>
                <span>Email:</span>
                <span>{detailContact.email}</span>
              </div>
              <div className={cx('detailRow')}>
                <span>Điện thoại:</span>
                <span>{detailContact.phone}</span>
              </div>
              <div className={cx('detailRow')}>
                <span>Chủ đề:</span>
                <span>{detailContact.subject}</span>
              </div>
              <div className={cx('detailRow')}>
                <span>Trạng thái:</span>
                <Tag color={CONTACT_STATUS_COLOR[detailContact.status]}>
                  {CONTACT_STATUS_LABEL[detailContact.status]}
                </Tag>
              </div>
              <div className={cx('detailRow')}>
                <span>Ngày gửi:</span>
                <span>{formatDate(detailContact.createdAt)}</span>
              </div>
              <div className={cx('messageBox')}>
                <div className={cx('messageLabel')}>Nội dung:</div>
                <div className={cx('messageContent')}>
                  <Text>{detailContact.message}</Text>
                </div>
              </div>
            </div>
          )}
        </Drawer>
      </div>
    </MasterLayout>
  );
};

export default Contacts;
