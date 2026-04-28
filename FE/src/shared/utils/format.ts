// ============================================
// Hàm format dùng chung
// ============================================
import dayjs from 'dayjs';
import { DATE_FORMAT, DATETIME_FORMAT } from './constants';

export const formatCurrency = (value?: number | null): string => {
  const n = typeof value === 'number' && Number.isFinite(value) ? value : 0;
  return `${n.toLocaleString('vi-VN')}đ`;
};

export const formatNumber = (value?: number | null): string => {
  const n = typeof value === 'number' && Number.isFinite(value) ? value : 0;
  return n.toLocaleString('vi-VN');
};

export const formatDate = (value?: string | Date | null): string => {
  if (!value) return '—';
  const d = dayjs(value);
  return d.isValid() ? d.format(DATE_FORMAT) : '—';
};

export const formatDateTime = (value?: string | Date | null): string => {
  if (!value) return '—';
  const d = dayjs(value);
  return d.isValid() ? d.format(DATETIME_FORMAT) : '—';
};

export const formatPercent = (value?: number | null): string => {
  const n = typeof value === 'number' && Number.isFinite(value) ? value : 0;
  return `${n}%`;
};

const compactFormatter = new Intl.NumberFormat('vi-VN', { notation: 'compact' });

export const formatCompact = (value: number): string => compactFormatter.format(value);
