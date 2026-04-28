import React from 'react';

interface Props {
  children: React.ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

class ErrorBoundary extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, info: React.ErrorInfo) {
    console.error('[ErrorBoundary]', error, info);
  }

  handleReload = () => {
    this.setState({ hasError: false, error: undefined });
    window.location.href = '/';
  };

  render() {
    if (this.state.hasError) {
      return (
        <div style={{ padding: 32, textAlign: 'center' }}>
          <h2>Đã xảy ra lỗi khi hiển thị trang</h2>
          <p style={{ color: '#888' }}>{this.state.error?.message}</p>
          <button onClick={this.handleReload} style={{ padding: '8px 16px', cursor: 'pointer' }}>
            Tải lại trang chủ
          </button>
        </div>
      );
    }
    return this.props.children;
  }
}

export default ErrorBoundary;
