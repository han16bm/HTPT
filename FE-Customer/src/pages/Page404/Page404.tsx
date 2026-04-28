import React from 'react';
import { Result, Button } from 'antd';
import { useNavigate } from 'react-router-dom';
import { CustomerLayout } from '@/components/Layout';

const Page404: React.FC = () => {
  const navigate = useNavigate();
  return (
    <CustomerLayout>
      <div style={{ padding: '60px 0' }}>
        <Result
          status="404"
          title="404"
          subTitle="Trang bạn tìm kiếm không tồn tại."
          extra={
            <Button type="primary" onClick={() => navigate('/')}>
              Về trang chủ
            </Button>
          }
        />
      </div>
    </CustomerLayout>
  );
};

export default Page404;
