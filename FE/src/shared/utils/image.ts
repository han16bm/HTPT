const SVG_PLACEHOLDER = `
<svg xmlns="http://www.w3.org/2000/svg" width="96" height="96" viewBox="0 0 96 96">
  <rect width="96" height="96" rx="12" fill="#f1f5f9"/>
  <path d="M29 64l12-15 10 12 7-8 9 11H29z" fill="#cbd5e1"/>
  <circle cx="39" cy="36" r="7" fill="#cbd5e1"/>
</svg>`;

export const IMAGE_PLACEHOLDER = `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(SVG_PLACEHOLDER)}`;

const MANAGED_PRODUCT_PREFIXES = ['products/', 'categories/'];
const MANAGED_CONTENT_PREFIXES = ['blogs/'];

function normalizeUrl(url?: string | null): string | undefined {
  if (!url) return undefined;

  const normalized = url.trim();
  if (!normalized) return undefined;

  return normalized.replace(/\\/g, '/');
}

function isAbsoluteUrl(url: string): boolean {
  return /^https?:\/\//i.test(url) || url.startsWith('data:') || url.startsWith('blob:');
}

function resolveManagedUrl(url: string, prefixes: string[], requestPath: string): string | undefined {
  const withoutLeadingSlash = url.replace(/^\/+/, '');
  const matchedPrefix = prefixes.find((prefix) => withoutLeadingSlash.startsWith(prefix));

  if (!matchedPrefix) {
    return undefined;
  }

  return `${requestPath}/${withoutLeadingSlash}`;
}

export function resolveAssetImageUrl(
  url?: string | null,
  service: 'product' | 'content' = 'product',
): string | undefined {
  const normalized = normalizeUrl(url);
  if (!normalized) return undefined;

  if (isAbsoluteUrl(normalized) || normalized.startsWith('/api/')) {
    return normalized;
  }

  if (normalized.startsWith('api/')) {
    return `/${normalized}`;
  }

  const productUrl = resolveManagedUrl(normalized, MANAGED_PRODUCT_PREFIXES, '/api/product/assets');
  if (productUrl) return productUrl;

  const contentUrl = resolveManagedUrl(normalized, MANAGED_CONTENT_PREFIXES, '/api/content/assets');
  if (contentUrl) return contentUrl;

  if (normalized.startsWith('/assets/')) {
    return normalized;
  }

  if (service === 'content' && normalized.startsWith('content/')) {
    return `/api/content/assets/${normalized}`;
  }

  return undefined;
}

export const resolveProductImageUrl = (url?: string | null): string | undefined =>
  resolveAssetImageUrl(url, 'product');

export const resolveContentImageUrl = (url?: string | null): string | undefined =>
  resolveAssetImageUrl(url, 'content');
