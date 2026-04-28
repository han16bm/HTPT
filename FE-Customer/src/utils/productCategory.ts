import type { Category } from '@/api/categoryService';
import type { Product, ProductSortOption } from '@/api/productService';

export const getCategoryBranchIds = (
  categories: Category[],
  rootCategoryId?: number | null
): number[] => {
  if (!rootCategoryId) {
    return [];
  }

  const branchIds = new Set<number>();
  const queue = [rootCategoryId];

  while (queue.length > 0) {
    const currentId = queue.shift();

    if (!currentId || branchIds.has(currentId)) {
      continue;
    }

    branchIds.add(currentId);

    categories.forEach((category) => {
      if (category.parentId === currentId) {
        queue.push(category.id);
      }
    });
  }

  return [...branchIds];
};

export const dedupeProducts = (products: Product[]): Product[] => {
  const productMap = new Map<number, Product>();

  products.forEach((product) => {
    productMap.set(product.id, product);
  });

  return [...productMap.values()];
};

export const sortProducts = (
  products: Product[],
  sort: ProductSortOption | string = 'newest'
): Product[] => {
  const getCreatedAt = (product: Product) =>
    product.createdAt ? new Date(product.createdAt).getTime() : 0;

  return [...products].sort((left, right) => {
    switch (sort) {
      case 'price-asc':
        return left.salePrice - right.salePrice || getCreatedAt(right) - getCreatedAt(left);
      case 'price-desc':
        return right.salePrice - left.salePrice || getCreatedAt(right) - getCreatedAt(left);
      case 'best-seller':
        return right.soldQuantity - left.soldQuantity || getCreatedAt(right) - getCreatedAt(left);
      case 'name-asc':
        return left.name.localeCompare(right.name) || getCreatedAt(right) - getCreatedAt(left);
      case 'name-desc':
        return right.name.localeCompare(left.name) || getCreatedAt(right) - getCreatedAt(left);
      case 'newest':
      default:
        return getCreatedAt(right) - getCreatedAt(left);
    }
  });
};

export const paginateProducts = (
  products: Product[],
  page: number,
  pageSize: number
): Product[] => {
  const startIndex = (page - 1) * pageSize;
  return products.slice(startIndex, startIndex + pageSize);
};
