-- ==========================================================
-- SCRIPT SEED DATA DÀNH CHO SQL SERVER (Tất cả Microservices)
-- Mật khẩu mặc định cho tất cả user: 123456
-- ==========================================================

-- ----------------------------------------------------------
-- 1. SEED DATA CHO MICROSERVICE USER
-- ----------------------------------------------------------
USE FishShop_User;
GO

-- Xóa dữ liệu cũ theo thứ tự FK
DELETE FROM dbo.CUSTOMER_ADDRESSES;
DELETE FROM dbo.CUSTOMER_PROFILES;
DELETE FROM dbo.USERS;
DELETE FROM dbo.ROLES;

SET IDENTITY_INSERT dbo.ROLES ON;
INSERT INTO dbo.ROLES (ID, CODE, NAME) VALUES
(1, 'ADMIN', 'Admin'),
(2, 'CUSTOMER', 'Customer');
SET IDENTITY_INSERT dbo.ROLES OFF;

-- Password: 123456 (BCrypt hash, workFactor=12)
SET IDENTITY_INSERT dbo.USERS ON;
INSERT INTO dbo.USERS (ID, ROLE_ID, USERNAME, PASSWORD_HASH, EMAIL, PHONE, STATUS, IS_ADMIN) VALUES
(1, 1, 'admin', '$2a$12$kxJFpnF8lv10tPuwHjnmweG5Nj1o5ZbdjQs9Sl9Az4fe7jhXoImZC', 'admin@fishshop.local', '0901234567', 1, 1),
(2, 2, 'customer01', '$2a$12$kxJFpnF8lv10tPuwHjnmweG5Nj1o5ZbdjQs9Sl9Az4fe7jhXoImZC', 'khachhang@gmail.com', '0901234569', 1, 0);
SET IDENTITY_INSERT dbo.USERS OFF;

SET IDENTITY_INSERT dbo.CUSTOMER_PROFILES ON;
INSERT INTO dbo.CUSTOMER_PROFILES (ID, USER_ID, CUSTOMER_CODE, FULL_NAME, PHONE) VALUES
(1, 2, 'KH20260101000000', 'Nguyen Van Khach', '0901234569');
SET IDENTITY_INSERT dbo.CUSTOMER_PROFILES OFF;

-- ----------------------------------------------------------
-- 2. SEED DATA CHO MICROSERVICE PRODUCT
-- ----------------------------------------------------------
USE FishShop_Product;
GO

-- Xóa dữ liệu cũ theo thứ tự FK
DELETE FROM dbo.INVENTORY_TRANSACTIONS;
DELETE FROM dbo.INVENTORY;
DELETE FROM dbo.PRODUCT_IMAGES;
DELETE FROM dbo.PRODUCTS;
DELETE FROM dbo.CATEGORIES;

SET IDENTITY_INSERT dbo.CATEGORIES ON;
INSERT INTO dbo.CATEGORIES (ID, CATEGORY_CODE, NAME, SLUG, DESCRIPTION, STATUS) VALUES
(1, 'C1', 'Ca Canh', 'ca-canh', 'Cac loai ca canh dep', 1),
(2, 'C2', 'Phu Kien', 'phu-kien', 'Phu kien ho ca', 1),
(3, 'C3', 'Thuc An', 'thuc-an', 'Thuc an dinh duong cho ca', 1);
SET IDENTITY_INSERT dbo.CATEGORIES OFF;

SET IDENTITY_INSERT dbo.PRODUCTS ON;
INSERT INTO dbo.PRODUCTS (ID, CATEGORY_ID, PRODUCT_CODE, NAME, SLUG, SALE_PRICE, DESCRIPTION, IMAGE_URL, STATUS) VALUES
(1, 1, 'P1', 'Ca Betta Rong Do', 'ca-betta-rong-do', 50000.00, 'Ca Betta Rong Do cuc dep', 'betta-red.jpg', 1),
(2, 1, 'P2', 'Ca Bay Mau Thai', 'ca-bay-mau-thai', 25000.00, 'Ca Bay Mau Thai thuan chung', 'guppy-thai.jpg', 1),
(3, 2, 'P3', 'May bom chim 5W', 'may-bom-chim-5w', 65000.00, 'Bom nuoc ho ca loai nho', 'bom-5w.jpg', 1),
(4, 3, 'P4', 'Cam ca Tetra Color 50g', 'cam-ca-tetra-color-50g', 85000.00, 'Thuc an len mau cho ca', 'tetra-color.jpg', 1);
SET IDENTITY_INSERT dbo.PRODUCTS OFF;

INSERT INTO dbo.INVENTORY (PRODUCT_ID, STOCK_QUANTITY, RESERVED_QUANTITY, WAREHOUSE_LOCATION) VALUES
(1, 100, 0, 'A1-01'),
(2, 500, 0, 'A1-02'),
(3, 50, 0, 'B1-01'),
(4, 200, 0, 'C1-01');

-- ----------------------------------------------------------
-- 3. SEED DATA CHO MICROSERVICE ORDER
-- ----------------------------------------------------------
USE FishShop_Order;
GO

-- Xóa dữ liệu cũ theo thứ tự FK
DELETE FROM dbo.PROMOTION_USAGES;
DELETE FROM dbo.PROMOTION_PRODUCTS;
DELETE FROM dbo.PAYMENTS;
DELETE FROM dbo.ORDER_ITEMS;
DELETE FROM dbo.ORDERS;
DELETE FROM dbo.CART_ITEMS;
DELETE FROM dbo.SHOPPING_CARTS;
DELETE FROM dbo.PROMOTIONS;

SET IDENTITY_INSERT dbo.PROMOTIONS ON;
INSERT INTO dbo.PROMOTIONS (ID, PROMO_CODE, TITLE, DISCOUNT_TYPE, DISCOUNT_VALUE, START_AT, END_AT, STATUS) VALUES
(1, 'WELCOME', 'Chao ban moi', 'PERCENT', 10.00, DATEADD(day, -1, GETDATE()), DATEADD(year, 1, GETDATE()), 1),
(2, 'FREESHIP', 'Freeship don 0d', 'AMOUNT', 30000.00, DATEADD(day, -1, GETDATE()), DATEADD(year, 1, GETDATE()), 1);
SET IDENTITY_INSERT dbo.PROMOTIONS OFF;

-- ----------------------------------------------------------
-- 4. SEED DATA CHO MICROSERVICE CONTENT
-- ----------------------------------------------------------
USE FishShop_Content;
GO

-- Xóa dữ liệu cũ theo thứ tự FK
DELETE FROM dbo.BLOG_POSTS;
DELETE FROM dbo.BLOG_CATEGORIES;
DELETE FROM dbo.CONTACT_MESSAGES;

SET IDENTITY_INSERT dbo.BLOG_CATEGORIES ON;
INSERT INTO dbo.BLOG_CATEGORIES (ID, NAME, SLUG) VALUES
(1, 'Kien Thuc Nuoi Ca', 'kien-thuc-nuoi-ca'),
(2, 'Tin Tuc Khuyen Mai', 'tin-tuc-khuyen-mai');
SET IDENTITY_INSERT dbo.BLOG_CATEGORIES OFF;

SET IDENTITY_INSERT dbo.BLOG_POSTS ON;
INSERT INTO dbo.BLOG_POSTS (ID, CATEGORY_ID, AUTHOR_ID, TITLE, SLUG, SUMMARY, CONTENT, STATUS) VALUES
(1, 1, 1, 'Cach cham soc ca Betta', 'cach-cham-soc-ca-betta', 'Huong dan chi tiet', '<p>Ca Betta rat de nuoi...</p>', 'PUBLISHED'),
(2, 2, 1, 'Khuyen mai thang 5', 'khuyen-mai-thang-5', 'Co hoi mua sam', '<p>Nhap ma WELCOME...</p>', 'PUBLISHED');
SET IDENTITY_INSERT dbo.BLOG_POSTS OFF;

PRINT 'Seed data cho SQL Server da duoc tao thanh cong!';
GO
