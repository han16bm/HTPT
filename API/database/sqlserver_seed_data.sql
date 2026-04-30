-- ==========================================================
-- SCRIPT SEED DATA DÀNH CHO SQL SERVER (Tất cả Microservices)
-- ==========================================================

-- ----------------------------------------------------------
-- 1. SEED DATA CHO MICROSERVICE USER
-- ----------------------------------------------------------
USE FishShop_User;
GO

SET IDENTITY_INSERT dbo.ROLES ON;
INSERT INTO dbo.ROLES (Id, Code, Name) VALUES 
(1, 'ADMIN', N'Quản trị viên'),
(2, 'CUSTOMER', N'Khách hàng');
SET IDENTITY_INSERT dbo.ROLES OFF;

SET IDENTITY_INSERT dbo.USERS ON;
-- Passwords below are '123456' hashed with BCrypt, but since the system may use its own hasher, 
-- we provide a dummy hash. Assuming the system uses BCrypt or standard Identity hasher.
INSERT INTO dbo.USERS (Id, Username, PasswordHash, Email, Phone, Status) VALUES 
(1, 'admin', '$2a$11$N5lTq8r9w/1H2x8wRb8nIedgTqL3p9E/3lF0U2/50a/8x1j0T2y1W', 'admin@fishshop.local', '0901234567', 1),
(2, 'customer01', '$2a$11$N5lTq8r9w/1H2x8wRb8nIedgTqL3p9E/3lF0U2/50a/8x1j0T2y1W', 'khachhang@gmail.com', '0901234569', 1);
SET IDENTITY_INSERT dbo.USERS OFF;

INSERT INTO dbo.USER_ROLES (UserId, RoleId) VALUES 
(1, 1),
(2, 2);

SET IDENTITY_INSERT dbo.PROFILES ON;
INSERT INTO dbo.PROFILES (Id, UserId, FullName, Gender) VALUES 
(1, 1, N'System Administrator', 'Male'),
(2, 2, N'Nguyễn Văn Khách', 'Male');
SET IDENTITY_INSERT dbo.PROFILES OFF;

-- ----------------------------------------------------------
-- 2. SEED DATA CHO MICROSERVICE PRODUCT
-- ----------------------------------------------------------
USE FishShop_Product;
GO

SET IDENTITY_INSERT dbo.CATEGORIES ON;
INSERT INTO dbo.CATEGORIES (Id, Name, Slug, Description, Status) VALUES 
(1, N'Cá Cảnh', 'ca-canh', N'Các loại cá cảnh đẹp', 1),
(2, N'Phụ Kiện', 'phu-kien', N'Phụ kiện hồ cá', 1),
(3, N'Thức Ăn', 'thuc-an', N'Thức ăn dinh dưỡng cho cá', 1);
SET IDENTITY_INSERT dbo.CATEGORIES OFF;

SET IDENTITY_INSERT dbo.PRODUCTS ON;
INSERT INTO dbo.PRODUCTS (Id, CategoryId, Name, Slug, Price, Description, ThumbImage, Status) VALUES 
(1, 1, N'Cá Betta Rồng Đỏ', 'ca-betta-rong-do', 50000.00, N'Cá Betta Rồng Đỏ cực đẹp', 'betta-red.jpg', 1),
(2, 1, N'Cá Bảy Màu Thái', 'ca-bay-mau-thai', 25000.00, N'Cá Bảy Màu Thái thuần chủng', 'guppy-thai.jpg', 1),
(3, 2, N'Máy bơm chìm 5W', 'may-bom-chim-5w', 65000.00, N'Bơm nước hồ cá loại nhỏ', 'bom-5w.jpg', 1),
(4, 3, N'Cám cá Tetra Color 50g', 'cam-ca-tetra-color-50g', 85000.00, N'Thức ăn lên màu cho cá', 'tetra-color.jpg', 1);
SET IDENTITY_INSERT dbo.PRODUCTS OFF;

INSERT INTO dbo.INVENTORY (ProductId, StockQuantity, ReservedQuantity, WarehouseLocation) VALUES 
(1, 100, 0, 'A1-01'),
(2, 500, 0, 'A1-02'),
(3, 50, 0, 'B1-01'),
(4, 200, 0, 'C1-01');

-- ----------------------------------------------------------
-- 3. SEED DATA CHO MICROSERVICE ORDER
-- ----------------------------------------------------------
USE FishShop_Order;
GO

SET IDENTITY_INSERT dbo.PROMOTIONS ON;
INSERT INTO dbo.PROMOTIONS (Id, Code, DiscountType, DiscountValue, StartDate, EndDate, Status) VALUES 
(1, 'WELCOME', 'PERCENT', 10.00, DATEADD(day, -1, GETDATE()), DATEADD(year, 1, GETDATE()), 1),
(2, 'FREESHIP', 'AMOUNT', 30000.00, DATEADD(day, -1, GETDATE()), DATEADD(year, 1, GETDATE()), 1);
SET IDENTITY_INSERT dbo.PROMOTIONS OFF;

-- ----------------------------------------------------------
-- 4. SEED DATA CHO MICROSERVICE CONTENT
-- ----------------------------------------------------------
USE FishShop_Content;
GO

SET IDENTITY_INSERT dbo.BLOG_CATEGORIES ON;
INSERT INTO dbo.BLOG_CATEGORIES (Id, Name, Slug) VALUES 
(1, N'Kiến Thức Nuôi Cá', 'kien-thuc-nuoi-ca'),
(2, N'Tin Tức Khuyến Mãi', 'tin-tuc-khuyen-mai');
SET IDENTITY_INSERT dbo.BLOG_CATEGORIES OFF;

SET IDENTITY_INSERT dbo.BLOGS ON;
INSERT INTO dbo.BLOGS (Id, CategoryId, AuthorId, Title, Slug, Summary, Content, Status) VALUES 
(1, 1, 1, N'Cách chăm sóc cá Betta cho người mới', 'cach-cham-soc-ca-betta', N'Hướng dẫn chi tiết từ A-Z', N'<p>Cá Betta là loài cá rất dễ nuôi nhưng cần lưu ý chất lượng nước...</p>', 'Published'),
(2, 2, 1, N'Khuyến mãi tháng 5 giảm giá toàn bộ', 'khuyen-mai-thang-5', N'Cơ hội mua sắm thả ga', N'<p>Nhập mã WELCOME để được giảm giá...</p>', 'Published');
SET IDENTITY_INSERT dbo.BLOGS OFF;

PRINT 'Seed data cho SQL Server da duoc tao thanh cong!';
GO
