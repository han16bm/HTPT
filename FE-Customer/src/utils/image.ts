import type { SyntheticEvent } from 'react';

const SVG_PLACEHOLDER = `
<svg xmlns="http://www.w3.org/2000/svg" width="96" height="96" viewBox="0 0 96 96">
  <rect width="96" height="96" rx="12" fill="#f1f5f9"/>
  <path d="M29 64l12-15 10 12 7-8 9 11H29z" fill="#cbd5e1"/>
  <circle cx="39" cy="36" r="7" fill="#cbd5e1"/>
</svg>`;

export const IMAGE_PLACEHOLDER = `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(SVG_PLACEHOLDER)}`;

const PRODUCT_PREFIXES = ['products/', 'categories/'];
const CONTENT_PREFIXES = ['blogs/'];

function normalizeImageUrl(url?: string | null): string | undefined {
  const normalized = url?.trim().replace(/\\/g, '/');
  return normalized || undefined;
}

function isAbsoluteImageUrl(url: string): boolean {
  return /^https?:\/\//i.test(url) || url.startsWith('data:') || url.startsWith('blob:');
}

function resolveManagedUrl(url: string, prefixes: string[], requestPath: string): string | undefined {
  const withoutLeadingSlash = url.replace(/^\/+/, '');
  return prefixes.some((prefix) => withoutLeadingSlash.startsWith(prefix))
    ? `${requestPath}/${withoutLeadingSlash}`
    : undefined;
}

export function resolveImageUrl(
  url?: string | null,
  service: 'product' | 'content' = 'product',
): string {
  const normalized = normalizeImageUrl(url);
  if (!normalized) return IMAGE_PLACEHOLDER;

  if (isAbsoluteImageUrl(normalized) || normalized.startsWith('/api/')) {
    return normalized;
  }

  if (normalized.startsWith('api/')) {
    return `/${normalized}`;
  }

  const productUrl = resolveManagedUrl(normalized, PRODUCT_PREFIXES, '/api/product/assets');
  if (productUrl) return productUrl;

  const contentUrl = resolveManagedUrl(normalized, CONTENT_PREFIXES, '/api/content/assets');
  if (contentUrl) return contentUrl;

  if (normalized.startsWith('/assets/')) {
    return normalized;
  }

  if (service === 'content' && normalized.startsWith('content/')) {
    return `/api/content/assets/${normalized}`;
  }

  return IMAGE_PLACEHOLDER;
}

export function useImageFallback(event: SyntheticEvent<HTMLImageElement>): void {
  if (event.currentTarget.src !== IMAGE_PLACEHOLDER) {
    event.currentTarget.src = IMAGE_PLACEHOLDER;
  }
}
