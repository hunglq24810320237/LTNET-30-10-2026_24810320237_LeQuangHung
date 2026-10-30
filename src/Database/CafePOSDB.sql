IF DB_ID(N'CafePOSDB') IS NULL CREATE DATABASE CafePOSDB;
GO
USE CafePOSDB;
GO
IF OBJECT_ID(N'dbo.ChiTietHoaDon', N'U') IS NULL
CREATE TABLE dbo.ChiTietHoaDon (
    MaHoaDon INT NOT NULL,
    MaMon INT NOT NULL,
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0),
    CONSTRAINT PK_ChiTietHoaDon PRIMARY KEY (MaHoaDon, MaMon)
);
GO
IF OBJECT_ID(N'dbo.BanAn', N'U') IS NULL
CREATE TABLE dbo.BanAn (
    MaBan INT IDENTITY(1,1) PRIMARY KEY,
    TenBan NVARCHAR(50) NOT NULL UNIQUE,
    TrangThai NVARCHAR(20) NOT NULL CONSTRAINT DF_BanAn_TrangThai DEFAULT N'Trống'
        CHECK (TrangThai IN (N'Trống', N'Có khách'))
);
GO
IF OBJECT_ID(N'dbo.DanhMuc', N'U') IS NULL
CREATE TABLE dbo.DanhMuc (
    MaDanhMuc INT IDENTITY(1,1) PRIMARY KEY,
    TenDanhMuc NVARCHAR(100) NOT NULL UNIQUE
);
GO
IF OBJECT_ID(N'dbo.MonAn', N'U') IS NULL
CREATE TABLE dbo.MonAn (
    MaMon INT IDENTITY(1,1) PRIMARY KEY,
    TenMon NVARCHAR(100) NOT NULL,
    MaDanhMuc INT NOT NULL REFERENCES dbo.DanhMuc(MaDanhMuc),
    DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0)
);
GO
IF OBJECT_ID(N'dbo.HoaDon', N'U') IS NULL
CREATE TABLE dbo.HoaDon (
    MaHoaDon INT IDENTITY(1,1) PRIMARY KEY,
    MaBan INT NOT NULL REFERENCES dbo.BanAn(MaBan),
    GioVao DATETIME2 NOT NULL CONSTRAINT DF_HoaDon_GioVao DEFAULT SYSDATETIME(),
    GioRa DATETIME2 NULL,
    TongTien DECIMAL(18,2) NOT NULL CONSTRAINT DF_HoaDon_TongTien DEFAULT 0,
    TrangThai BIT NOT NULL CONSTRAINT DF_HoaDon_TrangThai DEFAULT 0
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_ChiTietHoaDon_HoaDon')
    ALTER TABLE dbo.ChiTietHoaDon ADD CONSTRAINT FK_ChiTietHoaDon_HoaDon FOREIGN KEY(MaHoaDon) REFERENCES dbo.HoaDon(MaHoaDon);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_ChiTietHoaDon_MonAn')
    ALTER TABLE dbo.ChiTietHoaDon ADD CONSTRAINT FK_ChiTietHoaDon_MonAn FOREIGN KEY(MaMon) REFERENCES dbo.MonAn(MaMon);
GO
IF NOT EXISTS (SELECT 1 FROM dbo.BanAn)
    INSERT dbo.BanAn(TenBan) VALUES (N'Bàn 01'),(N'Bàn 02'),(N'Bàn 03'),(N'Bàn 04'),(N'Bàn 05'),(N'Bàn 06');
IF NOT EXISTS (SELECT 1 FROM dbo.DanhMuc)
    INSERT dbo.DanhMuc(TenDanhMuc) VALUES (N'Cà phê'),(N'Trà sữa'),(N'Đồ ăn nhẹ');
IF NOT EXISTS (SELECT 1 FROM dbo.MonAn)
    INSERT dbo.MonAn(TenMon,MaDanhMuc,DonGia)
    SELECT N'Cà phê sữa',MaDanhMuc,32000 FROM dbo.DanhMuc WHERE TenDanhMuc=N'Cà phê'
    UNION ALL SELECT N'Bạc xỉu',MaDanhMuc,35000 FROM dbo.DanhMuc WHERE TenDanhMuc=N'Cà phê'
    UNION ALL SELECT N'Trà sữa trân châu',MaDanhMuc,42000 FROM dbo.DanhMuc WHERE TenDanhMuc=N'Trà sữa'
    UNION ALL SELECT N'Bánh croissant',MaDanhMuc,28000 FROM dbo.DanhMuc WHERE TenDanhMuc=N'Đồ ăn nhẹ';
GO
