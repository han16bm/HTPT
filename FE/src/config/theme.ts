import type { ThemeConfig } from 'antd';

// Khớp design token trong Plan/05_FRONTEND_ADMIN_PLAN.md
const antdTheme: ThemeConfig = {
  token: {
    colorPrimary: '#198754',
    colorSuccess: '#198754',
    colorError: '#dc3545',
    colorWarning: '#ffc107',
    colorInfo: '#0dcaf0',
    borderRadius: 6,
    fontFamily:
      "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', sans-serif",
    fontSize: 14,
    colorBgLayout: '#f5f7fa',
  },
  components: {
    Layout: {
      siderBg: '#001529',
      headerBg: '#ffffff',
      bodyBg: '#f5f7fa',
    },
    Menu: {
      darkItemBg: '#001529',
      darkItemHoverBg: '#198754',
      darkItemSelectedBg: '#198754',
      darkSubMenuItemBg: '#000c17',
    },
    Table: {
      headerBg: '#f8f9fa',
      headerColor: '#212529',
      rowHoverBg: '#f1f5f9',
    },
    Button: {
      controlHeight: 36,
      fontWeight: 500,
    },
    Card: {
      borderRadiusLG: 8,
    },
  },
};

export default antdTheme;
