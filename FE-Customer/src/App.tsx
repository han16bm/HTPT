import React, { useState } from 'react';
import { ConfigProvider } from 'antd';
import viVN from 'antd/locale/vi_VN';
import AppRoutes from '@/routes/Routes';
import { LoadingScreen } from '@/components/LoadingScreen';
import { ErrorBoundary } from '@/components/ErrorBoundary';
import './App.scss';

const App: React.FC = () => {
  const [loading, setLoading] = useState(true);

  return (
    <ErrorBoundary>
      <ConfigProvider
        locale={viVN}
        theme={{
          token: {
            colorPrimary: '#1890ff',
            borderRadius: 6,
          },
        }}
      >
        {/* LoadingScreen overlays the app on first visit; AppRoutes preloads underneath */}
        {loading && <LoadingScreen onFinish={() => setLoading(false)} />}
        <AppRoutes />
      </ConfigProvider>
    </ErrorBoundary>
  );
};

export default App;
