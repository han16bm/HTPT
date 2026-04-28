import { useCallback, useEffect, useState } from 'react';
import { message } from 'antd';
import type { PagedResult } from '@/interfaces';
import { DEFAULT_PAGE_SIZE } from '@/shared/utils/constants';

interface UsePagedQueryOptions<TParams> {
  fetcher: (params: TParams) => Promise<PagedResult<unknown>>;
  initialParams?: TParams;
  errorMessage?: string;
  immediate?: boolean;
}

interface UsePagedQueryResult<TItem, TParams> {
  items: TItem[];
  total: number;
  page: number;
  pageSize: number;
  loading: boolean;
  params: TParams;
  setParams: (next: Partial<TParams>) => void;
  refetch: () => Promise<void>;
}

/**
 * Hook tổng quát cho danh sách phân trang.
 * - Tự gọi API khi `params` đổi
 * - Tự catch lỗi và toast
 */
export function usePagedQuery<
  TItem,
  TParams extends { page?: number; pageSize?: number }
>({
  fetcher,
  initialParams = {} as TParams,
  errorMessage = 'Lỗi khi tải dữ liệu',
  immediate = true,
}: UsePagedQueryOptions<TParams>): UsePagedQueryResult<TItem, TParams> {
  const [items, setItems] = useState<TItem[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [params, setParamsState] = useState<TParams>({
    page: 1,
    pageSize: DEFAULT_PAGE_SIZE,
    ...initialParams,
  });

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const result = (await fetcher(params)) as PagedResult<TItem>;
      setItems(Array.isArray(result?.items) ? result.items : []);
      setTotal(result?.totalCount ?? 0);
    } catch (err) {
      const msg = (err as { message?: string })?.message || errorMessage;
      message.error(msg);
      setItems([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  }, [fetcher, params, errorMessage]);

  useEffect(() => {
    if (immediate) {
      fetchData();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [params]);

  const setParams = useCallback((next: Partial<TParams>) => {
    setParamsState((prev) => ({ ...prev, ...next }));
  }, []);

  return {
    items,
    total,
    page: params.page ?? 1,
    pageSize: params.pageSize ?? DEFAULT_PAGE_SIZE,
    loading,
    params,
    setParams,
    refetch: fetchData,
  };
}
