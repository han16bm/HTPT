# Hướng Dẫn Triển Khai - FISH SHOP

> Môi trường development hiện tại dùng .NET 8, React/Vite, SQL Server, RabbitMQ, Seq và API Gateway.

## Công Cụ Cần Có

| Công cụ | Phiên bản khuyến nghị | Mục đích |
|---|---:|---|
| .NET SDK | 8.x | Build và chạy backend |
| Node.js | 18+ hoặc 20 LTS | Chạy frontend |
| npm | Đi kèm Node.js | Cài package frontend |
| Docker Desktop | Bản mới | Chạy RabbitMQ, Seq và backend bằng Docker Compose |
| SQL Server | 2019+ / 2022 Developer hoặc Express | Cơ sở dữ liệu |
| `sqlcmd` hoặc SSMS/Azure Data Studio | Bản mới | Chạy script database |

## Chuẩn Bị Database

SQL Server cần chạy sẵn trên máy host. Script setup nằm tại:

```text
API/database/sqlserver_full_setup.sql
```

Chạy từ thư mục gốc dự án:

```powershell
cd API\database
sqlcmd -S localhost -U sa -P viet123 -i .\sqlserver_full_setup.sql
```

Script sẽ tạo lại 4 database và seed dữ liệu mẫu:

```text
FishShop_User
FishShop_Product
FishShop_Order
FishShop_Content
```

Tài khoản demo:

| Vai trò | Username | Password |
|---|---|---|
| Admin | `admin` | `123456` |
| Khách hàng | `customer01` | `123456` |

## Chạy Backend Bằng Docker Compose

Từ thư mục gốc dự án:

```powershell
docker compose up --build
```

Các URL chính:

| Dịch vụ | URL |
|---|---|
| API Gateway | `http://localhost:5000` |
| RabbitMQ UI | `http://localhost:15672` |
| Seq Log UI | `http://localhost:5341` |

RabbitMQ mặc định dùng `guest / guest`.

## Chạy Backend Trực Tiếp

Chạy RabbitMQ và Seq:

```powershell
docker compose up rabbitmq seq
```

Mở các terminal riêng:

```powershell
dotnet run --project API\Services\API.User\API.User.csproj --launch-profile http
```

```powershell
$env:RabbitMQ__Host='localhost'
dotnet run --project API\Services\API.Product\API.Product.csproj --launch-profile http
```

```powershell
$env:RabbitMQ__Host='localhost'
dotnet run --project API\Services\API.Order\API.Order.csproj --launch-profile http
```

```powershell
dotnet run --project API\Services\API.Content\API.Content.csproj --launch-profile http
```

```powershell
dotnet run --project API\Gateway\FishShop.Gateway\FishShop.Gateway.csproj --urls http://localhost:5000
```

Port mặc định:

| Service | URL |
|---|---|
| API.User | `http://localhost:5001` |
| API.Product | `http://localhost:5002` |
| API.Order | `http://localhost:5003` |
| API.Content | `http://localhost:5005` |
| Gateway | `http://localhost:5000` |

## Chạy Frontend

Admin:

```powershell
cd FE
npm install
npm run dev
```

URL admin: `http://localhost:3000`

Khách hàng:

```powershell
cd FE-Customer
npm install
npm run dev
```

URL khách hàng: `http://localhost:3001`

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

## Thứ Tự Khởi Động Dev

1. Chạy SQL Server.
2. Chạy `API/database/sqlserver_full_setup.sql`.
3. Chạy RabbitMQ và Seq, hoặc chạy toàn bộ bằng `docker compose up --build`.
4. Chạy Gateway và các service backend nếu debug trực tiếp.
5. Chạy `FE` và `FE-Customer`.

## Kiểm Tra Nhanh

```text
GET http://localhost:5000/api/user/auth/health
GET http://localhost:5000/api/product/products/health
GET http://localhost:5000/api/order/orders/health
GET http://localhost:5000/api/content/blogs/health
```
