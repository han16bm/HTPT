# 🛠️ FE-Customer Developer Guide - Hướng Dẫn Tiếp Tục Phát Triển

## 📚 Mục Lục
1. [Setup & Installation](#setup--installation)
2. [Project Structure](#project-structure)
3. [Component Architecture](#component-architecture)
4. [Styling Guidelines](#styling-guidelines)
5. [Adding New Features](#adding-new-features)
6. [Troubleshooting](#troubleshooting)

---

## Setup & Installation

### Prerequisites
```bash
Node.js >= 18.x
npm >= 9.x
```

### Initial Setup
```bash
cd FE-Customer
npm install
npm run dev    # Start dev server at http://localhost:5173
npm run build  # Build for production
npm run preview # Preview production build
```

### Environment Setup (if needed)
```bash
# Create .env.local (not tracked by git)
VITE_API_URL=http://localhost:3000/api
VITE_APP_NAME=Cá Cảnh Shop
```

---

## Project Structure

```
FE-Customer/
├── src/
│   ├── api/                 # API services
│   │   ├── authService.ts
│   │   ├── productService.ts
│   │   ├── cartService.ts
│   │   ├── orderService.ts
│   │   ├── axiosClient.ts
│   │   └── constants.ts
│   │
│   ├── components/          # Reusable components
│   │   ├── Layout/
│   │   │   ├── CustomerLayout.tsx          ⭐ Main layout
│   │   │   ├── CustomerLayout.module.scss
│   │   │   └── index.ts
│   │   │
│   │   ├── ProductCard/
│   │   │   ├── ProductCard.tsx              ⭐ Product display
│   │   │   ├── ProductCard.module.scss
│   │   │   └── index.ts
│   │   │
│   │   └── common/
│   │       └── ProtectedRoute.tsx
│   │
│   ├── pages/               # Page components
│   │   ├── Home/
│   │   │   ├── Home.tsx                    ⭐ Homepage
│   │   │   └── Home.module.scss
│   │   │
│   │   ├── Products/
│   │   │   ├── Products.tsx                ⭐ Product listing
│   │   │   └── Products.module.scss
│   │   │
│   │   ├── Cart/
│   │   │   ├── Cart.tsx
│   │   │   └── Cart.module.scss
│   │   │
│   │   ├── Checkout/
│   │   │   ├── Checkout.tsx
│   │   │   └── Checkout.module.scss
│   │   │
│   │   ├── Login/
│   │   │   ├── Login.tsx
│   │   │   └── Login.module.scss
│   │   │
│   │   ├── MyOrders/
│   │   │   ├── MyOrders.tsx
│   │   │   └── MyOrders.module.scss
│   │   │
│   │   └── Error/
│   │       └── Page404.tsx
│   │
│   ├── routes/
│   │   └── Routes.tsx       # Route definitions
│   │
│   ├── config/
│   │   ├── env.ts
│   │   ├── theme.ts         # Ant Design theme
│   │   └── index.ts
│   │
│   ├── interfaces/
│   │   └── index.ts         # TypeScript interfaces
│   │
│   ├── shared/
│   │   └── utils/
│   │       ├── constants.ts
│   │       └── index.ts
│   │
│   ├── App.tsx
│   ├── App.scss
│   ├── main.tsx
│   └── index.scss
│
├── public/
│   └── assets/
│       └── images/          # Static images
│
├── package.json
├── tsconfig.json
├── vite.config.ts
├── eslint.config.js
└── README.md
```

---

## Component Architecture

### 1. CustomerLayout (Main Wrapper)

**Usage:**
```tsx
import { CustomerLayout } from '@/components/Layout';

export default function MyPage() {
  return (
    <CustomerLayout>
      <div>Page content here</div>
    </CustomerLayout>
  );
}
```

**Features:**
- Top bar with contact info
- Sticky header with search
- Navigation bar
- Footer with policy links
- Mobile responsive

**Props:** None (children via composition)

### 2. ProductCard (Reusable Component)

**Usage:**
```tsx
import { ProductCard } from '@/components/ProductCard';

<ProductCard 
  product={productObject}
  onAddToCart={(product) => handleAddToCart(product)}
/>
```

**Props:**
```typescript
interface ProductCardProps {
  product: Product;
  onAddToCart?: (product: Product) => void;
}
```

**Features:**
- Responsive image display
- Badges (discount, out of stock)
- Rating display
- Favorite toggle
- Price comparison
- Stock status
- Quick add button

---

## Styling Guidelines

### SCSS Naming Convention

Use **BEM (Block Element Modifier)** pattern:

```scss
// Block - standalone component
.productCard {
  // Element - part of block
  &__imageWrapper {
    // Modifier - variant
    &--loading {
      opacity: 0.5;
    }
  }
}

// Ant Design component overrides
:global(.ant-button) {
  // Custom styles
}
```

### Responsive Breakpoints

```scss
// Define reusable mixins
@mixin mobile {
  @media (max-width: 576px) { @content; }
}

@mixin tablet {
  @media (max-width: 768px) { @content; }
}

@mixin desktop {
  @media (min-width: 1024px) { @content; }
}

// Usage
.productCard {
  font-size: 14px;
  
  @include tablet {
    font-size: 13px;
  }
  
  @include mobile {
    font-size: 12px;
  }
}
```

### Color Variables

```scss
$primary-blue: #1890ff;
$danger-red: #ff4d4f;
$success-green: #52c41a;
$warning-orange: #faad14;
$dark-text: #1f1f1f;
$gray-text: #666;
$light-bg: #f5f5f5;
$border-color: #f0f0f0;
$white: #fff;
```

### Spacing System

```scss
$spacing-xs: 4px;    // Micro spacing
$spacing-sm: 8px;    // Small elements
$spacing-md: 12px;   // Mobile padding
$spacing-lg: 16px;   // Tablet padding
$spacing-xl: 24px;   // Desktop padding
$spacing-2xl: 32px;  // Section margins
$spacing-3xl: 48px;  // Large sections
```

### Using CSS Modules

```tsx
// Component file
import styles from './MyComponent.module.scss';

export default function MyComponent() {
  return <div className={styles.container}>...</div>;
}

// SCSS Module (local scoping)
.container {
  padding: 24px;
}

.title {
  font-size: 24px;
}

// Override Ant Design (global scope needed)
:global(.ant-button) {
  border-radius: 4px;
}
```

---

## Adding New Features

### 1. Creating a New Page

**File structure:**
```
src/pages/NewPage/
├── NewPage.tsx
├── NewPage.module.scss
└── index.ts
```

**Template:**
```tsx
// NewPage.tsx
import React, { useState, useEffect } from 'react';
import { CustomerLayout } from '@/components/Layout';
import styles from './NewPage.module.scss';

const NewPage: React.FC = () => {
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    // Load data on mount
  }, []);

  return (
    <CustomerLayout>
      <div className={styles.page}>
        {/* Page content */}
      </div>
    </CustomerLayout>
  );
};

export default NewPage;
```

```scss
// NewPage.module.scss
.page {
  padding: 24px;
  
  @media (max-width: 768px) {
    padding: 16px;
  }
  
  @media (max-width: 576px) {
    padding: 12px;
  }
}
```

### 2. Creating a New Component

**File structure:**
```
src/components/NewComponent/
├── NewComponent.tsx
├── NewComponent.module.scss
└── index.ts
```

**Template:**
```tsx
// NewComponent.tsx
import React from 'react';
import styles from './NewComponent.module.scss';

interface NewComponentProps {
  title: string;
  children?: React.ReactNode;
}

const NewComponent: React.FC<NewComponentProps> = ({ title, children }) => {
  return (
    <div className={styles.component}>
      <h2 className={styles.title}>{title}</h2>
      {children}
    </div>
  );
};

export default NewComponent;
```

```tsx
// index.ts (for cleaner imports)
export { default } from './NewComponent';
export type { default as NewComponent } from './NewComponent';
```

### 3. Adding API Integration

**In apiService.ts:**
```typescript
export const newService = {
  // GET request
  getAll: async (params: any) => {
    const response = await axiosClient.get('/endpoint', { params });
    return response.data;
  },

  // GET by ID
  getById: async (id: string) => {
    const response = await axiosClient.get(`/endpoint/${id}`);
    return response.data;
  },

  // POST request
  create: async (data: any) => {
    const response = await axiosClient.post('/endpoint', data);
    return response.data;
  },

  // PUT request
  update: async (id: string, data: any) => {
    const response = await axiosClient.put(`/endpoint/${id}`, data);
    return response.data;
  },

  // DELETE request
  delete: async (id: string) => {
    const response = await axiosClient.delete(`/endpoint/${id}`);
    return response.data;
  },
};
```

**Usage in component:**
```tsx
import { newService } from '@/api';

const MyComponent = () => {
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        const response = await newService.getAll({ page: 1 });
        if (response.success) {
          setData(response.data);
        }
      } catch (error) {
        console.error(error);
      } finally {
        setLoading(false);
      }
    };
    
    fetchData();
  }, []);

  return <div>{/* JSX */}</div>;
};
```

---

## Troubleshooting

### Common Issues

#### 1. SCSS Module Not Working
**Problem:** CSS not applying to components

**Solution:**
- Check file named with `.module.scss` extension
- Import as: `import styles from './Component.module.scss'`
- Use: `className={styles.className}`

#### 2. Responsive Design Not Working
**Problem:** Mobile styles not applying

**Solution:**
```scss
// ✅ Correct
@media (max-width: 768px) {
  .container {
    padding: 12px;
  }
}

// ❌ Wrong
@media screen and (max-width: 768px) {
  // Avoid using 'screen'
}
```

#### 3. Ant Design Component Not Styling
**Problem:** Cannot override Ant Design styles

**Solution:**
```scss
// Use :global() for Ant Design components
:global(.ant-button) {
  border-radius: 4px;
}

// Or use CSS variables in theme.ts
// Then apply via ConfigProvider
```

#### 4. Build Failures
**Problem:** `npm run build` fails

**Solution:**
```bash
# Clear cache
rm -rf node_modules package-lock.json
npm install

# Check TypeScript errors
npm run type-check

# Check SCSS syntax
npm run lint:scss
```

#### 5. Images Not Loading
**Problem:** Images broken in production

**Solution:**
```tsx
// Use relative paths from public folder
<img src="/assets/images/banner.jpg" />

// Or import images
import bannerImg from '@/assets/images/banner.jpg';
<img src={bannerImg} />
```

### Performance Optimization

**1. Code Splitting:**
```tsx
import { lazy, Suspense } from 'react';
import { Spin } from 'antd';

const ProductDetail = lazy(() => import('@/pages/ProductDetail'));

<Suspense fallback={<Spin />}>
  <ProductDetail />
</Suspense>
```

**2. Image Optimization:**
```tsx
// Use Next.js Image component (if using Next.js)
// Or add loading="lazy" attribute
<img src="..." alt="..." loading="lazy" />
```

**3. Memoization:**
```tsx
import { memo } from 'react';

const ProductCard = memo(({ product, onAddToCart }) => {
  // Component code
});

export default ProductCard;
```

---

## Best Practices

### ✅ Do's
- ✅ Use TypeScript for type safety
- ✅ Keep components small and focused
- ✅ Use CSS Modules for local scoping
- ✅ Test responsive breakpoints
- ✅ Document complex logic with comments
- ✅ Use semantic HTML elements
- ✅ Handle loading and error states
- ✅ Write meaningful commit messages

### ❌ Don'ts
- ❌ Don't use inline styles (use CSS Modules)
- ❌ Don't ignore TypeScript warnings
- ❌ Don't hardcode values (use constants)
- ❌ Don't forget accessibility (alt text, labels)
- ❌ Don't make components too complex
- ❌ Don't ignore console errors/warnings
- ❌ Don't push broken code to production
- ❌ Don't commit node_modules or build files

---

## Testing

### Running Tests (if configured)
```bash
npm run test          # Run tests
npm run test:watch   # Watch mode
npm run test:coverage # Coverage report
```

### Manual Testing Checklist
- [ ] Desktop view (1920px)
- [ ] Tablet view (768px)
- [ ] Mobile view (375px)
- [ ] Search functionality
- [ ] Filters working
- [ ] Add to cart
- [ ] Responsive images
- [ ] Links working
- [ ] Form validation
- [ ] Error handling

---

## Useful Commands

```bash
npm run dev           # Start dev server
npm run build         # Build for production
npm run preview       # Preview build
npm run type-check    # TypeScript check
npm run lint          # ESLint
npm run format        # Prettier format
```

---

## Resources

- [React Docs](https://react.dev)
- [TypeScript Docs](https://www.typescriptlang.org/docs/)
- [Ant Design Docs](https://ant.design)
- [Vite Docs](https://vitejs.dev)
- [SCSS Docs](https://sass-lang.com)

---

## Getting Help

1. **Check existing documentation:**
   - DESIGN_UPDATES.md
   - TECHNICAL_DETAILS.md
   - This file

2. **Check Ant Design documentation:**
   - Most UI components from Ant Design
   - Examples in their docs

3. **Debug with browser tools:**
   - Chrome DevTools (F12)
   - React DevTools extension
   - Network tab for API calls

4. **Git history:**
   - `git log` to see changes
   - `git diff` to see specific changes

---

**Last Updated**: February 3, 2026
**Maintainer**: Development Team
**Status**: Active Development ✅
