const SVG_PLACEHOLDER = `
<svg xmlns="http://www.w3.org/2000/svg" width="96" height="96" viewBox="0 0 96 96">
  <rect width="96" height="96" rx="12" fill="#f1f5f9"/>
  <path d="M29 64l12-15 10 12 7-8 9 11H29z" fill="#cbd5e1"/>
  <circle cx="39" cy="36" r="7" fill="#cbd5e1"/>
</svg>`;

export const IMAGE_PLACEHOLDER = `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(SVG_PLACEHOLDER)}`;

export function resolveProductImageUrl(url?: string | null): string | undefined {
  if (!url) return undefined;

  const normalized = url.trim();
  if (!normalized) return undefined;

  if (
    normalized.includes('minio.hoctuthien.com') ||
    normalized.includes('/fishop/') ||
    normalized.startsWith('products/') ||
    normalized.startsWith('/products/')
  ) {
    return undefined;
  }

  return normalized;
}
