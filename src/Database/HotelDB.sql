IF DB_ID(N'HotelDB') IS NULL CREATE DATABASE HotelDB;
GO
USE HotelDB;
GO
IF OBJECT_ID(N'dbo.SuDungDichVu', N'U') IS NULL
CREATE TABLE dbo.SuDungDichVu (
    MaPhieu INT NOT NULL,
    MaDV INT NOT NULL,
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    ThoiGian DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0),
    CONSTRAINT PK_SuDungDichVu PRIMARY KEY (MaPhieu, MaDV, ThoiGian)
);
GO
IF OBJECT_ID(N'dbo.Phong', N'U') IS NULL
CREATE TABLE dbo.Phong (
    SoPhong VARCHAR(10) PRIMARY KEY,
    LoaiPhong NVARCHAR(50) NOT NULL,
    GiaTheoGio DECIMAL(18,2) NOT NULL CHECK (GiaTheoGio >= 0),
    TrangThai NVARCHAR(30) NOT NULL DEFAULT N'Trống'
        CHECK (TrangThai IN (N'Trống',N'Đang ở',N'Đang dọn dẹp'))
);
GO
IF OBJECT_ID(N'dbo.KhachHang', N'U') IS NULL
CREATE TABLE dbo.KhachHang (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    CCCD VARCHAR(20) NOT NULL UNIQUE,
    SoDienThoai VARCHAR(15) NULL
);
GO
IF OBJECT_ID(N'dbo.DichVu', N'U') IS NULL
CREATE TABLE dbo.DichVu (
    MaDV INT IDENTITY(1,1) PRIMARY KEY,
    TenDV NVARCHAR(100) NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0)
);
GO
IF OBJECT_ID(N'dbo.PhieuDatPhong', N'U') IS NULL
CREATE TABLE dbo.PhieuDatPhong (
    MaPhieu INT IDENTITY(1,1) PRIMARY KEY,
    SoPhong VARCHAR(10) NOT NULL REFERENCES dbo.Phong(SoPhong),
    MaKH INT NOT NULL REFERENCES dbo.KhachHang(MaKH),
    NgayNhanPhong DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayTraPhong DATETIME2 NULL,
    TongTienThanhToan DECIMAL(18,2) NOT NULL DEFAULT 0
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_SuDungDichVu_Phieu')
    ALTER TABLE dbo.SuDungDichVu ADD CONSTRAINT FK_SuDungDichVu_Phieu FOREIGN KEY (MaPhieu) REFERENCES dbo.PhieuDatPhong(MaPhieu);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_SuDungDichVu_DichVu')
    ALTER TABLE dbo.SuDungDichVu ADD CONSTRAINT FK_SuDungDichVu_DichVu FOREIGN KEY (MaDV) REFERENCES dbo.DichVu(MaDV);
GO
IF NOT EXISTS(SELECT 1 FROM dbo.Phong)
    INSERT dbo.Phong(SoPhong,LoaiPhong,GiaTheoGio,TrangThai) VALUES
    ('P101',N'Đơn',120000,N'Trống'),('P102',N'Đơn',120000,N'Trống'),
    ('P201',N'Đôi',180000,N'Trống'),('P202',N'Đôi',180000,N'Trống'),('P301',N'VIP',350000,N'Trống');
IF NOT EXISTS(SELECT 1 FROM dbo.DichVu)
    INSERT dbo.DichVu(TenDV,DonGia) VALUES (N'Nước suối',15000),(N'Giặt là',50000),(N'Ăn sáng',80000);
GO
