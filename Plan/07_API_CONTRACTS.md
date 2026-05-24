# Hợp Đồng API RESTful - FISH SHOP

> Base URL qua Docker Gateway: `http://localhost:5000/api`
> Khi chạy Gateway trực tiếp bằng `dotnet run`: `http://localhost:8080/api`
> Response chuẩn: `{ success, data, message, errors }`

## Quy Ước REST

| Service | Prefix | Resource mẫu |
|---|---|---|
| User | `/api/user` | `/api/user/auth/login`, `/api/user/customers` |
| Product | `/api/product` | `/api/product/products`, `/api/product/categories` |
| Order | `/api/order` | `/api/order/cart`, `/api/order/orders` |
| Content | `/api/content` | `/api/content/blogs`, `/api/content/contacts` |

Không dùng action trong URL kiểu `tim-kiem`, `dang-nhap`, `dat-hang`. Hành động được thể hiện bằng HTTP method, resource path và query string.

## Authentication

| Method | Endpoint | Mô tả |
|---|---|---|
| `POST` | `/api/user/auth/login` | Đăng nhập |
| `POST` | `/api/user/auth/register` | Đăng ký khách hàng |
| `POST` | `/api/user/auth/refresh-token` | Làm mới token |
| `POST` | `/api/user/auth/logout` | Đăng xuất |
| `GET` | `/api/user/auth/me` | Lấy thông tin người dùng hiện tại |
| `PUT` | `/api/user/auth/me` | Cập nhật thông tin |
| `PUT` | `/api/user/auth/password` | Đổi mật khẩu |

Login request:

```json
{ "username": "admin", "password": "Admin@123" }
```

## Products

| Method | Endpoint | Mô tả |
|---|---|---|
| `GET` | `/api/product/products` | Danh sách sản phẩm, phân trang và lọc |
| `GET` | `/api/product/products/{id}` | Chi tiết sản phẩm |
| `GET` | `/api/product/products/slug/{slug}` | Chi tiết theo slug |
| `GET` | `/api/product/products/featured?top=8` | Sản phẩm nổi bật |
| `POST` | `/api/product/products` | Tạo sản phẩm, `multipart/form-data` |
| `PUT` | `/api/product/products/{id}` | Cập nhật sản phẩm, `multipart/form-data` |
| `DELETE` | `/api/product/products/{id}` | Xóa sản phẩm |

Query danh sách:

```text
page, pageSize, name, categoryId, status, isFeatured, minPrice, maxPrice, sortBy, sortDir
```

## Categories

| Method | Endpoint | Mô tả |
|---|---|---|
| `GET` | `/api/product/categories` | Danh sách danh mục |
| `GET` | `/api/product/categories/tree` | Cây danh mục |
| `GET` | `/api/product/categories/{id}` | Chi tiết danh mục |
| `POST` | `/api/product/categories` | Tạo danh mục, `multipart/form-data` |
| `PUT` | `/api/product/categories/{id}` | Cập nhật danh mục |
| `DELETE` | `/api/product/categories/{id}` | Xóa danh mục |

## Inventory

| Method | Endpoint | Mô tả |
|---|---|---|
| `GET` | `/api/product/inventory/transactions` | Lịch sử nhập/xuất kho |
| `POST` | `/api/product/inventory/imports` | Nhập hàng |
| `GET` | `/api/product/inventory/low-stock` | Sản phẩm sắp hết hàng |

## Cart

| Method | Endpoint | Mô tả |
|---|---|---|
| `GET` | `/api/order/cart` | Giỏ hàng hiện tại |
| `POST` | `/api/order/cart/items` | Thêm sản phẩm vào giỏ |
| `PUT` | `/api/order/cart/items/{cartItemId}` | Cập nhật số lượng |
| `DELETE` | `/api/order/cart/items/{cartItemId}` | Xóa một dòng giỏ hàng |
| `DELETE` | `/api/order/cart` | Xóa toàn bộ giỏ hàng |

Add item request:

```json
{ "productId": 1, "quantity": 2 }
```

## Orders

| Method | Endpoint | Mô tả |
|---|---|---|
| `GET` | `/api/order/orders` | Admin xem danh sách đơn hàng |
| `GET` | `/api/order/orders/me` | Khách hàng xem đơn của mình |
| `GET` | `/api/order/orders/{orderCode}` | Chi tiết đơn hàng |
| `POST` | `/api/order/orders` | Tạo đơn từ giỏ hàng |
| `POST` | `/api/order/orders/direct` | Tạo đơn trực tiếp/POS |
| `PATCH` | `/api/order/orders/{orderCode}/status` | Admin cập nhật trạng thái |
| `DELETE` | `/api/order/orders/{orderCode}?reason=...` | Hủy đơn |

Create order from cart:

```json
{
  "customerName": "Nguyen Van A",
  "customerPhone": "0901234567",
  "shippingAddress": "123 Tran Phu, Ha Noi",
  "paymentMethod": "COD",
  "promotionCode": "SALE10",
  "shippingFee": 30000,
  "note": "Giao buoi sang"
}
```

Direct/POS order:

```json
{
  "customerId": 1,
  "customerName": "Khach le",
  "customerPhone": "0901234567",
  "customerAddress": "Tai quay",
  "paymentMethod": "CASH",
  "source": "POS",
  "items": [
    { "productId": 1, "quantity": 2, "unitPrice": 150000 }
  ]
}
```

## Payments And Promotions

| Method | Endpoint | Mô tả |
|---|---|---|
| `POST` | `/api/order/payments` | Tạo giao dịch thanh toán |
| `GET` | `/api/order/payments/order/{orderId}` | Lấy thanh toán theo đơn |
| `GET` | `/api/order/promotions` | Danh sách khuyến mãi |
| `POST` | `/api/order/promotions` | Tạo khuyến mãi |
| `PUT` | `/api/order/promotions/{id}` | Cập nhật khuyến mãi |
| `DELETE` | `/api/order/promotions/{id}` | Xóa khuyến mãi |
| `POST` | `/api/order/promotions/validate` | Kiểm tra mã giảm giá |

Validate promotion:

```json
{ "code": "SALE10", "orderAmount": 300000 }
```

## Dashboard, Sales, Reports

| Method | Endpoint | Mô tả |
|---|---|---|
| `GET` | `/api/order/dashboard/stats` | Thống kê dashboard |
| `GET` | `/api/order/sales/stats` | Thống kê bán hàng |
| `GET` | `/api/order/reports/revenue` | Báo cáo doanh thu |
| `GET` | `/api/order/reports/top-products` | Sản phẩm bán chạy |
| `GET` | `/api/order/reports/order-summary` | Tổng hợp đơn hàng |

## Blog And Contact

| Method | Endpoint | Mô tả |
|---|---|---|
| `GET` | `/api/content/blogs` | Danh sách bài viết |
| `GET` | `/api/content/blogs/{id}` | Chi tiết bài viết theo id |
| `GET` | `/api/content/blogs/slug/{slug}` | Chi tiết bài viết theo slug |
| `POST` | `/api/content/blogs` | Tạo bài viết |
| `PUT` | `/api/content/blogs/{id}` | Cập nhật bài viết |
| `DELETE` | `/api/content/blogs/{id}` | Xóa bài viết |
| `GET` | `/api/content/blog-categories` | Danh mục blog |
| `POST` | `/api/content/contacts` | Gửi liên hệ |
| `GET` | `/api/content/contacts` | Admin xem liên hệ |
| `PATCH` | `/api/content/contacts/{id}/status` | Cập nhật trạng thái liên hệ |

## Message Queue

Luồng bất đồng bộ chính:

1. `API.Order` tạo đơn hàng và publish `OrderCreatedEvent`.
2. RabbitMQ nhận event.
3. `API.Product` consume event qua queue `order-created-queue`.
4. `API.Product` kiểm tra tồn kho, ghi `INVENTORY_TRANSACTIONS`, trừ `STOCK_QUANTITY`, tăng `SOLD_QUANTITY`.

