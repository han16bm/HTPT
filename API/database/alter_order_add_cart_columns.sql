-- ============================================================
-- Fix: Thêm/cập nhật cột PRODUCT_NAME và IMAGE_URL
-- Chạy script này nếu database FishShop_Order đã tồn tại
-- ============================================================

USE FishShop_Order;
GO

-- CART_ITEMS: thêm PRODUCT_NAME nếu chưa có
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.CART_ITEMS') AND name = 'PRODUCT_NAME'
)
BEGIN
    ALTER TABLE dbo.CART_ITEMS ADD PRODUCT_NAME NVARCHAR(250) NULL;
    PRINT 'Added PRODUCT_NAME to CART_ITEMS';
END
GO

-- CART_ITEMS: thêm IMAGE_URL nếu chưa có
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.CART_ITEMS') AND name = 'IMAGE_URL'
)
BEGIN
    ALTER TABLE dbo.CART_ITEMS ADD IMAGE_URL NVARCHAR(500) NULL;
    PRINT 'Added IMAGE_URL to CART_ITEMS';
END
GO

-- ORDER_ITEMS: đổi PRODUCT_NAME VARCHAR → NVARCHAR (hỗ trợ tiếng Việt)
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ORDER_ITEMS') AND name = 'PRODUCT_NAME'
      AND system_type_id = TYPE_ID('varchar')
)
BEGIN
    ALTER TABLE dbo.ORDER_ITEMS ALTER COLUMN PRODUCT_NAME NVARCHAR(250) NULL;
    PRINT 'Converted ORDER_ITEMS.PRODUCT_NAME from VARCHAR to NVARCHAR';
END
GO

-- ORDER_ITEMS: thêm IMAGE_URL nếu chưa có, hoặc đổi VARCHAR → NVARCHAR
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ORDER_ITEMS') AND name = 'IMAGE_URL'
)
BEGIN
    ALTER TABLE dbo.ORDER_ITEMS ADD IMAGE_URL NVARCHAR(500) NULL;
    PRINT 'Added IMAGE_URL to ORDER_ITEMS';
END
ELSE IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ORDER_ITEMS') AND name = 'IMAGE_URL'
      AND system_type_id = TYPE_ID('varchar')
)
BEGIN
    ALTER TABLE dbo.ORDER_ITEMS ALTER COLUMN IMAGE_URL NVARCHAR(500) NULL;
    PRINT 'Converted ORDER_ITEMS.IMAGE_URL from VARCHAR to NVARCHAR';
END
GO

PRINT 'Migration complete.';
