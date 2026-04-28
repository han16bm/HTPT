import React from 'react';
import { Spin } from 'antd';

const PageLoader: React.FC = () => (
  <div
    style={{
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      minHeight: '100vh',
      width: '100%',
    }}
  >
    <Spin size="large" tip="Đang tải..." />
  </div>
);

export default PageLoader;
