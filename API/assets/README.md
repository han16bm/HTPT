# API Assets

Thư mục này lưu ảnh upload dùng chung cho backend API, chia theo từng service.

```text
API/assets/
|-- user/       # Ảnh hồ sơ, avatar người dùng
|-- product/    # Ảnh sản phẩm, danh mục, gallery
|-- order/      # Ảnh/chứng từ liên quan đơn hàng nếu cần mở rộng
+-- content/    # Ảnh blog, nội dung, liên hệ
```

Ứng dụng đang phục vụ ảnh qua các đường dẫn:

```text
/api/user/assets
/api/product/assets
/api/content/assets
```

Không commit file ảnh upload thật. Chỉ giữ `.gitkeep` để tạo sẵn cấu trúc thư mục.
