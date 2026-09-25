/* =====================================================================
   BeautyCenter BD - Full Schema Script
   Run this against your database (matches Web.config's connection
   string, e.g. "Vendors" on (localdb)\MSSQLLocalDB) to create every
   table used by the app: Users, master data (Category/Brand/Unit/
   Supplier/Customer/Warehouse), Products, Purchases, Sales, Stock
   transfers/transactions, Salaries, the permission system, and the
   public shop (Orders/OrderItems/ProductImages).

   Safe to re-run: every CREATE TABLE / ALTER TABLE is wrapped in an
   existence check. Order matters (master data first, then things
   that reference it).

   NOTE: If you use EF Code-First automatic migrations instead
   (the app is configured for this - see Migrations/Configuration.cs),
   you do NOT need to run this script; just set your connection
   string and run the app / Update-Database and EF creates everything
   itself, including default Modules/RolePermissions/Unit/Warehouse
   seed data. This script is here for people who want to set up the
   database by hand, or a colleague's SQL Server directly.
===================================================================== */

/* Adjust this if your database has a different name. */
-- USE [Vendors];
-- GO

/* =====================================================================
   1. USERS
===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE dbo.Users (
        UserId              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FullName            NVARCHAR(100) NOT NULL,
        Phone               NVARCHAR(30)  NOT NULL,
        Address             NVARCHAR(500) NULL,
        Email               NVARCHAR(100) NOT NULL,
        PasswordHash        NVARCHAR(MAX) NOT NULL,
        RoleName            NVARCHAR(MAX) NOT NULL,
        ImagePath           NVARCHAR(300) NULL,
        IsActive            BIT NOT NULL DEFAULT(1),
        CreateDate          DATETIME NOT NULL DEFAULT(GETDATE()),
        WarehouseId         INT NULL,
        ProfileImagePath    NVARCHAR(300) NULL,
        CanViewDashboard    BIT NOT NULL DEFAULT(1),
        CanViewProducts     BIT NOT NULL DEFAULT(0),
        CanAddProducts      BIT NOT NULL DEFAULT(0),
        CanEditProducts     BIT NOT NULL DEFAULT(0),
        CanDeleteProducts   BIT NOT NULL DEFAULT(0),
        CanViewOrders       BIT NOT NULL DEFAULT(0),
        CanAddOrders        BIT NOT NULL DEFAULT(0),
        CanEditOrders       BIT NOT NULL DEFAULT(0),
        CanDeleteOrders     BIT NOT NULL DEFAULT(0),
        CanViewReports      BIT NOT NULL DEFAULT(0),
        CanManageUsers      BIT NOT NULL DEFAULT(0),
        CanViewNotebook     BIT NOT NULL DEFAULT(0),
        CanAddNotebook      BIT NOT NULL DEFAULT(0),
        CanEditNotebook     BIT NOT NULL DEFAULT(0),
        CanDeleteNotebook   BIT NOT NULL DEFAULT(0)
    );
END
GO

/* =====================================================================
   2. WAREHOUSES  (and the deferred FK from Users)
===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Warehouses')
BEGIN
    CREATE TABLE dbo.Warehouses (
        WarehouseId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name        NVARCHAR(150) NOT NULL,
        Code        NVARCHAR(30)  NULL,
        Address     NVARCHAR(300) NULL,
        Phone       NVARCHAR(30)  NULL,
        IsActive    BIT NOT NULL DEFAULT(1)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Warehouses_WarehouseId')
BEGIN
    ALTER TABLE dbo.Users
        ADD CONSTRAINT FK_Users_Warehouses_WarehouseId
        FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(WarehouseId);
END
GO

/* =====================================================================
   3. MASTER DATA: Category, Brand, Unit, Supplier, Customer
===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE dbo.Categories (
        CategoryId  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name        NVARCHAR(100) NOT NULL,
        Description NVARCHAR(300) NULL,
        IsActive    BIT NOT NULL DEFAULT(1)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Brands')
BEGIN
    CREATE TABLE dbo.Brands (
        BrandId     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name        NVARCHAR(100) NOT NULL,
        Description NVARCHAR(300) NULL,
        ImagePath   NVARCHAR(300) NULL,
        IsActive    BIT NOT NULL DEFAULT(1)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Units')
BEGIN
    CREATE TABLE dbo.Units (
        UnitId     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name       NVARCHAR(50) NOT NULL,
        ShortCode  NVARCHAR(10) NOT NULL,
        IsActive   BIT NOT NULL DEFAULT(1)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Suppliers')
BEGIN
    CREATE TABLE dbo.Suppliers (
        SupplierId  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name        NVARCHAR(150) NOT NULL,
        Phone       NVARCHAR(30)  NULL,
        Email       NVARCHAR(100) NULL,
        Address     NVARCHAR(300) NULL,
        IsActive    BIT NOT NULL DEFAULT(1)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE dbo.Customers (
        CustomerId  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name        NVARCHAR(150) NOT NULL,
        Phone       NVARCHAR(30)  NULL,
        Email       NVARCHAR(100) NULL,
        Address     NVARCHAR(300) NULL,
        IsActive    BIT NOT NULL DEFAULT(1)
    );
END
GO

/* =====================================================================
   4. PRODUCTS  (+ ProductImages gallery)
===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE dbo.Products (
        ProductId       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProductName     NVARCHAR(150) NOT NULL,
        Description     NVARCHAR(500) NULL,
        UnitPrice       DECIMAL(18,2) NOT NULL,
        StockQuantity   INT NOT NULL,
        ImagePath       NVARCHAR(300) NULL,
        IsActive        BIT NOT NULL DEFAULT(1),
        CreateDate      DATETIME NOT NULL DEFAULT(GETDATE()),
        SKU             NVARCHAR(50) NULL,
        CategoryId      INT NOT NULL,
        BrandId         INT NOT NULL,
        UnitId          INT NOT NULL,
        PurchasePrice   DECIMAL(18,2) NOT NULL DEFAULT(0),
        DiscountPercent DECIMAL(5,2)  NOT NULL DEFAULT(0),
        ReorderLevel    INT NOT NULL DEFAULT(0),
        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId),
        CONSTRAINT FK_Products_Brands     FOREIGN KEY (BrandId)    REFERENCES dbo.Brands(BrandId),
        CONSTRAINT FK_Products_Units      FOREIGN KEY (UnitId)     REFERENCES dbo.Units(UnitId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductImages')
BEGIN
    CREATE TABLE dbo.ProductImages (
        ProductImageId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProductId      INT NOT NULL,
        ImagePath      NVARCHAR(300) NOT NULL,
        SortOrder      INT NOT NULL DEFAULT(0),
        CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId) ON DELETE CASCADE
    );
END
GO

/* =====================================================================
   5. PURCHASES
===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Purchases')
BEGIN
    CREATE TABLE dbo.Purchases (
        PurchaseId      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        PurchaseNumber  NVARCHAR(50) NOT NULL,
        SupplierId      INT NOT NULL,
        WarehouseId     INT NOT NULL,
        PurchaseDate    DATETIME NOT NULL DEFAULT(GETDATE()),
        SubTotal        DECIMAL(18,2) NOT NULL,
        Discount        DECIMAL(18,2) NOT NULL DEFAULT(0),
        Tax             DECIMAL(18,2) NOT NULL DEFAULT(0),
        TotalAmount     DECIMAL(18,2) NOT NULL,
        Status          NVARCHAR(30) NOT NULL DEFAULT('Pending'),
        CreatedByUserId INT NOT NULL,
        IsActive        BIT NOT NULL DEFAULT(1),
        CONSTRAINT FK_Purchases_Suppliers  FOREIGN KEY (SupplierId)  REFERENCES dbo.Suppliers(SupplierId),
        CONSTRAINT FK_Purchases_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(WarehouseId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PurchaseDetails')
BEGIN
    CREATE TABLE dbo.PurchaseDetails (
        PurchaseDetailId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        PurchaseId       INT NOT NULL,
        ProductId        INT NOT NULL,
        Quantity         INT NOT NULL,
        UnitCost         DECIMAL(18,2) NOT NULL,
        LineTotal        DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_PurchaseDetails_Purchases FOREIGN KEY (PurchaseId) REFERENCES dbo.Purchases(PurchaseId) ON DELETE CASCADE,
        CONSTRAINT FK_PurchaseDetails_Products  FOREIGN KEY (ProductId)  REFERENCES dbo.Products(ProductId)
    );
END
GO

/* =====================================================================
   6. SALES
===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Sales')
BEGIN
    CREATE TABLE dbo.Sales (
        SaleId          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        SaleNumber      NVARCHAR(50) NOT NULL,
        CustomerId      INT NULL,
        WarehouseId     INT NOT NULL,
        SaleDate        DATETIME NOT NULL DEFAULT(GETDATE()),
        SubTotal        DECIMAL(18,2) NOT NULL,
        Discount        DECIMAL(18,2) NOT NULL DEFAULT(0),
        Tax             DECIMAL(18,2) NOT NULL DEFAULT(0),
        TotalAmount     DECIMAL(18,2) NOT NULL,
        Status          NVARCHAR(30) NOT NULL DEFAULT('Completed'),
        CreatedByUserId INT NOT NULL,
        IsActive        BIT NOT NULL DEFAULT(1),
        CONSTRAINT FK_Sales_Customers  FOREIGN KEY (CustomerId)  REFERENCES dbo.Customers(CustomerId),
        CONSTRAINT FK_Sales_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(WarehouseId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SaleDetails')
BEGIN
    CREATE TABLE dbo.SaleDetails (
        SaleDetailId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        SaleId       INT NOT NULL,
        ProductId    INT NOT NULL,
        Quantity     INT NOT NULL,
        UnitPrice    DECIMAL(18,2) NOT NULL,
        LineTotal    DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_SaleDetails_Sales    FOREIGN KEY (SaleId)    REFERENCES dbo.Sales(SaleId) ON DELETE CASCADE,
        CONSTRAINT FK_SaleDetails_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId)
    );
END
GO

/* =====================================================================
   7. STOCK MOVEMENT: Transfers + Transactions
===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StockTransfers')
BEGIN
    CREATE TABLE dbo.StockTransfers (
        StockTransferId  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TransferNumber   NVARCHAR(50) NOT NULL,
        FromWarehouseId  INT NOT NULL,
        ToWarehouseId    INT NOT NULL,
        TransferDate     DATETIME NOT NULL DEFAULT(GETDATE()),
        Status           NVARCHAR(30) NOT NULL DEFAULT('Requested'),
        RequestedByUserId INT NOT NULL,
        RequestedDate    DATETIME NULL,
        ApprovedByUserId INT NULL,
        ApprovedDate     DATETIME NULL,
        RejectionReason  NVARCHAR(300) NULL,
        SentByUserId     INT NULL,
        SentDate         DATETIME NULL,
        ReceivedByUserId INT NULL,
        ReceivedDate     DATETIME NULL,
        IsActive         BIT NOT NULL DEFAULT(1),
        CONSTRAINT FK_StockTransfers_FromWarehouse FOREIGN KEY (FromWarehouseId) REFERENCES dbo.Warehouses(WarehouseId),
        CONSTRAINT FK_StockTransfers_ToWarehouse   FOREIGN KEY (ToWarehouseId)   REFERENCES dbo.Warehouses(WarehouseId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StockTransferDetails')
BEGIN
    CREATE TABLE dbo.StockTransferDetails (
        StockTransferDetailId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        StockTransferId       INT NOT NULL,
        ProductId             INT NOT NULL,
        Quantity              INT NOT NULL,
        CONSTRAINT FK_StockTransferDetails_Transfers FOREIGN KEY (StockTransferId) REFERENCES dbo.StockTransfers(StockTransferId) ON DELETE CASCADE,
        CONSTRAINT FK_StockTransferDetails_Products   FOREIGN KEY (ProductId)       REFERENCES dbo.Products(ProductId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StockTransactions')
BEGIN
    CREATE TABLE dbo.StockTransactions (
        StockTransactionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProductId          INT NOT NULL,
        WarehouseId         INT NOT NULL,
        TransactionType     NVARCHAR(20) NOT NULL,
        QuantityChange      INT NOT NULL,
        ReferenceId         INT NULL,
        ReferenceType       NVARCHAR(50) NULL,
        TransactionDate     DATETIME NOT NULL DEFAULT(GETDATE()),
        CreatedByUserId      INT NOT NULL,
        CONSTRAINT FK_StockTransactions_Products   FOREIGN KEY (ProductId)   REFERENCES dbo.Products(ProductId),
        CONSTRAINT FK_StockTransactions_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(WarehouseId)
    );
END
GO

/* =====================================================================
   8. SALARIES (HR)
===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Salaries')
BEGIN
    CREATE TABLE dbo.Salaries (
        SalaryId        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId          INT NOT NULL,
        BasicSalary     DECIMAL(18,2) NOT NULL,
        Bonus           DECIMAL(18,2) NOT NULL DEFAULT(0),
        Deduction       DECIMAL(18,2) NOT NULL DEFAULT(0),
        NetSalary       DECIMAL(18,2) NOT NULL,
        SalaryMonth     INT NOT NULL,
        SalaryYear      INT NOT NULL,
        Status          NVARCHAR(20) NOT NULL DEFAULT('Pending'),
        PaidDate        DATETIME NULL,
        CreatedByUserId INT NOT NULL,
        Remarks         NVARCHAR(300) NULL,
        IsActive        BIT NOT NULL DEFAULT(1),
        CONSTRAINT FK_Salaries_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
    );
END
GO

/* =====================================================================
   9. PERMISSION SYSTEM: Modules + RolePermissions
===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Modules')
BEGIN
    CREATE TABLE dbo.Modules (
        ModuleId    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name        NVARCHAR(50) NOT NULL,
        Description NVARCHAR(150) NULL,
        SortOrder   INT NOT NULL DEFAULT(0)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RolePermissions')
BEGIN
    CREATE TABLE dbo.RolePermissions (
        RolePermissionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        RoleName         NVARCHAR(50) NOT NULL,
        ModuleId         INT NOT NULL,
        CanView          BIT NOT NULL DEFAULT(0),
        CanCreate        BIT NOT NULL DEFAULT(0),
        CanEdit          BIT NOT NULL DEFAULT(0),
        CanDelete        BIT NOT NULL DEFAULT(0),
        CONSTRAINT FK_RolePermissions_Modules FOREIGN KEY (ModuleId) REFERENCES dbo.Modules(ModuleId) ON DELETE CASCADE
    );
END
GO

/* =====================================================================
   10. PUBLIC SHOP: Orders + OrderItems
       (Dhaka / Outside-Dhaka delivery charge lives on Orders)
===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Orders')
BEGIN
    CREATE TABLE dbo.Orders (
        OrderId          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId           INT NOT NULL,
        OrderNumber      NVARCHAR(50) NOT NULL,
        TotalAmount      DECIMAL(18,2) NOT NULL,
        OrderStatus      NVARCHAR(30) NOT NULL,
        OrderDate        DATETIME NOT NULL DEFAULT(GETDATE()),
        IsActive         BIT NOT NULL DEFAULT(1),
        Discount         DECIMAL(18,2) NOT NULL DEFAULT(0),
        Tax              DECIMAL(18,2) NOT NULL DEFAULT(0),
        CustomerPhone    NVARCHAR(30) NULL,
        CustomerEmail    NVARCHAR(150) NULL,
        CustomerAddress  NVARCHAR(500) NULL,
        WarehouseId      INT NOT NULL,
        ShippingName     NVARCHAR(150) NOT NULL,
        ShippingPhone    NVARCHAR(30) NOT NULL,
        ShippingAddress  NVARCHAR(300) NOT NULL,
        DeliveryArea     NVARCHAR(30) NOT NULL DEFAULT('Dhaka'),
        DeliveryCharge   DECIMAL(18,2) NOT NULL DEFAULT(60),
        SubTotal         DECIMAL(18,2) NOT NULL DEFAULT(0),
        DiscountAmount   DECIMAL(18,2) NOT NULL DEFAULT(0),
        SaleId           INT NULL,
        CONSTRAINT FK_Orders_Users      FOREIGN KEY (UserId)      REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_Orders_Warehouses FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(WarehouseId),
        CONSTRAINT FK_Orders_Sales      FOREIGN KEY (SaleId)      REFERENCES dbo.Sales(SaleId)
    );
END
GO

/* If you're upgrading an existing Orders table that pre-dates the
   Dhaka/Outside-Dhaka delivery charge feature, add the two columns: */
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'DeliveryArea' AND Object_ID = Object_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders ADD DeliveryArea NVARCHAR(30) NOT NULL DEFAULT('Dhaka');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'DeliveryCharge' AND Object_ID = Object_ID('dbo.Orders'))
BEGIN
    ALTER TABLE dbo.Orders ADD DeliveryCharge DECIMAL(18,2) NOT NULL DEFAULT(60);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderItems')
BEGIN
    CREATE TABLE dbo.OrderItems (
        OrderItemId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        OrderId     INT NOT NULL,
        ProductId   INT NOT NULL,
        Quantity    INT NOT NULL,
        UnitPrice   DECIMAL(18,2) NOT NULL,
        LineTotal   DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_OrderItems_Orders   FOREIGN KEY (OrderId)   REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId)
    );
END
GO

/* =====================================================================
   11. NOTEBOOK (existing legacy module - kept as-is)
===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NotebookEntries')
BEGIN
    CREATE TABLE dbo.NotebookEntries (
        NotebookEntryId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title           NVARCHAR(200) NOT NULL,
        Content         NVARCHAR(MAX) NULL,
        CreateDate      DATETIME NOT NULL DEFAULT(GETDATE())
    );
END
GO

/* =====================================================================
   12. SEED DATA - default Unit + Warehouse so Product/Order creation
       has something to reference right away. Safe to re-run.
===================================================================== */
IF NOT EXISTS (SELECT 1 FROM dbo.Units)
BEGIN
    INSERT INTO dbo.Units (Name, ShortCode, IsActive) VALUES ('Piece', 'pcs', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Warehouses)
BEGIN
    INSERT INTO dbo.Warehouses (Name, Code, Address, IsActive)
    VALUES ('Main Store', 'MAIN', 'Dhaka, Bangladesh', 1);
END
GO

PRINT 'Schema is up to date.';
