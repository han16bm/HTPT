const vndFormatter = new Intl.NumberFormat('vi-VN', {
  style: 'currency',
  currency: 'VND',
});

export const formatVND = (amount: number): string => vndFormatter.format(amount);
