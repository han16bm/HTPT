/**
 * Trả về một mảng từ giá trị bất kỳ.
 *
 * Lưu ý: tầng axiosClient đã unwrap `ApiResponse<T>` và `PagedResult<T>`
 * thành `T[]` cho list endpoints rồi. Helper này chỉ là lớp phòng vệ
 * cuối cùng phòng khi BE trả về format khác chuẩn — đảm bảo `.map`/`.length`
 * không bao giờ ném lỗi runtime.
 */
export function extractArray<T = unknown>(input: unknown): T[] {
  if (Array.isArray(input)) return input as T[];
  if (input && typeof input === 'object') {
    const obj = input as Record<string, unknown>;
    for (const key of [
      'items',
      'data',
      'list',
      'results',
      'records',
      'content',
    ]) {
      if (Array.isArray(obj[key])) return obj[key] as T[];
    }
  }
  return [];
}
