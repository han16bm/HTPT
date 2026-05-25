# Hướng Dẫn Test Các Tính Năng Đã Triển Khai

Tài liệu này hướng dẫn cách kiểm tra **5 tính năng nâng cao** đã thêm vào project FishShop:

1. Centralized logging với CorrelationId
2. Distributed tracing với OpenTelemetry + Jaeger
3. Circuit breaker + retry với Polly
4. Saga pattern với MassTransit state machine
5. Dashboard giám sát Prometheus + Grafana

---

## 0. Chuẩn bị trước khi test

### 0.1. Yêu cầu môi trường

| Công cụ | Cách kiểm tra | Ghi chú |
|---|---|---|
| Docker Desktop | `docker version` | Phải running |
| SQL Server 2022 | `Get-Service MSSQLSERVER` | Status = Running |
| SQL Server TCP/IP | `Test-NetConnection localhost -Port 1433` | Phải reachable. Nếu chưa, xem mục 0.3 |
| Tài khoản `sa` | `sqlcmd -S localhost -U sa -P viet123 -Q "SELECT 1"` | Phải login được |
| Node.js 18+ | `node --version` | Để chạy FE (optional) |
| sqlcmd CLI | `sqlcmd -?` | Để chạy seed |

### 0.2. Khởi động hệ thống

Từ thư mục gốc `HTPT/`:

```powershell
docker compose up --build -d
```

Đợi ~30 giây cho tất cả service ổn định. Kiểm tra 10 container đều `Up`:

```powershell
docker compose ps
```

Phải thấy:
- `rabbitmq-service` — queue cho saga
- `seq-service` — log centralized
- `jaeger-service` — distributed tracing
- `prometheus-service` — metrics scraping
- `grafana-service` — dashboard
- `api-user`, `api-product`, `api-order`, `api-content` — 4 microservice
- `fishshop-gateway` — API Gateway

### 0.3. Bật SQL Server TCP/IP (nếu chưa)

Nếu `Test-NetConnection localhost -Port 1433` trả `False`, mở **PowerShell as Administrator** chạy:

```powershell
$base = "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQLServer\SuperSocketNetLib\Tcp"
Set-ItemProperty -Path $base -Name Enabled -Value 1
Set-ItemProperty -Path "$base\IPAll" -Name TcpPort -Value "1433"
Set-ItemProperty -Path "$base\IPAll" -Name TcpDynamicPorts -Value ""
Get-ChildItem $base | Where-Object { $_.PSChildName -like "IP*" -and $_.PSChildName -ne "IPAll" } | ForEach-Object {
    Set-ItemProperty -Path $_.PSPath -Name Enabled -Value 1 -ErrorAction SilentlyContinue
}
Restart-Service -Name MSSQLSERVER -Force
```

Nếu `sa/viet123` không login được, cũng cần bật Mixed Mode auth (xem `report/` history hoặc README cũ).

### 0.4. Seed database

```powershell
sqlcmd -S localhost -U sa -P viet123 -i .\API\database\sqlserver_full_setup.sql
```

Sau đó set stock cho test:

```powershell
sqlcmd -S localhost -U sa -P viet123 -d FishShop_Product -Q "UPDATE PRODUCTS SET STOCK_QUANTITY=50, SOLD_QUANTITY=0 WHERE ID IN (1,2,3,4)"
```

### 0.5. Tài khoản demo

| Vai trò | Username | Password |
|---|---|---|
| Admin | `admin` | `123456` |
| Khách hàng | `customer01` | `123456` |

### 0.6. URLs các UI

```
Gateway        http://localhost:5000
Grafana        http://localhost:3002     (admin/admin)
Prometheus     http://localhost:9090
Jaeger         http://localhost:16686
Seq logs       http://localhost:5341     (admin/admin123)
RabbitMQ       http://localhost:15672    (guest/guest)
```

---

## 1. Centralized Logging — CorrelationId

### 1.1. Nó làm gì?

Mỗi request HTTP đi qua Gateway sẽ được gắn một **mã định danh duy nhất** (CorrelationId) — kiểu `abc123def456...`. Mã này được:

- Đẩy vào response header `X-Correlation-Id` (client thấy lại)
- Forward sang các service downstream qua HTTP header
- Nhét vào header của RabbitMQ message khi publish event
- Tự động in vào **mọi log line** của mọi service (qua Serilog `LogContext`)

→ Khi 1 request gặp lỗi, ta có thể tìm trong Seq tất cả log liên quan (kể cả ở service B mà service A gọi qua HTTP hoặc message queue) chỉ bằng cách filter theo CorrelationId.

### 1.2. Code ở đâu?

| File | Vai trò |
|---|---|
| `API/netcore/netcore.Commons/Correlation/CorrelationIdMiddleware.cs` | Middleware ASP.NET — đọc/sinh CorrelationId từ request, push vào LogContext |
| `API/netcore/netcore.Commons/Correlation/CorrelationIdHandler.cs` | `DelegatingHandler` — tự gắn header vào outbound HttpClient |
| `API/netcore/netcore.Commons/Correlation/CorrelationIdPublishFilter.cs` | MassTransit filter — gắn CorrelationId vào header RabbitMQ message |
| `API/netcore/netcore.Commons/Correlation/CorrelationIdConsumeFilter.cs` | MassTransit filter — đọc CorrelationId từ message, push vào LogContext |
| `API/netcore/netcore.Commons/Correlation/CorrelationIdExtensions.cs` | `services.AddCorrelationId()` + `app.UseCorrelationId()` |

Đăng ký trong từng `Program.cs`:
- `API/Gateway/FishShop.Gateway/Program.cs`
- `API/Services/API.User/Program.cs`
- `API/Services/API.Product/Program.cs`
- `API/Services/API.Order/Program.cs`
- `API/Services/API.Content/Program.cs`

### 1.3. Cách test bằng tay

**Test 1: Gửi request có sẵn CorrelationId**

```powershell
$cid = "my-test-cid-001"
$r = Invoke-WebRequest -Uri 'http://localhost:5000/api/user/auth/health' -Headers @{ 'X-Correlation-Id' = $cid } -UseBasicParsing
"Sent CID: $cid"
"Recv CID: $($r.Headers['X-Correlation-Id'])"
```

Kết quả mong đợi: response header `X-Correlation-Id` trả về **đúng cùng giá trị** `my-test-cid-001`.

**Test 2: Không gửi CorrelationId → server tự sinh**

```powershell
$r = Invoke-WebRequest -Uri 'http://localhost:5000/api/product/products/health' -UseBasicParsing
"Generated CID: $($r.Headers['X-Correlation-Id'])"
```

Kết quả: response trả về 1 GUID 32 ký tự không có dấu `-`.

**Test 3: Filter log Seq theo CorrelationId**

1. Mở http://localhost:5341
2. Login `admin / admin123`
3. Trong filter bar gõ:
   ```
   CorrelationId = 'my-test-cid-001'
   ```
4. Sẽ thấy mọi log line (cả của Gateway, API.User, API.Product, …) có cùng CorrelationId.

Có thể xem trực tiếp trong console log:

```powershell
docker logs api-order --tail 50 | Select-String "cid:my-test-cid-001"
```

### 1.4. Verify CorrelationId xuyên qua RabbitMQ

Đây là phần quan trọng — chứng minh CorrelationId không chỉ qua HTTP mà còn qua message queue async:

```powershell
# Gửi 1 đơn hàng để trigger event
$cid = "trace-mq-" + (Get-Date -Format "HHmmss")
# (xem ví dụ đầy đủ ở mục Saga bên dưới)
```

Sau đó:

```powershell
docker logs api-order --tail 100 | Select-String "cid:$cid"
docker logs api-product --tail 100 | Select-String "cid:$cid"
```

→ Cả hai service đều có log dán cùng `cid:$cid`, dù API.Product nhận event qua RabbitMQ chứ không phải HTTP từ API.Order.

---

## 2. Distributed Tracing — OpenTelemetry + Jaeger

### 2.1. Nó làm gì?

Khác với log (in từng dòng riêng lẻ), **trace** ghi lại **toàn bộ một request** như 1 cây span:

```
[Gateway] HTTP POST /api/order/orders  (350ms)
  └─[API.Order] OrdersController.Create  (340ms)
      ├─[SQL] INSERT INTO ORDERS  (15ms)
      ├─[HTTP] GET api-product:8080/api/product/products/1  (20ms)
      │   └─[API.Product] ProductsController.GetById  (18ms)
      └─[RabbitMQ] publish OrderCreatedEvent  (5ms)
```

→ Khi 1 request chậm, ta thấy ngay điểm nghẽn nằm ở đâu (DB, HTTP call, hay message broker).

### 2.2. Code ở đâu?

| File | Vai trò |
|---|---|
| `API/netcore/netcore.Commons/Observability/OpenTelemetryExtensions.cs` | Cấu hình OTel: ASP.NET, HttpClient, SqlClient, MassTransit |
| `docker-compose.yml` (service `jaeger`) | Container chạy Jaeger all-in-one + nhận OTLP gRPC port 4317 |

Đăng ký trong từng `Program.cs`:
```csharp
services.AddDistributedTracing(configuration, "API.User");
```

### 2.3. Cách test bằng tay

**Bước 1: Generate traffic**

```powershell
# Gọi vài lần
for ($i = 1; $i -le 5; $i++) {
    Invoke-WebRequest -Uri 'http://localhost:5000/api/product/products?page=1&pageSize=5' -UseBasicParsing | Out-Null
    Invoke-WebRequest -Uri 'http://localhost:5000/api/content/blogs' -UseBasicParsing | Out-Null
}
"Đã sinh traffic"
```

**Bước 2: Mở Jaeger UI**

http://localhost:16686

**Bước 3: Verify**

1. Mở dropdown **Service**, phải thấy:
   - `Gateway`
   - `API.User`
   - `API.Product`
   - `API.Order`
   - `API.Content`
2. Chọn `Gateway` → click **Find Traces**
3. Trên danh sách trace, click 1 trace bất kỳ → thấy cây span:
   - Top: `Gateway POST /api/product/products` (vài chục ms)
   - Bên dưới: span của `API.Product` (proxy tới downstream)
   - Bên trong API.Product: span SQL query (EF Core), span outbound HTTP nếu có

**Bước 4: Kiểm tra trace của 1 saga đầy đủ**

Sau khi chạy 1 đơn hàng (xem mục 4):

1. Mở Jaeger, chọn service `API.Order`
2. Tags filter: `correlation_id="<your_cid>"`
3. Click trace → thấy span của toàn bộ flow: HTTP create order → DB insert → publish RabbitMQ → consume ở Product → reserve stock → publish event reply → …

### 2.4. Verify metrics + correlation gắn vào trace

Mỗi span tự động enrich:
- Tag `correlation_id` = CorrelationId của request
- Tag `user.id` = giá trị header `X-User-Id` (nếu có)
- Tag `http.method`, `http.status_code`, `db.statement` (SQL câu lệnh thật sự chạy)

Trong Jaeger UI, click vào 1 span → tab **Tags** → kiểm tra các thông tin trên.

### 2.5. Endpoint bị filter (không trace)

Để Jaeger không bị spam, các endpoint sau **không** được trace:
- `/swagger/*`
- `/assets/*`
- `/health` (mọi service)
- `/metrics` (Prometheus scrape)

Logic này ở `OpenTelemetryExtensions.ShouldTraceRequest()`.

---

## 3. Circuit Breaker + Retry — Polly

### 3.1. Nó làm gì?

Áp `Microsoft.Extensions.Http.Resilience` (Polly v8) cho **mọi HttpClient** trong cả 5 service. Khi 1 outbound HTTP call:

- **Bị timeout / lỗi 5xx / lỗi network**: tự retry tối đa 3 lần với exponential backoff + jitter (~500ms, 1s, 2s)
- **Lỗi liên tục > 50% trong 30s, ít nhất 10 sample**: circuit breaker mở (open) 5 giây — mọi call tiếp theo fail nhanh không gọi đến service đang chết
- **Per-attempt timeout**: 10 giây/lần thử
- **Total request timeout**: 45 giây tổng

### 3.2. Code ở đâu?

| File | Vai trò |
|---|---|
| `API/netcore/netcore.Commons/Resilience/ResilienceExtensions.cs` | Cấu hình `AddStandardResilienceHandler` qua `ConfigureHttpClientDefaults` |

Đăng ký trong `Program.cs`:
```csharp
services.AddResilientHttpClient();
```

Vì dùng `ConfigureHttpClientDefaults`, **mọi** HttpClient (default + named + typed) đều có resilience tự động.

### 3.3. Cách test bằng tay

**Test 1: Quan sát retry chain trong log**

```powershell
# Gọi endpoint /orders (Gateway → API.Order → gọi HTTP API.Product để lấy thông tin)
Invoke-WebRequest -Uri 'http://localhost:5000/api/order/orders/health' -UseBasicParsing | Out-Null

# Xem log
docker logs api-order --tail 30 | Select-String "Execution attempt"
```

Mỗi outbound HTTP call sẽ có log:
```
Execution attempt. Source: '-standard//Standard-Retry', Operation Key: 'null',
Result: '200', Handled: 'False', Attempt: '0', Execution Time: 18.13ms
```
- `Attempt: 0` = lần thử đầu tiên
- `Handled: False` = không retry (vì 200 OK)

**Test 2: Force circuit breaker (giả lập service chết)**

Dừng `api-product`:

```powershell
docker stop api-product
```

Gọi liên tục endpoint cần Product:

```powershell
1..15 | ForEach-Object {
    try {
        Invoke-WebRequest -Uri 'http://localhost:5000/api/product/products/1' -UseBasicParsing -TimeoutSec 5
    } catch {
        "Attempt $_`: $($_.Exception.Message)"
    }
}
```

Quan sát: vài request đầu sẽ retry rồi fail. Sau ~10 sample fail, các request sau **fail ngay lập tức** (không chờ retry) — đó là circuit breaker mở.

Khởi động lại:

```powershell
docker start api-product
```

Sau ~5 giây (BreakDuration), circuit về half-open rồi closed nếu request mới thành công.

### 3.4. Verify config

Xem `API/netcore/netcore.Commons/Resilience/ResilienceExtensions.cs`:

```csharp
options.Retry.MaxRetryAttempts = 3;            // tối đa 3 lần thử lại
options.Retry.Delay = TimeSpan.FromMilliseconds(500);
options.Retry.BackoffType = DelayBackoffType.Exponential;
options.Retry.UseJitter = true;

options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
options.CircuitBreaker.FailureRatio = 0.5;     // 50% lỗi
options.CircuitBreaker.MinimumThroughput = 10; // ít nhất 10 sample
options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);
```

---

## 4. Saga Pattern — Event-driven workflow

### 4.1. Nó làm gì?

Khi customer đặt 1 đơn hàng, hệ thống cần làm nhiều bước trên nhiều service khác nhau:

1. Tạo Order (API.Order, DB FishShop_Order)
2. Reserve kho (API.Product, DB FishShop_Product)
3. Xử lý thanh toán (mock gateway trong API.Order)
4. Confirm hoặc rollback

**Saga** điều phối các bước này bằng các event qua RabbitMQ. Nếu bước nào fail (ví dụ payment fail) thì saga gửi event **compensation** để các bước trước rollback (ví dụ hoàn kho).

**Flow chính:**

```
[1] OrderCreatedEvent
       │
       ▼
[2] Saga (state=AwaitingInventory) publish InventoryReservationRequested
       │
       ▼
[3] API.Product consume → reserve kho → publish InventoryReservedEvent
                                     ↘ (nếu fail) publish InventoryReservationFailedEvent
       │
       ▼
[4] Saga (state=AwaitingPayment) publish PaymentProcessRequested
       │
       ▼
[5] API.Order consume (mock gateway) → publish PaymentProcessedEvent / PaymentFailedEvent
       │
       ▼
[6a] Nếu Payment OK:
     Saga → state=Completed → publish OrderCompletedEvent
     OrderCompletedConsumer → cập nhật Order = "CONFIRMED"

[6b] Nếu Payment FAIL:
     Saga → state=Compensating → publish InventoryReleaseRequested
     API.Product consume → hoàn kho → publish InventoryReleasedEvent
     Saga → state=Failed → publish OrderFailedEvent
     OrderFailedConsumer → cập nhật Order = "CANCELLED"
```

### 4.2. Code ở đâu?

| File | Vai trò |
|---|---|
| `API/netcore/netcore.Commons/Messages/Events/OrderCreatedEvent.cs` | Event khởi tạo |
| `API/netcore/netcore.Commons/Messages/Events/SagaEvents.cs` | 10 event saga |
| `API/Services/API.Order/Saga/OrderSagaState.cs` | State của 1 instance saga (lưu OrderCode, Items, …) |
| `API/Services/API.Order/Saga/OrderStateMachine.cs` | State machine: định nghĩa state + transition |
| `API/Services/API.Order/Consumers/PaymentProcessConsumer.cs` | Mock payment gateway |
| `API/Services/API.Order/Consumers/OrderOutcomeConsumers.cs` | Update Order khi saga complete/fail |
| `API/Services/API.Product/Consumers/InventoryReservationConsumer.cs` | Reserve stock |
| `API/Services/API.Product/Consumers/InventoryReleaseConsumer.cs` | Hoàn stock (compensation) |
| `API/Services/API.Order/Extensions/ServiceRegistrationExtensions.cs` | Đăng ký saga + consumer |

### 4.3. Mock payment rules (trong `PaymentProcessConsumer.SimulateGateway`)

| Phương thức | Kết quả |
|---|---|
| `COD`, `CASH` | Luôn pass, payment status = `PENDING` (thu khi giao) |
| `BANK_TRANSFER` | Pass, status = `AWAITING` (chờ admin duyệt) |
| `VNPAY`, `MOMO` | 90% pass (status = `PAID`), 10% fail |
| Khác (ví dụ `INVALID_METHOD`) | Luôn fail → kích hoạt compensation |

### 4.4. Cách test — Saga success path (COD)

Tạo file `test-saga-cod.ps1` hoặc copy đoạn này vào PowerShell:

```powershell
# Reset stock
sqlcmd -S localhost -U sa -P viet123 -d FishShop_Product -Q "UPDATE PRODUCTS SET STOCK_QUANTITY=50, SOLD_QUANTITY=0 WHERE ID IN (1,2,3,4)" | Out-Null

Add-Type -AssemblyName System.Net.Http
$cid = "test-cod-" + (Get-Date -Format "HHmmss")
$client = New-Object System.Net.Http.HttpClient
$client.DefaultRequestHeaders.Add('X-Correlation-Id', $cid)

# Login
$loginBody = '{"username":"customer01","password":"123456"}'
$login = $client.PostAsync('http://localhost:5000/api/user/auth/login',
    (New-Object System.Net.Http.StringContent($loginBody, [Text.Encoding]::UTF8, 'application/json'))).Result
$token = ($login.Content.ReadAsStringAsync().Result | ConvertFrom-Json).data.accessToken
$client.DefaultRequestHeaders.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue('Bearer', $token)
"[1] Login OK"

# Clear cart + add product
$null = $client.DeleteAsync('http://localhost:5000/api/order/cart').Result
$cartBody = '{"productId":1,"quantity":3,"productName":"Ca Betta","unitPrice":50000,"imageUrl":"x.jpg"}'
$null = $client.PostAsync('http://localhost:5000/api/order/cart/items',
    (New-Object System.Net.Http.StringContent($cartBody, [Text.Encoding]::UTF8, 'application/json'))).Result
"[2] Cart add 3x product 1 (Ca Betta 50000)"

# Place order with COD
$orderBody = '{"paymentMethod":"COD","shippingFee":0,"note":"Test","shippingAddress":"HN|0900000000","customerName":"T","customerPhone":"0900000000"}'
$orderResp = $client.PostAsync('http://localhost:5000/api/order/orders',
    (New-Object System.Net.Http.StringContent($orderBody, [Text.Encoding]::UTF8, 'application/json'))).Result
$orderJson = $orderResp.Content.ReadAsStringAsync().Result | ConvertFrom-Json
$orderCode = $orderJson.data.orderCode
"[3] Order placed: $orderCode (initial status = $($orderJson.data.orderStatus))"

# Wait for saga
"[4] Wait 8s for saga..."
Start-Sleep -Seconds 8

# Check final state
$detail = ($client.GetStringAsync("http://localhost:5000/api/order/orders/$orderCode").Result | ConvertFrom-Json).data
"[5] Final: status=$($detail.orderStatus) payment=$($detail.paymentStatus)"

$p1 = ($client.GetStringAsync('http://localhost:5000/api/product/products/1').Result | ConvertFrom-Json).data
"[6] Stock product 1: $($p1.stockQuantity) (was 50, expect 47)  sold=$($p1.soldQuantity)"

"=== CorrelationId: $cid ==="
```

**Kết quả mong đợi:**
- Order status: `PENDING → CONFIRMED`
- Payment status: `PENDING` (COD thu khi giao)
- Stock: 50 → **47** (giảm 3)
- Sold: 0 → **3**

### 4.5. Cách test — Saga compensation path (payment fail)

```powershell
# Reset
sqlcmd -S localhost -U sa -P viet123 -d FishShop_Product -Q "UPDATE PRODUCTS SET STOCK_QUANTITY=50, SOLD_QUANTITY=0 WHERE ID=3" | Out-Null

Add-Type -AssemblyName System.Net.Http
$cid = "compensate-" + (Get-Date -Format "HHmmss")
$client = New-Object System.Net.Http.HttpClient
$client.DefaultRequestHeaders.Add('X-Correlation-Id', $cid)

$loginBody = '{"username":"customer01","password":"123456"}'
$token = ($client.PostAsync('http://localhost:5000/api/user/auth/login',
    (New-Object System.Net.Http.StringContent($loginBody, [Text.Encoding]::UTF8, 'application/json'))).Result.Content.ReadAsStringAsync().Result | ConvertFrom-Json).data.accessToken
$client.DefaultRequestHeaders.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue('Bearer', $token)

$null = $client.DeleteAsync('http://localhost:5000/api/order/cart').Result
$null = $client.PostAsync('http://localhost:5000/api/order/cart/items',
    (New-Object System.Net.Http.StringContent('{"productId":3,"quantity":4,"productName":"May bom","unitPrice":65000,"imageUrl":"x.jpg"}', [Text.Encoding]::UTF8, 'application/json'))).Result

# paymentMethod = INVALID_METHOD => mock gateway always fails => compensation
$orderResp = $client.PostAsync('http://localhost:5000/api/order/orders',
    (New-Object System.Net.Http.StringContent('{"paymentMethod":"INVALID_METHOD","shippingFee":0,"note":"Compensation","shippingAddress":"HN|0900000000","customerName":"T","customerPhone":"0900000000"}', [Text.Encoding]::UTF8, 'application/json'))).Result
$orderCode = ($orderResp.Content.ReadAsStringAsync().Result | ConvertFrom-Json).data.orderCode
"[1] Order placed: $orderCode (will fail payment)"

Start-Sleep -Seconds 10

$detail = ($client.GetStringAsync("http://localhost:5000/api/order/orders/$orderCode").Result | ConvertFrom-Json).data
"[2] Final: status=$($detail.orderStatus)  note=$($detail.notes)"

$p3 = ($client.GetStringAsync('http://localhost:5000/api/product/products/3').Result | ConvertFrom-Json).data
"[3] Stock product 3: $($p3.stockQuantity) (was 50, expect 50 after release)  sold=$($p3.soldQuantity) (expect 0)"

"=== CorrelationId: $cid ==="
```

**Kết quả mong đợi:**
- Order status: `CANCELLED`
- Order note: chứa `"Saga: Phương thức không hỗ trợ: INVALID_METHOD"`
- Stock product 3: **50** (reserve rồi release lại) — không đổi
- Sold: **0** — không đổi

### 4.6. Verify saga flow trong log

Sau khi chạy test compensation ở trên, lấy CorrelationId in ra cuối log rồi:

```powershell
$cid = "<CID_in_kèm_log>"   # ví dụ: compensate-170647
"=== api-order log ==="
docker logs api-order --tail 200 2>&1 | Select-String "cid:$cid" | Select-String "Saga|Order created|process payment|failed"

"=== api-product log ==="
docker logs api-product --tail 200 2>&1 | Select-String "cid:$cid" | Select-String "Saga|reserve|release|kho"
```

Phải thấy chuỗi event đúng thứ tự:
```
api-order:   Order created: DH...
api-product: Saga reserve inventory: DH...
api-product: Đã xuất kho cho đơn hàng DH...
api-order:   Saga process payment: DH... method=INVALID_METHOD
api-product: Saga release inventory: DH...
api-product: Đã hoàn kho cho đơn hàng DH...
api-order:   Saga failed: DH... -> CANCELLED
```

### 4.7. Verify queue RabbitMQ

http://localhost:15672 (guest/guest) → tab **Queues**

Phải thấy các queue do saga tạo:
- `order-saga` — saga state machine
- `inventory-reservation-queue` — reserve consumer
- `inventory-release-queue` — release consumer
- `payment-process-queue` — payment consumer
- `order-completed-queue`, `order-failed-queue` — DB updaters

Mỗi queue ở trạng thái idle (0 message) sau khi saga complete — nghĩa là không có message stuck.

---

## 5. Dashboard giám sát — Prometheus + Grafana

### 5.1. Nó làm gì?

**Prometheus** scrape (cào) metrics từ endpoint `/metrics` của 5 service mỗi 15 giây. **Grafana** đọc Prometheus và hiển thị trên dashboard với 12 panel.

Khác với log (kể chuyện) và trace (đo thời gian 1 request), **metrics** trả lời câu hỏi "**lúc này hệ thống thế nào?**":
- Tốc độ request (req/s)
- Tỉ lệ lỗi
- Latency p50/p95/p99 theo thời gian
- Số request đang xử lý (in-flight)
- Bộ nhớ heap .NET, GC collections, threadpool

### 5.2. Code & config ở đâu?

| File | Vai trò |
|---|---|
| `API/netcore/netcore.Commons/Observability/OpenTelemetryExtensions.cs` (block `ConfigureMetrics`) | Đăng ký metrics instrument + Prometheus exporter |
| `infra/prometheus/prometheus.yml` | Scrape config — 5 service target |
| `infra/grafana/provisioning/datasources/datasource.yml` | Auto-provision Prometheus datasource |
| `infra/grafana/provisioning/dashboards/dashboard.yml` | Auto-load dashboard từ folder |
| `infra/grafana/dashboards/fishshop-overview.json` | Dashboard JSON (12 panel) |
| `docker-compose.yml` (service `prometheus` + `grafana`) | Container |

### 5.3. Cách test — kiểm tra Prometheus scrape

**Bước 1: Mở Prometheus targets**

http://localhost:9090/targets

Phải thấy **6 target** đều `UP`:
- `fishshop-gateway` (Gateway)
- `api-user`, `api-product`, `api-order`, `api-content`
- `prometheus` (self)

Nếu thấy `DOWN` → click vào để xem lý do (thường là service chưa start xong).

**Bước 2: Query metrics trực tiếp**

Vào http://localhost:9090/graph, gõ:

```promql
sum by (service) (rate(http_server_request_duration_seconds_count[1m]))
```

→ Hiển thị request rate (req/s) chia theo service.

Vài query hữu ích khác:

```promql
# Latency p95 ms theo service
1000 * histogram_quantile(0.95, sum by (service, le) (rate(http_server_request_duration_seconds_bucket[5m])))

# Số request 4xx/5xx mỗi giây
sum by (service, http_response_status_code) (rate(http_server_request_duration_seconds_count{http_response_status_code=~"4..|5.."}[1m]))

# Heap memory MB
process_runtime_dotnet_gc_heap_size_bytes / 1024 / 1024
```

### 5.4. Cách test — Grafana dashboard

**Bước 1: Mở Grafana**

http://localhost:3002 — login `admin / admin` (lần đầu sẽ hỏi đổi password, bấm Skip)

**Bước 2: Mở dashboard**

Sidebar trái → **Dashboards** → folder **FishShop** → **Service Overview**

Hoặc URL trực tiếp: http://localhost:3002/d/fishshop-overview

**Bước 3: Generate traffic + xem real-time**

Mở dashboard, đổi time range thành **Last 15 minutes**, refresh `10s`. Trong PowerShell:

```powershell
1..100 | ForEach-Object {
    Invoke-WebRequest -Uri 'http://localhost:5000/api/product/products?page=1&pageSize=5' -UseBasicParsing | Out-Null
    Invoke-WebRequest -Uri 'http://localhost:5000/api/content/blogs' -UseBasicParsing | Out-Null
    Start-Sleep -Milliseconds 200
}
```

Quan sát panel **Request rate** trên Grafana → đường biểu đồ nhảy lên.

**Bước 4: Test filter theo service**

Trên top dashboard có dropdown **service**. Chọn 1 service cụ thể (ví dụ `API.Product`) → tất cả panel update chỉ hiển thị service đó.

### 5.5. 12 Panel của dashboard

| Panel | Ý nghĩa |
|---|---|
| Request rate (req/s) | Lượng request mỗi giây theo service |
| Error rate | Số request 4xx + 5xx mỗi giây |
| Latency p50 (ms) | 50% request nhanh hơn giá trị này |
| Latency p95 | 95% request nhanh hơn — quan trọng nhất |
| Latency p99 | 99% — tail latency |
| Outbound HTTP rate | Service A gọi service B bao nhiêu req/s |
| Outbound HTTP errors | Lỗi khi gọi service khác |
| .NET Heap Memory (MB) | Bộ nhớ managed heap theo generation |
| GC Collections / min | Số lần GC chạy/phút (cao = pressure memory) |
| ThreadPool active threads | Số thread đang dùng |
| Active HTTP requests in-flight | Số request đang xử lý chưa trả response |
| Request rate by status code | Phân bố 200/201/4xx/5xx |

### 5.6. Verify metrics endpoint trực tiếp

```powershell
# Lấy metrics raw từ container
docker exec prometheus-service wget -qO- --timeout=5 http://api-order:8080/metrics | Select-Object -First 30
```

Sẽ thấy hàng trăm dòng kiểu:
```
# TYPE http_server_request_duration_seconds histogram
http_server_request_duration_seconds_bucket{...,le="0.005"} 12
http_server_request_duration_seconds_bucket{...,le="0.01"}  18
process_runtime_dotnet_gc_heap_size_bytes{generation="gen0"} 1234567
...
```

Đây chính là payload mà Prometheus đọc.

---

## 6. Combo demo end-to-end

Sau khi đã hiểu từng phần, có thể demo tổng:

```powershell
# 1. Reset state
sqlcmd -S localhost -U sa -P viet123 -d FishShop_Product -Q "UPDATE PRODUCTS SET STOCK_QUANTITY=50, SOLD_QUANTITY=0 WHERE ID IN (1,2,3,4)" | Out-Null

# 2. Generate traffic (xem trên Grafana)
1..30 | ForEach-Object { Invoke-WebRequest -Uri 'http://localhost:5000/api/product/products?page=1&pageSize=5' -UseBasicParsing | Out-Null }

# 3. Đặt 1 đơn thành công (xem flow trên Jaeger + Seq filter CID)
$cid = "demo-" + (Get-Date -Format "HHmmss")
# … paste đoạn test ở mục 4.4 với CID=$cid …

# 4. Đặt 1 đơn fail (xem compensation trên log + RabbitMQ + Order DB)
# … paste đoạn test ở mục 4.5 …
```

Lúc này có thể mở 6 tab browser cùng lúc:
- Seq lọc theo CID — thấy log full flow
- Jaeger xem trace của đơn → click span thấy DB query, HTTP call
- Grafana xem traffic spike + latency
- RabbitMQ xem queue depth
- Prometheus query metric raw
- Gateway/API trả response

---

## 7. Troubleshooting

### Không kết nối được SQL Server từ container
**Lỗi**: `TCP Provider, error: 40`
**Fix**: Mục 0.3 — bật TCP/IP cho SQL Server + restart.

### Token 401 Unauthorized sau khi login
**Lỗi**: `Token không hợp lệ hoặc đã hết hạn`
**Cause**: Version skew giữa `Microsoft.IdentityModel.Tokens` và `JsonWebTokens`.
**Fix**: Đã pin `8.2.0` trong `FishShop.Gateway.csproj`. Rebuild bằng:
```powershell
docker compose up --build -d fishshop.gateway
```

### Saga không chạy / order mãi không CONFIRMED
**Check**:
1. `docker logs api-order --tail 50` — xem có lỗi `R-FAULT` hoặc exception
2. RabbitMQ UI xem queue `order-saga` có message stuck không
3. Có thể MassTransit chưa wire saga: kiểm tra `API/Services/API.Order/Extensions/ServiceRegistrationExtensions.cs` có `AddSagaStateMachine<OrderStateMachine, OrderSagaState>().InMemoryRepository()`

### Prometheus target DOWN
**Check**:
1. Service đó còn chạy không: `docker compose ps`
2. Có route `/metrics` không: `docker exec prometheus-service wget -qO- http://api-order:8080/metrics`
3. Service có gọi `app.UseObservabilityEndpoints()` trong `Program.cs` không

### Grafana không thấy dashboard
**Check**:
1. Container đang chạy: `docker logs grafana-service --tail 30`
2. Volume mount đúng: file `infra/grafana/dashboards/fishshop-overview.json` phải tồn tại
3. Restart Grafana: `docker compose restart grafana`

### Jaeger không thấy service
**Cause**: Endpoint OTLP sai hoặc Jaeger chưa nhận trace.
**Check**:
1. Env `Otlp__Endpoint=http://jaeger:4317` trong `docker-compose.yml`
2. Generate request **không phải** /health (đã bị filter)
3. Đợi ~30s để batch trace flush

---

## 8. Reset / dọn dẹp

**Dừng all container** (giữ volume — data DB rabbit seq grafana còn nguyên):
```powershell
docker compose down
```

**Dừng + xóa volume** (mất hết — phải seed lại DB và mất history log/metric):
```powershell
docker compose down -v
```

**Rebuild 1 service cụ thể** (sau khi sửa code):
```powershell
docker compose up --build -d api.order
```

**Rebuild toàn bộ**:
```powershell
docker compose up --build -d
```

---

## 9. Tóm tắt 5 tính năng

| # | Tính năng | UI verify | Code chính |
|---|---|---|---|
| 1 | CorrelationId logging | Seq filter `CorrelationId = '…'` | `netcore.Commons/Correlation/` |
| 2 | Distributed tracing | Jaeger UI chọn service | `netcore.Commons/Observability/OpenTelemetryExtensions.cs` |
| 3 | Polly resilience | Log `Execution attempt` | `netcore.Commons/Resilience/ResilienceExtensions.cs` |
| 4 | Saga + compensation | Order DB + RabbitMQ + log chuỗi event | `API.Order/Saga/`, `API.Product/Consumers/` |
| 5 | Prometheus + Grafana | Dashboard `FishShop/Service Overview` | `infra/prometheus`, `infra/grafana`, `OpenTelemetryExtensions.ConfigureMetrics` |
