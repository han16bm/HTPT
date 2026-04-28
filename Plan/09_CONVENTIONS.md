# 📖 Quy Ước Code — FISH SHOP

> Áp dụng cho toàn dự án: Backend (.NET 8) + Frontend (React/TypeScript)

---

## I. Quy Ước Đặt Tên

### Backend (.NET)

| Loại | Quy tắc | Ví dụ |
|------|---------|-------|
| Class / Interface | PascalCase | `ProductRepository`, `IUserService` |
| Method | PascalCase | `GetByIdAsync`, `CreateProductAsync` |
| Property | PascalCase | `ProductCode`, `SalePrice` |
| Private field | _camelCase | `_repository`, `_logger` |
| Parameter / Local var | camelCase | `productId`, `cancellationToken` |
| Constant | PascalCase | `DefaultPageSize` |
| Enum | PascalCase (type + value) | `OrderStatus.Pending` |
| Controller | `{Resource}Controller` | `ProductsController` |
| Service Interface | `I{Name}Service` | `IProductService` |
| Command | `{Action}{Entity}Command` | `CreateProductCommand` |
| Query | `Get{Entity/s}Query` | `GetProductsQuery` |
| Handler | `{Command/Query}Handler` | `CreateProductCommandHandler` |
| DTO | `{Entity}Dto` hoặc `{Entity}ListDto` | `ProductDto`, `ProductListDto` |
| Configuration | `{Entity}Configuration` | `ProductConfiguration` |

### Frontend (TypeScript/React)

| Loại | Quy tắc | Ví dụ |
|------|---------|-------|
| React Component | PascalCase | `ProductCard`, `CustomerLayout` |
| Hook | `use` + PascalCase | `useCart`, `useAuth` |
| Interface/Type | PascalCase | `Product`, `OrderItem` |
| Enum | PascalCase | `OrderStatus` |
| Constant | UPPER_SNAKE_CASE | `API_ENDPOINTS`, `REQUEST_TIMEOUT` |
| Function/Variable | camelCase | `handleAddToCart`, `isLoading` |
| SCSS class | camelCase (CSS Modules) | `productCard`, `imageWrapper` |
| File - Component | PascalCase | `ProductCard.tsx` |
| File - Hook | camelCase | `useCart.ts` |
| File - Utility | camelCase | `formatters.ts` |
| SCSS Module | `{Component}.module.scss` | `ProductCard.module.scss` |

---

## II. Cấu Trúc File Component (FE)

```typescript
// ProductCard.tsx — Thứ tự chuẩn

// 1. Imports: thư viện bên ngoài
import React, { useState, useEffect, memo } from 'react';
import { Button, Tag } from 'antd';

// 2. Imports: nội bộ (types, hooks, api)
import { useCart } from '@/hooks/useCart';
import { productService } from '@/api';
import type { Product } from '@/interfaces';

// 3. Import styles
import styles from './ProductCard.module.scss';

// 4. Interface Props
interface ProductCardProps {
  product: Product;
  onAddToCart?: (product: Product) => void;
}

// 5. Component
const ProductCard: React.FC<ProductCardProps> = ({ product, onAddToCart }) => {
  // 5a. State
  const [isFavorite, setIsFavorite] = useState(false);
  
  // 5b. Hooks
  const { addItem } = useCart();
  
  // 5c. Computed values
  const isOutOfStock = product.stockQuantity === 0;
  
  // 5d. Effects
  useEffect(() => {
    // ...
  }, []);
  
  // 5e. Handlers
  const handleAddToCart = () => {
    addItem(product, 1);
    onAddToCart?.(product);
  };
  
  // 5f. Render
  return (
    <div className={styles.card}>
      {/* JSX */}
    </div>
  );
};

// 6. Export
export default memo(ProductCard);
```

---

## III. Cấu Trúc Controller (.NET)

```csharp
// Theo chuẩn từ NKT.Internal README.md
[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetProductsQuery query,
        CancellationToken ct)
    {
        _logger.LogInformation("Getting products: {@Query}", query);
        var result = await _mediator.Send(query, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductCommand cmd,
        CancellationToken ct)
    {
        var result = await _mediator.Send(cmd, ct);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }
}
```

---

## IV. Logging Conventions

### Frontend
```typescript
// Dùng console có chọn lọc, bọc trong debug check
import { env } from '@/config/env';

if (env.enableDebug) {
  console.log('[ProductCard] Adding to cart:', product.id);
  console.error('[useAuth] Token refresh failed:', error);
}
```

### Backend (Serilog)
```csharp
// Log có structure — dùng {@} để serialize objects
_logger.LogInformation("Creating product: {@Command}", cmd);
_logger.LogWarning("Product {ProductId} out of stock", productId);
_logger.LogError(ex, "Failed to process order {OrderCode}", orderCode);

// KHÔNG log dữ liệu nhạy cảm
// ❌ _logger.LogInformation("User login: password={Password}", password);
// ✅ _logger.LogInformation("User login attempt: username={Username}", username);
```

---

## V. Error Handling

### Frontend — Centralized
```typescript
// axiosClient.ts interceptor
axiosClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiResponse<unknown>>) => {
    if (error.response?.status === 401) {
      // Redirect to login
    }
    const message = error.response?.data?.message
      ?? ERROR_MESSAGES[error.response?.status ?? 0]
      ?? ERROR_MESSAGES.UNKNOWN_ERROR;
    
    notification.error({ message: 'Lỗi', description: message });
    return Promise.reject(error);
  }
);
```

### Backend — Exception Middleware
```csharp
// GlobalExceptionFilter.cs
public class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var (statusCode, message) = context.Exception switch
        {
            NotFoundException ex => (404, ex.Message),
            BusinessException ex => (400, ex.Message),
            UnauthorizedException ex => (401, ex.Message),
            _ => (500, "Đã có lỗi xảy ra. Vui lòng thử lại sau.")
        };
        
        context.Result = new ObjectResult(BaseResponse<object>.Fail(message))
            { StatusCode = statusCode };
        context.ExceptionHandled = true;
    }
}
```

---

## VI. API Service Pattern (FE)

```typescript
// src/api/apiService.ts
export const productService = {
  getAll: async (params: GetProductsParams): Promise<ApiResponse<PagedResult<Product>>> => {
    const response = await axiosClient.get<ApiResponse<PagedResult<Product>>>(
      API_ENDPOINTS.PRODUCTS,
      { params }
    );
    return response.data;
  },

  getById: async (id: number): Promise<ApiResponse<Product>> => {
    const response = await axiosClient.get<ApiResponse<Product>>(
      API_ENDPOINTS.PRODUCT_BY_ID(id)
    );
    return response.data;
  },

  create: async (data: CreateProductRequest): Promise<ApiResponse<Product>> => {
    const response = await axiosClient.post<ApiResponse<Product>>(
      API_ENDPOINTS.PRODUCTS,
      data
    );
    return response.data;
  },
};

// Sử dụng trong component:
const fetchProducts = async () => {
  setLoading(true);
  try {
    const res = await productService.getAll({ page: 1, pageSize: 10 });
    if (res.success && res.data) {
      setProducts(res.data.items);
      setTotal(res.data.totalCount);
    }
  } catch {
    // Đã xử lý trong interceptor
  } finally {
    setLoading(false);
  }
};
```

---

## VII. SCSS Module Convention

```scss
// ProductCard.module.scss
// BEM trong camelCase vì CSS Modules

.card {
  border-radius: 8px;
  overflow: hidden;
  transition: box-shadow 0.2s ease;

  &:hover {
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  }

  // Element
  .imageWrapper {
    position: relative;
    aspect-ratio: 1;
    overflow: hidden;
  }

  .title {
    font-size: 14px;
    font-weight: 600;
    color: #1a1a2e;
  }

  // Ant Design override (global scope)
  :global(.ant-btn-primary) {
    width: 100%;
  }
}

// Modifier — dùng thêm class
.cardFeatured {
  border: 2px solid #ffd60a;
}

.cardOutOfStock {
  opacity: 0.7;
}
```

---

## VIII. Git Commit Convention

```
# Format:
<type>(<scope>): <message ngắn>

# Types:
feat      — Tính năng mới
fix       — Sửa bug
refactor  — Refactor code (không thêm tính năng, không sửa bug)
style     — Format code, styling
docs      — Cập nhật tài liệu
chore     — Cấu hình, build tools
test      — Thêm/sửa tests
perf      — Tối ưu hiệu năng

# Ví dụ:
feat(products): add product filter by price range
fix(cart): fix quantity not updating in localStorage
refactor(auth): extract token refresh logic to hook
docs(plan): update API contracts for orders
chore(api): upgrade EF Core to 8.0.5
feat(FE-admin): add inventory import modal
```

---

## IX. Environment Variables

### FE Admin (`.env`)
```env
VITE_API_URL=http://localhost:8080/api
VITE_APP_NAME=FishShop Admin
VITE_ENABLE_DEBUG=false
```

### FE Customer (`.env`)
```env
VITE_API_URL=http://localhost:8080/api
VITE_APP_NAME=Cá Cảnh Shop
VITE_ENABLE_DEBUG=false
```

### Backend (`appsettings.Development.json`)
```json
{
  "ConnectionStrings": {
    "Oracle": "User Id=fishuser;Password=fish123;Data Source=localhost:1521/FISHDB"
  },
  "Jwt": {
    "SecretKey": "dev-secret-key-min-32-chars-here!",
    "Issuer": "FishShop",
    "Audience": "FishShop",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173", "http://localhost:5174"]
  }
}
```

---

## X. Barrel Exports

```typescript
// Mỗi folder component/page có index.ts

// components/ProductCard/index.ts
export { default } from './ProductCard';
export type { ProductCardProps } from './ProductCard';

// components/Layout/index.ts
export { default as CustomerLayout } from './CustomerLayout';
export { default as MasterLayout } from './MasterLayout';

// Sử dụng:
import { CustomerLayout } from '@/components/Layout';
import ProductCard from '@/components/ProductCard';
```

---

## XI. TypeScript Strict Mode

```json
// tsconfig.app.json
{
  "compilerOptions": {
    "strict": true,
    "noUncheckedIndexedAccess": true,
    "noImplicitReturns": true,
    "exactOptionalPropertyTypes": false
  }
}
```

**Quy tắc:**
- Không dùng `any` — dùng `unknown` nếu cần
- Không ignore TypeScript warnings (`// @ts-ignore`)
- Luôn type return của async function
- Dùng `type` thay `interface` cho union types
