# FISH SHOP - Đồ Án Microservice

Website bán cá cảnh và phụ kiện theo kiến trúc microservice. Hệ thống gồm backend .NET 8, API Gateway, RabbitMQ, SQL Server và hai frontend React/Vite cho quản trị và khách hàng.

## Mục Tiêu Đồ Án

Dự án bám theo yêu cầu đề bài:

- Có nhiều microservice chạy độc lập.
- Giao tiếp đồng bộ bằng RESTful API.
- Giao tiếp bất đồng bộ bằng message queue.
- Có API Gateway.
- Có xác thực JWT và API key nội bộ giữa Gateway và service.
- Có cơ sở dữ liệu tách theo vùng/service.
- Có logging và xử lý lỗi cơ bản.

## Kiến Trúc Tổng Quan

```text
FE Admin      FE Customer
   |              |
   +------ REST API qua Gateway ------+
                                      |
                             FishShop.Gateway
                                      |
        +-------------+---------------+--------------+
        |             |               |              |
    API.User     API.Product      API.Order     API.Content
        |             |               |              |
 FishShop_User  FishShop_Product FishShop_Order FishShop_Content
                      ^
                      |
          RabbitMQ + MassTransit Saga
          API.Order tạo đơn, API.Product reserve/hoàn kho,
          API.Order xử lý payment và cập nhật kết quả
```

## Cấu Trúc Thư Mục

```text
HTPT/
|-- API/
|   |-- API.sln
|   |-- assets/
|   |   +-- user/ product/ order/ content/  # Lưu ảnh upload, chỉ commit folder/.gitkeep
|   |-- database/
|   |   +-- sqlserver_full_setup.sql  # Tạo 4 database và seed dữ liệu mẫu
|   |-- tests/
|   |   +-- interservice-order-flow.http  # Test luồng liên service qua Gateway
|   |-- Gateway/FishShop.Gateway/
|   |-- netcore/netcore.Commons/
|   |-- netcore/netcore.Entities/
|   +-- Services/
|       |-- API.User/
|       |-- API.Product/
|       |-- API.Order/
|       +-- API.Content/
|-- FE/                 # Giao diện quản trị
|-- FE-Customer/        # Giao diện khách hàng
|-- Plan/               # Tài liệu thiết kế
|-- Debai.txt
|-- docker-compose.yml
+-- README.md
```

## Công Cụ Cần Cài

| Công cụ | Phiên bản khuyến nghị | Mục đích |
|---|---:|---|
| .NET SDK | 8.x | Build/chạy backend |
| Node.js | 18+ hoặc 20 LTS | Chạy frontend React/Vite |
| npm | Đi kèm Node.js | Cài package frontend |
| Docker Desktop | Bản mới | Chạy RabbitMQ, Seq, Jaeger, Prometheus, Grafana và backend bằng Docker Compose |
| SQL Server | 2019+ / 2022 Developer hoặc Express | Cơ sở dữ liệu |
| SQL Server command-line tools (`sqlcmd`) | Bản mới | Chạy script tạo DB/seed từ terminal |
| SQL Server Management Studio hoặc Azure Data Studio | Bản mới | Chạy script tạo DB |
| Git | Bản mới | Quản lý mã nguồn |
| Postman hoặc REST Client | Tùy chọn | Test API thủ công |

Lưu ý: `docker-compose.yml` hiện chưa tạo container SQL Server. SQL Server cần chạy sẵn trên máy host, hoặc bạn chỉnh lại connection string trong các file `appsettings.json`.

## Cấu Hình Mặc Định

Backend đang dùng connection string mặc định:

```text
Server=host.docker.internal;Database=FishShop_*;User Id=sa;Password=viet123;TrustServerCertificate=True;
```

Nếu SQL Server của bạn không dùng tài khoản/mật khẩu trên, sửa các file:

```text
API/Services/API.User/appsettings.json
API/Services/API.Product/appsettings.json
API/Services/API.Order/appsettings.json
API/Services/API.Content/appsettings.json
```

Khi chạy backend trực tiếp không qua Docker, có thể đổi `host.docker.internal` thành `localhost`.

## Chuẩn Bị Cơ Sở Dữ Liệu Và Seed

1. Mở SQL Server và bật đăng nhập bằng tài khoản `sa`, hoặc cập nhật connection string theo tài khoản của bạn.
2. Vào thư mục chứa script database:

```powershell
cd API\database
```

3. Chạy script tạo toàn bộ database và seed dữ liệu mẫu:

```powershell
sqlcmd -S localhost -U sa -P viet123 -i .\sqlserver_full_setup.sql
```

Nếu không dùng `sqlcmd`, mở file sau bằng SSMS/Azure Data Studio và chạy toàn bộ:

```text
API/database/sqlserver_full_setup.sql
```

Script này là file SQL duy nhất cần chạy trong `API/database`. File sẽ xóa database cũ nếu đã tồn tại, tạo lại 4 database và nạp dữ liệu mẫu:

```text
FishShop_User
FishShop_Product
FishShop_Order
FishShop_Content
```

Tài khoản seed để demo:

| Vai trò | Username | Password |
|---|---|---|
| Admin | `admin` | `123456` |
| Khách hàng | `customer01` | `123456` |

Kiểm tra nhanh dữ liệu seed sau khi chạy script:

```powershell
sqlcmd -S localhost -U sa -P viet123 -d FishShop_Product -Q "SELECT TOP 5 ID, NAME, SALE_PRICE FROM dbo.PRODUCTS"
```

Khi sửa dữ liệu seed trong `API/database/sqlserver_full_setup.sql`, cần chạy lại lệnh `sqlcmd` ở trên để xóa và tạo lại dữ liệu mẫu. Docker build không tự chạy lại seed database.

## Cách Chạy Backend Bằng Docker Compose

Từ thư mục gốc dự án:

```powershell
docker compose up --build
```

Khi sửa code backend và muốn chạy lại container, có thể dùng:

```powershell
docker compose up --build -d
```

Các dịch vụ chính:

| Dịch vụ | URL |
|---|---|
| API Gateway | `http://localhost:5000` |
| RabbitMQ UI | `http://localhost:15672` |
| Seq Log UI | `http://localhost:5341` |
| Jaeger UI (distributed tracing) | `http://localhost:16686` |
| Prometheus UI | `http://localhost:9090` |
| Grafana dashboard | `http://localhost:3002` (admin/admin) |

RabbitMQ mặc định:

```text
Username: guest
Password: guest
```

Kiểm tra health API qua Gateway:

```text
GET http://localhost:5000/api/user/auth/health
GET http://localhost:5000/api/product/products/health
GET http://localhost:5000/api/order/orders/health
GET http://localhost:5000/api/content/blogs/health
```

## Cách Chạy Backend Trực Tiếp Bằng .NET

Cách này phù hợp khi muốn debug từng service. Cần RabbitMQ, Seq, Jaeger và SQL Server đang chạy.

Chạy RabbitMQ, Seq và Jaeger:

```powershell
docker compose up rabbitmq seq jaeger
```

Mở các terminal riêng:

```powershell
$env:Otlp__Endpoint='http://localhost:4317'
dotnet run --project API\Services\API.User\API.User.csproj --launch-profile http
```

```powershell
$env:RabbitMQ__Host='localhost'
$env:Otlp__Endpoint='http://localhost:4317'
dotnet run --project API\Services\API.Product\API.Product.csproj --launch-profile http
```

```powershell
$env:RabbitMQ__Host='localhost'
$env:Otlp__Endpoint='http://localhost:4317'
dotnet run --project API\Services\API.Order\API.Order.csproj --launch-profile http
```

```powershell
$env:Otlp__Endpoint='http://localhost:4317'
dotnet run --project API\Services\API.Content\API.Content.csproj --launch-profile http
```

```powershell
$env:Otlp__Endpoint='http://localhost:4317'
dotnet run --project API\Gateway\FishShop.Gateway\FishShop.Gateway.csproj --urls http://localhost:5000
```

Port mặc định của các service:

| Service | URL |
|---|---|
| API.User | `http://localhost:5001` |
| API.Product | `http://localhost:5002` |
| API.Order | `http://localhost:5003` |
| API.Content | `http://localhost:5005` |
| Gateway | `http://localhost:5000` |

## Cách Chạy Frontend Admin

```powershell
cd FE
npm install
npm run dev
```

Giao diện admin chạy tại:

```text
http://localhost:3000
```

Frontend admin gọi API qua Vite proxy đến:

```text
http://localhost:5000/api
```

## Cách Chạy Frontend Khách Hàng

```powershell
cd FE-Customer
npm install
npm run dev
```

Giao diện khách hàng chạy tại:

```text
http://localhost:3001
```

## Kiểm Tra Build

Backend:

```powershell
dotnet build API\API.sln
```

Frontend admin:

```powershell
cd FE
npm run build
```

Frontend khách hàng:

```powershell
cd FE-Customer
npm run build
```

## Kiểm Thử Luồng Liên Service

Sau khi đã chạy database seed và backend, có thể dùng REST Client trong VS Code hoặc import thủ công vào Postman theo file:

```text
API/tests/interservice-order-flow.http
```

File này kiểm tra luồng chính: health check các service, đăng nhập khách hàng, đọc sản phẩm qua `API.Product`, thêm giỏ hàng và đặt hàng qua `API.Order`, sau đó kiểm tra lại sản phẩm để xác nhận Saga đã gửi message sang `API.Product` reserve tồn kho qua RabbitMQ.

## Một Số RESTful API Chính

Base URL qua Gateway:

```text
http://localhost:5000/api
```

Auth:

```text
POST /api/user/auth/login
POST /api/user/auth/register
POST /api/user/auth/refresh-token
GET  /api/user/auth/me
PUT  /api/user/auth/me
```

Product:

```text
GET    /api/product/products
GET    /api/product/products/{id}
GET    /api/product/products/slug/{slug}
POST   /api/product/products
PUT    /api/product/products/{id}
DELETE /api/product/products/{id}
```

Cart và Order:

```text
GET    /api/order/cart
POST   /api/order/cart/items
PUT    /api/order/cart/items/{cartItemId}
DELETE /api/order/cart/items/{cartItemId}
POST   /api/order/orders
GET    /api/order/orders/me
GET    /api/order/orders/{orderCode}
PATCH  /api/order/orders/{orderCode}/status
DELETE /api/order/orders/{orderCode}
```

Content:

```text
GET   /api/content/blogs
GET   /api/content/blog-categories
GET   /api/content/blogs/{id}       # Admin
GET   /api/content/blogs/slug/{slug}
POST  /api/content/blogs
PUT   /api/content/blogs/{id}
DELETE /api/content/blogs/{id}
POST  /api/content/contacts
PATCH /api/content/contacts/{id}/status
```

## Luồng Demo Đề Xuất

1. Chạy SQL Server và script `sqlserver_full_setup.sql`.
2. Chạy backend:

```powershell
docker compose up --build
```

3. Chạy frontend admin và frontend khách hàng.
4. Đăng nhập bằng `admin / 123456` ở trang admin.
5. Vào trang khách hàng, đăng nhập `customer01 / 123456`.
6. Xem danh sách sản phẩm.
7. Thêm sản phẩm vào giỏ hàng.
8. Đặt hàng.
9. Mở RabbitMQ UI kiểm tra các queue Saga.
10. Kiểm tra tồn kho sản phẩm giảm, có giao dịch xuất kho và trạng thái đơn được cập nhật.

## Ghi Chú Kỹ Thuật

- API đang dùng RESTful resource URL, không dùng action tiếng Việt trong URL như `tim-kiem`, `dat-hang`, `dang-nhap`.
- `API.Order` tạo đơn và publish `OrderCreatedEvent`.
- `API.Order` có Saga điều phối luồng đặt hàng: reserve tồn kho, xử lý payment, hoàn kho khi payment lỗi và cập nhật trạng thái đơn.
- `API.Product` chịu trách nhiệm tồn kho, consume `InventoryReservationRequested`/`InventoryReleaseRequested` qua RabbitMQ để trừ hoặc hoàn kho.
- Các queue RabbitMQ chính: `order-saga`, `inventory-reservation-queue`, `inventory-release-queue`, `payment-process-queue`, `order-completed-queue`, `order-failed-queue`.
- Gateway inject `X-Api-Key` vào request nội bộ để các service xác nhận request đi qua Gateway.
- Public blog chỉ đọc danh sách và chi tiết theo slug; lấy blog theo id dùng cho admin.
- Các endpoint cần đăng nhập dùng JWT Bearer token.

## Xử Lý Lỗi Thường Gặp

Nếu frontend báo lỗi mạng:

- Kiểm tra Gateway có chạy ở `http://localhost:5000`.
- Kiểm tra file `FE/vite.config.ts` và `FE-Customer/vite.config.ts` đang proxy `/api` tới `http://localhost:5000`.

Nếu API báo lỗi kết nối DB:

- Kiểm tra SQL Server đang chạy.
- Kiểm tra tài khoản `sa` và mật khẩu trong `appsettings.json`.
- Kiểm tra đã chạy `sqlserver_full_setup.sql`.

Nếu đặt hàng không trừ kho:

- Kiểm tra RabbitMQ đang chạy.
- Kiểm tra `API.Order` publish `OrderCreatedEvent`.
- Kiểm tra Saga queue `order-saga`.
- Kiểm tra `API.Product` đang chạy và consume queue `inventory-reservation-queue`.
