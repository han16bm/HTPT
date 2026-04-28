import { useCallback, useEffect, useState } from 'react';
import { message } from 'antd';

interface UseAsyncOptions<T> {
  fetcher: () => Promise<T>;
  immediate?: boolean;
  errorMessage?: string;
  onSuccess?: (data: T) => void;
}

interface UseAsyncResult<T> {
  data: T | undefined;
  loading: boolean;
  error: Error | null;
  refetch: () => Promise<void>;
  setData: (data: T | undefined) => void;
}

/**
 * Hook tổng quát cho 1 lệnh fetch không phân trang.
 */
export function useAsync<T>({
  fetcher,
  immediate = true,
  errorMessage,
  onSuccess,
}: UseAsyncOptions<T>): UseAsyncResult<T> {
  const [data, setData] = useState<T | undefined>(undefined);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const refetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await fetcher();
      setData(result);
      onSuccess?.(result);
    } catch (err) {
      const e = err as Error;
      setError(e);
      if (errorMessage) message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  }, [fetcher, errorMessage, onSuccess]);

  useEffect(() => {
    if (immediate) {
      refetch();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return { data, loading, error, refetch, setData };
}
