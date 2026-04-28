import { createRoot } from 'react-dom/client';
import { ConfigProvider } from 'antd';
import viVN from 'antd/locale/vi_VN';
import 'antd/dist/reset.css';
import antdTheme from './config/theme';
import './index.scss';
import App from './App.tsx';

createRoot(document.getElementById('root')!).render(
  <ConfigProvider theme={antdTheme} locale={viVN}>
    <App />
  </ConfigProvider>
);
