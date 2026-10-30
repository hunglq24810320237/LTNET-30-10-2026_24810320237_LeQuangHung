IF DB_ID(N'InventoryDB') IS NULL CREATE DATABASE InventoryDB;
GO
USE InventoryDB;
GO
IF OBJECT_ID(N'dbo.ChiTietPhieuKho',N'U') IS NULL
CREATE TABLE dbo.ChiTietPhieuKho(
    MaPhieu INT NOT NULL,
    MaVatTu VARCHAR(20) NOT NULL,
    SoLuong DECIMAL(18,2) NOT NULL CHECK(SoLuong>0),
    DonGia DECIMAL(18,2) NOT NULL CHECK(DonGia>=0),
    CONSTRAINT PK_ChiTietPhieuKho PRIMARY KEY(MaPhieu,MaVatTu)
);
GO
IF OBJECT_ID(N'dbo.NhaCungCap',N'U') IS NULL
CREATE TABLE dbo.NhaCungCap(
    MaNCC INT IDENTITY(1,1) PRIMARY KEY,
    TenNCC NVARCHAR(150) NOT NULL,
    DienThoai VARCHAR(15) NULL UNIQUE,
    DiaChi NVARCHAR(255) NULL
);
GO
IF OBJECT_ID(N'dbo.DanhMucVatTu',N'U') IS NULL
CREATE TABLE dbo.DanhMucVatTu(
    MaDanhMuc INT IDENTITY(1,1) PRIMARY KEY,
    TenDanhMuc NVARCHAR(100) NOT NULL UNIQUE
);
GO
IF OBJECT_ID(N'dbo.VatTu',N'U') IS NULL
CREATE TABLE dbo.VatTu(
    MaVatTu VARCHAR(20) PRIMARY KEY,
    TenVatTu NVARCHAR(150) NOT NULL,
    MaDanhMuc INT NULL REFERENCES dbo.DanhMucVatTu(MaDanhMuc),
    DVT NVARCHAR(20) NOT NULL,
    SoLuongTon DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK(SoLuongTon>=0)
);
GO
IF OBJECT_ID(N'dbo.PhieuKho',N'U') IS NULL
CREATE TABLE dbo.PhieuKho(
    MaPhieu INT IDENTITY(1,1) PRIMARY KEY,
    LoaiPhieu NVARCHAR(10) NOT NULL CHECK(LoaiPhieu IN(N'NHAP',N'XUAT')),
    NgayLap DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    MaNCC INT NULL REFERENCES dbo.NhaCungCap(MaNCC),
    TongTien DECIMAL(18,2) NOT NULL DEFAULT 0
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_ChiTiet_Phieu')
    ALTER TABLE dbo.ChiTietPhieuKho ADD CONSTRAINT FK_ChiTiet_Phieu FOREIGN KEY(MaPhieu) REFERENCES dbo.PhieuKho(MaPhieu);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_ChiTiet_VatTu')
    ALTER TABLE dbo.ChiTietPhieuKho ADD CONSTRAINT FK_ChiTiet_VatTu FOREIGN KEY(MaVatTu) REFERENCES dbo.VatTu(MaVatTu);
GO
IF NOT EXISTS(SELECT 1 FROM dbo.NhaCungCap)
    INSERT dbo.NhaCungCap(TenNCC,DienThoai,DiaChi) VALUES(N'Công ty Minh Phát','0901000001',N'Hà Nội'),(N'Vật tư An Khang','0901000002',N'Hồ Chí Minh');
IF NOT EXISTS(SELECT 1 FROM dbo.DanhMucVatTu)
    INSERT dbo.DanhMucVatTu(TenDanhMuc) VALUES(N'Nguyên liệu'),(N'Bao bì'),(N'Dụng cụ');
IF NOT EXISTS(SELECT 1 FROM dbo.VatTu)
    INSERT dbo.VatTu(MaVatTu,TenVatTu,MaDanhMuc,DVT,SoLuongTon)
    SELECT 'CF001',N'Hạt cà phê',MaDanhMuc,N'Kg',100 FROM dbo.DanhMucVatTu WHERE TenDanhMuc=N'Nguyên liệu'
    UNION ALL SELECT 'TS001',N'Trà ô long',MaDanhMuc,N'Kg',45 FROM dbo.DanhMucVatTu WHERE TenDanhMuc=N'Nguyên liệu'
    UNION ALL SELECT 'LY001',N'Ly giấy',MaDanhMuc,N'Cái',500 FROM dbo.DanhMucVatTu WHERE TenDanhMuc=N'Bao bì';
GO
