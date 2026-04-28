import React, { useState } from 'react';
import { Layout } from 'antd';
import classNames from 'classnames/bind';
import Header from './Header/Header';
import Sidebar from './Sidebar/Sidebar';
import styles from './MasterLayout.module.scss';

const cx = classNames.bind(styles);
const { Content } = Layout;

interface MasterLayoutProps {
  children: React.ReactNode;
}

const MasterLayout: React.FC<MasterLayoutProps> = ({ children }) => {
  const [collapsed, setCollapsed] = useState(false);
  const siderWidth = collapsed ? 80 : 250;

  return (
    <Layout className={cx('layout')}>
      <Sidebar collapsed={collapsed} />
      <Layout className={cx('mainLayout')} style={{ marginLeft: siderWidth }}>
        <Header collapsed={collapsed} onToggle={() => setCollapsed((c) => !c)} />
        <Content className={cx('content')}>{children}</Content>
      </Layout>
    </Layout>
  );
};

export default MasterLayout;
