export const FREE_SHIP_THRESHOLD = 500_000;
export const SHIPPING_FEE = 30_000;

export const calcShipping = (subtotal: number): number =>
  subtotal >= FREE_SHIP_THRESHOLD ? 0 : subtotal > 0 ? SHIPPING_FEE : 0;
