# Thiết Kế Cơ Sở Dữ Liệu - FISH SHOP

> Nguồn chạy hiện tại: `/API/database/sqlserver_full_setup.sql`

File `sqlserver_full_setup.sql` là script duy nhất cần dùng trong thư mục `API/database`. Script này tạo lại toàn bộ database SQL Server và nạp dữ liệu mẫu để chạy demo.

## Nguyên Tắc Thiết Kế

- Mỗi microservice có database riêng.
- Không dùng khóa ngoại trực tiếp giữa các database khác service.
- Dữ liệu cần hiển thị ở service khác được lưu snapshot hoặc lấy qua REST API.
- Khi tạo đơn hàng, `API.Order` publish `OrderCreatedEvent`; `API.Product` consume event để trừ tồn kho.

## Các Database

| Database | Service sở hữu | Bảng chính |
|---|---|---|
| `FishShop_User` | `API.User` | `ROLES`, `USERS`, `CUSTOMER_PROFILES`, `CUSTOMER_ADDRESSES` |
| `FishShop_Product` | `API.Product` | `CATEGORIES`, `PRODUCTS`, `PRODUCT_IMAGES`, `INVENTORY`, `INVENTORY_TRANSACTIONS` |
| `FishShop_Order` | `API.Order` | `SHOPPING_CARTS`, `CART_ITEMS`, `PROMOTIONS`, `ORDERS`, `ORDER_ITEMS`, `PAYMENTS`, `PROMOTION_PRODUCTS`, `PROMOTION_USAGES` |
| `FishShop_Content` | `API.Content` | `BLOG_CATEGORIES`, `BLOG_POSTS`, `CONTACT_MESSAGES` |

## Chạy Script Tạo DB Và Seed

Từ thư mục gốc dự án:

```powershell
cd API\database
sqlcmd -S localhost -U sa -P viet123 -i .\sqlserver_full_setup.sql
```

Nếu không dùng `sqlcmd`, mở file `API/database/sqlserver_full_setup.sql` bằng SQL Server Management Studio hoặc Azure Data Studio và chạy toàn bộ file.

Lưu ý: script sẽ xóa các database cũ nếu đã tồn tại rồi tạo lại từ đầu.

## Dữ Liệu Seed Chính

| Nhóm dữ liệu | Nội dung |
|---|---|
| Tài khoản | `admin / 123456`, `customer01 / 123456` |
| Danh mục | Cá cảnh, phụ kiện, thức ăn |
| Sản phẩm | Một số sản phẩm demo kèm giá bán và tồn kho |
| Khuyến mãi | Mã khuyến mãi mẫu trong `FishShop_Order` |
| Nội dung | Danh mục blog và bài viết mẫu |

Kiểm tra nhanh seed sản phẩm:

```powershell
sqlcmd -S localhost -U sa -P viet123 -d FishShop_Product -Q "SELECT TOP 5 ID, NAME, SALE_PRICE FROM dbo.PRODUCTS"
```

## Connection String Mặc Định

Các service đang dùng SQL Server:

```text
Server=host.docker.internal;Database=FishShop_*;User Id=sa;Password=viet123;TrustServerCertificate=True;
```

Khi chạy service trực tiếp bằng `dotnet run`, có thể đổi `host.docker.internal` thành `localhost` trong các file `appsettings.json`.

## Quy Ước Cập Nhật Schema

Nếu cần đổi cấu trúc database, cập nhật trực tiếp trong `API/database/sqlserver_full_setup.sql` để repo chỉ có một script chuẩn cho setup và seed. Không tạo thêm script rời nếu thay đổi đó đã được gộp vào script tổng.
