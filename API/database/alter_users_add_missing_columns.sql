-- ============================================================
-- Fix: Thêm các cột còn thiếu vào bảng USERS
-- Chạy script này trên Oracle với user fshop_db
-- ============================================================

-- Thêm cột IS_ADMIN (phân biệt super admin)
ALTER TABLE USERS ADD IS_ADMIN NUMBER(1) DEFAULT 0 NOT NULL;

-- Thêm cột CREATED_BY, UPDATED_BY (audit trail)
ALTER TABLE USERS ADD CREATED_BY NUMBER;
ALTER TABLE USERS ADD UPDATED_BY NUMBER;

-- Thêm cột RESET_TOKEN, RESET_TOKEN_EXP (quên mật khẩu)
ALTER TABLE USERS ADD RESET_TOKEN VARCHAR2(200 CHAR);
ALTER TABLE USERS ADD RESET_TOKEN_EXP DATE;

-- Cập nhật tài khoản ADMIN đã seed sẵn
UPDATE USERS SET IS_ADMIN = 1 WHERE USERNAME = 'admin';

-- Seed thêm role CUSTOMER nếu chưa có (an toàn - bỏ qua nếu đã tồn tại)
INSERT INTO ROLES (CODE, NAME, DESCRIPTION)
SELECT 'CUSTOMER', 'Customer', 'Khach hang'
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM ROLES WHERE CODE = 'CUSTOMER');

COMMIT;

-- Kiểm tra kết quả
SELECT COLUMN_NAME, DATA_TYPE, NULLABLE, DATA_DEFAULT
FROM USER_TAB_COLUMNS
WHERE TABLE_NAME = 'USERS'
ORDER BY COLUMN_ID;
