# 🚀 Hướng Dẫn Triển Khai — FISH SHOP

> Môi trường Development và hướng dẫn bắt đầu dự án

---

## Yêu Cầu Môi Trường

| Phần mềm | Phiên bản | Mục đích |
|----------|-----------|----------|
| .NET SDK | 8.x | Build & run Backend |
| Node.js | >= 18.x | Build & run Frontend |
| npm | >= 9.x | Package manager |
| Oracle Database | 19c+ | Cơ sở dữ liệu |
| Oracle Data Access Client (ODAC) | Latest | .NET driver |
| Visual Studio 2022 / Rider | Latest | IDE Backend |
| VS Code | Latest | IDE Frontend |

---

## Cài Đặt Oracle (Dev Local)

### Option A: Oracle XE (miễn phí)
```bash
# Download Oracle Database 21c XE từ oracle.com
# Cài đặt và tạo user:
sqlplus / as sysdba

CREATE USER fishuser IDENTIFIED BY Fish@123456;
GRANT CONNECT, RESOURCE, DBA TO fishuser;
GRANT UNLIMITED TABLESPACE TO fishuser;

# Chạy schema:
sqlplus fishuser/Fish@123456@localhost:1521/XE @"C:\Users\Admin\Desktop\FISH_SHOP\API\database\oracle_full_schema.sql"
```

### Option B: Oracle Docker
```bash
docker run -d \
  --name fish-oracle \
  -p 1521:1521 \
  -e ORACLE_PASSWORD=Fish@123456 \
  -e APP_USER=fishuser \
  -e APP_USER_PASSWORD=Fish@123456 \
  container-registry.oracle.com/database/express:21.3.0-xe
```

---

## Chạy Backend

### 1. Cấu hình Connection String
```json
// API/Src/FishShop.API/appsettings.Development.json
{
  "ConnectionStrings": {
    "Oracle": "User Id=fishuser;Password=Fish@123456;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XE)))"
  },
  "Jwt": {
    "SecretKey": "FishShopSuperSecretKeyForJWT2026!",
    "Issuer": "FishShop",
    "Audience": "FishShop",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

### 2. (Khi có EF Migrations) Tạo migration và update database
```bash
cd API

# Tạo migration đầu tiên (sau khi implement Infrastructure)
dotnet ef migrations add InitialCreate \
  --project Core/FishShop.Infrastructure \
  --startup-project Src/FishShop.API

# Áp migration
dotnet ef database update \
  --project Core/FishShop.Infrastructure \
  --startup-project Src/FishShop.API
```

> **Lưu ý:** Schema Oracle đã có sẵn trong `database/oracle_full_schema.sql`.
> Trong giai đoạn đầu có thể chạy SQL trực tiếp rồi dùng EF theo dõi (không migrate-down toàn bộ).

### 3. Chạy FishShop.API
```bash
cd API/Src/FishShop.API
dotnet run
# → Swagger: https://localhost:5001/swagger
```

### 4. Chạy FishShop.Admin.API
```bash
cd API/Src/FishShop.Admin.API
dotnet run
# → Swagger: https://localhost:5002/swagger
```

### 5. Chạy cả solution từ Visual Studio
- Chuột phải solution → **Set Startup Projects** → Multiple startup
- Chọn: `FishShop.API` + `FishShop.Admin.API`
- Press **F5**

---

## Chạy Frontend

### FE Admin
```bash
cd FE

# Cài dependencies (lần đầu)
npm install

# Tạo file .env.development
echo "VITE_API_URL=http://localhost:8080/api" > .env.development
echo "VITE_APP_NAME=FishShop Admin" >> .env.development
echo "VITE_ENABLE_DEBUG=true" >> .env.development

# Chạy dev server
npm run dev
# → http://localhost:5174
```

### FE Customer
```bash
cd FE-Customer

# Cài dependencies (lần đầu)
npm install

# Copy .env.example thành .env.local
cp .env.example .env.local
# Chỉnh sửa VITE_API_URL nếu cần

# Chạy dev server
npm run dev
# → http://localhost:5173
```

---

## Cấu Trúc Port (Development)

```
┌────────────────────────────────────────┐
│         Development Ports              │
│                                        │
│  FE-Customer     → http://localhost:5173 │
│  FE-Admin        → http://localhost:5174 │
│                                        │
│  FishShop.API    → https://localhost:5001 │
│  Admin.API       → https://localhost:5002 │
│  API.Gateway     → http://localhost:8080  │
│                                        │
│  Oracle Database → localhost:1521/XE   │
└────────────────────────────────────────┘
```

---

## Thứ Tự Khởi Động Dev

```
1. 🗄️  Start Oracle Database
2. ⚙️  Start FishShop.API (port 5001)
3. ⚙️  Start FishShop.Admin.API (port 5002)
4. 🌐  Start FE-Customer (port 5173)  -- cho khách
5. 🌐  Start FE-Admin (port 5174)     -- cho admin
```

---

## Tài Khoản Demo (Seed Data)

Sau khi chạy schema + seed data:

| Username | Password | Role |
|----------|----------|------|
| `admin` | `Admin@123456` | ADMIN |
| `staff1` | `Staff@123456` | STAFF |

> Seed data cho USERS sẽ được tạo trong `database/seed_data.sql` (cần tạo thêm)

---

## Build Production

### Backend
```bash
# Publish FishShop.API
cd API/Src/FishShop.API
dotnet publish -c Release -o ./publish

# Publish Admin.API
cd API/Src/FishShop.Admin.API
dotnet publish -c Release -o ./publish
```

### Frontend
```bash
# FE Admin
cd FE
npm run build
# Output: ./dist/

# FE Customer
cd FE-Customer
npm run build
# Output: ./dist/
```

---

## Scripts Tiện Ích

### Kiểm tra TypeScript (FE)
```bash
# FE Admin
cd FE && npm run type-check

# FE Customer
cd FE-Customer && npm run type-check
```

### Lint (FE)
```bash
npm run lint        # Kiểm tra lỗi
npm run lint:fix    # Tự sửa lỗi có thể sửa
```

### dotnet watch (BE — hot reload)
```bash
cd API/Src/FishShop.API
dotnet watch run
```

---

## Kiểm Tra Sức Khỏe (Health Check)

Sau khi implement:
```
GET http://localhost:5001/health    → {"status": "Healthy"}
GET http://localhost:5002/health    → {"status": "Healthy"}
GET http://localhost:8080/health    → Gateway health
```

---

## Lưu Ý Quan Trọng

> [!IMPORTANT]
> **Oracle Case Sensitivity**: Oracle mặc định uppercase table/column names.
> EF Core phải map chính xác: `builder.ToTable("PRODUCTS")`, `HasColumnName("SALE_PRICE")`.

> [!WARNING]
> **Secret Key JWT**: Không commit secret key vào git.
> Dùng `appsettings.Development.json` (đã trong `.gitignore`) hoặc User Secrets.

> [!NOTE]
> **CORS**: FE chạy ở port 5173/5174 phải được whitelist trong BE.
> Kiểm tra `appsettings.json` mục `Cors.AllowedOrigins`.

> [!TIP]
> **Swagger UI**: Truy cập `https://localhost:5001/swagger` để test API trực tiếp
> mà không cần FE trong quá trình phát triển BE.
