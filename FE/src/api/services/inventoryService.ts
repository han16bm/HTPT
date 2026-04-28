import axiosClient from '../axiosClient';
import { API_ENDPOINTS } from '../constants';
import type {
  InventoryImportRequest,
  InventoryTransaction,
  InventoryTransactionType,
  PagedResult,
} from '@/interfaces';

export interface InventoryHistoryParams {
  page?: number;
  pageSize?: number;
  productId?: number;
  transactionType?: InventoryTransactionType;
}

export const inventoryService = {
  getHistory: (params: InventoryHistoryParams = {}) =>
    axiosClient.get<PagedResult<InventoryTransaction>>(
      API_ENDPOINTS.INVENTORY_HISTORY,
      { params }
    ),

  importStock: (data: InventoryImportRequest) =>
    axiosClient.post<InventoryTransaction>(
      API_ENDPOINTS.INVENTORY_IMPORT,
      data
    ),
};
