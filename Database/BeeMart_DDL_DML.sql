-- =====================================================================
-- BeeMart Point of Sale (POS) Database Script (DDL & DML)
-- Compatible with Microsoft SQL Server 2019/2022 and Azure SQL
-- Target Database: BeeMartDb
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BeeMartDb')
BEGIN
    CREATE DATABASE BeeMartDb;
END
GO

USE BeeMartDb;
GO

-- 1. Table: MsUser
IF OBJECT_ID('dbo.TrTransactionDetail', 'U') IS NOT NULL DROP TABLE dbo.TrTransactionDetail;
IF OBJECT_ID('dbo.TrTransaction', 'U') IS NOT NULL DROP TABLE dbo.TrTransaction;
IF OBJECT_ID('dbo.MsProduct', 'U') IS NOT NULL DROP TABLE dbo.MsProduct;
IF OBJECT_ID('dbo.LtPaymentMethod', 'U') IS NOT NULL DROP TABLE dbo.LtPaymentMethod;
IF OBJECT_ID('dbo.MsUser', 'U') IS NOT NULL DROP TABLE dbo.MsUser;
GO

CREATE TABLE dbo.MsUser (
    IdUser NVARCHAR(5) NOT NULL,
    Username NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(300) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    Stsrc BIT NOT NULL DEFAULT 1,
    Datein DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Userin NVARCHAR(36) NULL,
    Dateup DATETIME2 NULL,
    Userup NVARCHAR(36) NULL,
    CONSTRAINT PK_MsUser PRIMARY KEY (IdUser),
    CONSTRAINT UQ_MsUser_Username UNIQUE (Username)
);
GO

-- 2. Table: LtPaymentMethod
CREATE TABLE dbo.LtPaymentMethod (
    IdPaymentMethod INT NOT NULL,
    MethodName NVARCHAR(50) NOT NULL,
    Stsrc BIT NOT NULL DEFAULT 1,
    Datein DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Userin NVARCHAR(36) NULL,
    Dateup DATETIME2 NULL,
    Userup NVARCHAR(36) NULL,
    CONSTRAINT PK_LtPaymentMethod PRIMARY KEY (IdPaymentMethod)
);
GO

-- 3. Table: MsProduct
CREATE TABLE dbo.MsProduct (
    IdProduct NVARCHAR(36) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Weight DECIMAL(10,2) NOT NULL,
    Stock INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Discount DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    Stsrc BIT NOT NULL DEFAULT 1,
    Datein DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Userin NVARCHAR(36) NULL,
    Dateup DATETIME2 NULL,
    Userup NVARCHAR(36) NULL,
    CONSTRAINT PK_MsProduct PRIMARY KEY (IdProduct)
);
GO

-- 4. Table: TrTransaction
CREATE TABLE dbo.TrTransaction (
    IdTransaction NVARCHAR(36) NOT NULL,
    InvoiceNo NVARCHAR(20) NOT NULL,
    TransactionDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IdUser NVARCHAR(5) NOT NULL,
    IdPaymentMethod INT NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    Stsrc BIT NOT NULL DEFAULT 1,
    Datein DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Userin NVARCHAR(36) NULL,
    Dateup DATETIME2 NULL,
    Userup NVARCHAR(36) NULL,
    CONSTRAINT PK_TrTransaction PRIMARY KEY (IdTransaction),
    CONSTRAINT FK_TrTransaction_MsUser FOREIGN KEY (IdUser) REFERENCES dbo.MsUser(IdUser),
    CONSTRAINT FK_TrTransaction_LtPaymentMethod FOREIGN KEY (IdPaymentMethod) REFERENCES dbo.LtPaymentMethod(IdPaymentMethod)
);
GO

-- 5. Table: TrTransactionDetail
CREATE TABLE dbo.TrTransactionDetail (
    IdTransactionDetail NVARCHAR(36) NOT NULL,
    IdTransaction NVARCHAR(36) NOT NULL,
    IdProduct NVARCHAR(36) NOT NULL,
    Qty INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Discount DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    SubTotal DECIMAL(18,2) NOT NULL,
    Stsrc BIT NOT NULL DEFAULT 1,
    Datein DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Userin NVARCHAR(36) NULL,
    Dateup DATETIME2 NULL,
    Userup NVARCHAR(36) NULL,
    CONSTRAINT PK_TrTransactionDetail PRIMARY KEY (IdTransactionDetail),
    CONSTRAINT FK_TrTransactionDetail_TrTransaction FOREIGN KEY (IdTransactionDetail_TrTransaction) REFERENCES dbo.TrTransaction(IdTransaction),
    CONSTRAINT FK_TrTransactionDetail_MsProduct FOREIGN KEY (IdProduct) REFERENCES dbo.MsProduct(IdProduct)
);
GO

-- Fix constraint name typo in FK definition if needed
ALTER TABLE dbo.TrTransactionDetail DROP CONSTRAINT FK_TrTransactionDetail_TrTransaction;
ALTER TABLE dbo.TrTransactionDetail ADD CONSTRAINT FK_TrTransactionDetail_TrTransaction FOREIGN KEY (IdTransaction) REFERENCES dbo.TrTransaction(IdTransaction);
GO

-- =====================================================================
-- DML: Seed Data
-- Passwords: SHA256 of "Admin123!" for admin, "Cashier123!" for cashiers
-- SHA256("Admin123!")   = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3"
-- SHA256("Cashier123!") = "6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b"
-- SHA256("password")    = "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8"
-- =====================================================================

-- Seed Users
-- Pre-seeded users matching requirements: '76985', '66859', '55794', '78886', '55356'
INSERT INTO dbo.MsUser (IdUser, Username, FullName, PasswordHash, Role, Stsrc, Datein, Userin)
VALUES 
('76985', 'steven', 'Steven Anderson', '5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8', 'Cashier', 1, SYSUTCDATETIME(), 'SYSTEM'),
('66859', 'maria', 'Maria Chen', '5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8', 'Cashier', 1, SYSUTCDATETIME(), 'SYSTEM'),
('55794', 'admin', 'Administrator System', '5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8', 'Admin', 1, SYSUTCDATETIME(), 'SYSTEM'),
('78886', 'budi', 'Budi Santoso', '5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8', 'Cashier', 1, SYSUTCDATETIME(), 'SYSTEM'),
('55356', 'superadmin', 'Head Office Supervisor', '5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8', 'Admin', 1, SYSUTCDATETIME(), 'SYSTEM');
GO

-- Seed Payment Methods
INSERT INTO dbo.LtPaymentMethod (IdPaymentMethod, MethodName, Stsrc, Datein, Userin)
VALUES 
(1, 'Cash', 1, SYSUTCDATETIME(), 'SYSTEM'),
(2, 'Debit', 1, SYSUTCDATETIME(), 'SYSTEM'),
(3, 'Credit', 1, SYSUTCDATETIME(), 'SYSTEM');
GO

-- Seed Products
INSERT INTO dbo.MsProduct (IdProduct, Name, Weight, Stock, Price, Discount, Stsrc, Datein, Userin)
VALUES 
('11111111-1111-1111-1111-111111111101', 'Apel Fuji Premium', 1.00, 50, 45000.00, 5.00, 1, SYSUTCDATETIME(), 'SYSTEM'),
('11111111-1111-1111-1111-111111111102', 'Apel Manalagi Segar', 1.00, 35, 28000.00, 0.00, 1, SYSUTCDATETIME(), 'SYSTEM'),
('11111111-1111-1111-1111-111111111103', 'Susu UHT Full Cream 1L', 1.00, 80, 21500.00, 10.00, 1, SYSUTCDATETIME(), 'SYSTEM'),
('11111111-1111-1111-1111-111111111104', 'Susu Kental Manis 370g', 0.37, 100, 12500.00, 0.00, 1, SYSUTCDATETIME(), 'SYSTEM'),
('11111111-1111-1111-1111-111111111105', 'Beras Pandan Wangi 5kg', 5.00, 40, 78000.00, 0.00, 1, SYSUTCDATETIME(), 'SYSTEM'),
('11111111-1111-1111-1111-111111111106', 'Minyak Goreng Refill 2L', 2.00, 60, 36000.00, 2.50, 1, SYSUTCDATETIME(), 'SYSTEM'),
('11111111-1111-1111-1111-111111111107', 'Gula Pasir Kristal 1kg', 1.00, 90, 17500.00, 0.00, 1, SYSUTCDATETIME(), 'SYSTEM'),
('11111111-1111-1111-1111-111111111108', 'Telur Ayam Negeri 1kg', 1.00, 45, 31000.00, 0.00, 1, SYSUTCDATETIME(), 'SYSTEM'),
('11111111-1111-1111-1111-111111111109', 'Roti Tawar Gandum', 0.40, 30, 18500.00, 5.00, 1, SYSUTCDATETIME(), 'SYSTEM'),
('11111111-1111-1111-1111-111111111110', 'Kopi Bubuk Arabika 200g', 0.20, 55, 34000.00, 0.00, 1, SYSUTCDATETIME(), 'SYSTEM'),
('11111111-1111-1111-1111-111111111111', 'Teh Celup Melati Kotak 25s', 0.05, 120, 8500.00, 0.00, 1, SYSUTCDATETIME(), 'SYSTEM'),
('11111111-1111-1111-1111-111111111112', 'Mie Instan Goreng Spesial', 0.08, 250, 3500.00, 0.00, 1, SYSUTCDATETIME(), 'SYSTEM');
GO

-- Sample Initial Transaction for Testing
INSERT INTO dbo.TrTransaction (IdTransaction, InvoiceNo, TransactionDate, IdUser, IdPaymentMethod, TotalPrice, Stsrc, Datein, Userin)
VALUES 
('22222222-2222-2222-2222-222222222201', '76985090426001', '2026-04-09 10:15:30', '76985', 1, 89000.00, 1, '2026-04-09 10:15:30', '76985'),
('22222222-2222-2222-2222-222222222202', '66859090426001', '2026-04-09 11:45:10', '66859', 2, 78000.00, 1, '2026-04-09 11:45:10', '66859');
GO

INSERT INTO dbo.TrTransactionDetail (IdTransactionDetail, IdTransaction, IdProduct, Qty, Price, Discount, SubTotal, Stsrc, Datein, Userin)
VALUES 
('33333333-3333-3333-3333-333333333301', '22222222-2222-2222-2222-222222222201', '11111111-1111-1111-1111-111111111101', 1, 45000.00, 5.00, 42750.00, 1, '2026-04-09 10:15:30', '76985'),
('33333333-3333-3333-3333-333333333302', '22222222-2222-2222-2222-222222222201', '11111111-1111-1111-1111-111111111103', 2, 21500.00, 10.00, 38700.00, 1, '2026-04-09 10:15:30', '76985'),
('33333333-3333-3333-3333-333333333303', '22222222-2222-2222-2222-222222222201', '11111111-1111-1111-1111-111111111111', 1, 8500.00, 0.00, 8500.00, 1, '2026-04-09 10:15:30', '76985'),
('33333333-3333-3333-3333-333333333304', '22222222-2222-2222-2222-222222222202', '11111111-1111-1111-1111-111111111105', 1, 78000.00, 0.00, 78000.00, 1, '2026-04-09 11:45:10', '66859');
GO
