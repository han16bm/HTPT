export { authService } from './user/authService';
export { customerService } from './user/customerService';
export { productService } from './product/productService';
export { categoryService } from './product/categoryService';
export { inventoryService } from './product/inventoryService';
export { orderService } from './order/orderService';
export { dashboardService } from './order/dashboardService';
export { promotionService } from './order/promotionService';
export { reportService } from './order/reportService';
export { blogService } from './content/blogService';
export { contactService } from './content/contactService';

export type { CustomerSearchParams } from './user/customerService';
export type { InventoryHistoryParams } from './product/inventoryService';
export type { PromotionSearchParams } from './order/promotionService';
export type { BlogSearchParams } from './content/blogService';
export type { ContactSearchParams } from './content/contactService';
export type { DashboardOverview } from './order/dashboardService';
export type { RevenueReportParams } from './order/reportService';
