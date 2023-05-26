
use master
drop database TrungTamTA
create database TrungTamTA
use TrungTamTA

CREATE TABLE [Admin] (
  [IDTaiKhoan] INT PRIMARY KEY IDENTITY (1,1),
  [TaiKhoan] VARCHAR(50) ,
  [MatKhau] VARCHAR(50) NOT NULL,
);

CREATE TABLE [TaiKhoan] (
  [IDTaiKhoan] INT PRIMARY KEY IDENTITY (1,1),
  [TaiKhoan] VARCHAR(50) ,
  [MatKhau] VARCHAR(50) NOT NULL,
);

CREATE TABLE [ChuongTrinhHoc] (
  IDChuongTrinh INT PRIMARY KEY IDENTITY (1,1),
  TenChuongTrinh NVARCHAR(50),
  SoBuoiHoc NVARCHAR(50),
  ThoiLuong NVARCHAR(50),
  GiaTien FLOAT,
  MoTa NVARCHAR(50)
);

CREATE TABLE [TrangThaiHV] (
	IDTrangThai INT PRIMARY KEY,
	TenTrangThai  NVARCHAR(50),
);

CREATE TABLE [GiangVien] (
  IDGiangvien INT PRIMARY KEY IDENTITY (1,1),
  TenGiangVien NVARCHAR(50),
  DiaChi  NVARCHAR(MAX),
  SoDienThoai VARCHAR(20),
  Email VARCHAR(50) UNIQUE,
  Hinh VARCHAR(MAX),
  BangCap  NVARCHAR(MAX),
  Luong  FLOAT,
  IDTaiKhoan INT,
   FOREIGN KEY ([IDTaiKhoan]) REFERENCES [TaiKhoan]([IDTaiKhoan]),
);

CREATE TABLE [LopHoc] (
  IDLophoc INT PRIMARY KEY IDENTITY (1,1),
  TenLopHoc NVARCHAR(50),
  IDGiangVien INT,
  IDChuongTrinh INT,
  SoLuong INT,
  FOREIGN KEY (IDGiangVien) REFERENCES GiangVien(IDGiangvien),
  FOREIGN KEY (IDChuongTrinh) REFERENCES ChuongTrinhHoc(IDChuongTrinh),
);

CREATE TABLE [HocVien] (
  IDHocvien INT PRIMARY KEY IDENTITY (1,1),
  TenHocVien NVARCHAR(50),
  NgaySinh DATETIME,
  DiaChi NVARCHAR(50),
  SoDienThoai NVARCHAR(50),
  Email NVARCHAR(50) UNIQUE,
  Hinh NVARCHAR(MAX),
  IDTrangThai INT,
  CapDo NVARCHAR(50),
  IDTaiKhoan INT,
   FOREIGN KEY (IDTrangThai) REFERENCES [TrangThaiHV](IDTrangThai),
   FOREIGN KEY ([IDTaiKhoan]) REFERENCES [TaiKhoan]([IDTaiKhoan])
);

CREATE TABLE ChiTietLopHoc (
    IDLophoc INT ,
    IDHocVien INT,
	DiemNghe FLOAT,
	DiemNoi FLOAT,
	DiemViet FLOAT,
	DiemDoc FLOAT,
	DiemTB AS CEILING((DiemNghe + DiemNoi + DiemViet + DiemDoc) / 4.0 * 2) / 2.0,
	PRIMARY KEY (IDLophoc, IDHocVien),
    FOREIGN KEY (IDHocVien) REFERENCES HocVien(IDHocVien),
    FOREIGN KEY (IDLophoc) REFERENCES LopHoc(IDLophoc)
);
CREATE TABLE [CacNgayTrongTuan] (
	IDNgay INT PRIMARY KEY,
	TenNgay NVARCHAR(50),
);
 
CREATE TABLE [LichHoc] (
	IDLichhoc INT PRIMARY KEY IDENTITY (1,1),
	TGBatDau TIME,
    TGKetThuc TIME,
	IDNgay INT,
	Ngay DATETIME,
	FOREIGN KEY (IDNgay) REFERENCES [CacNgayTrongTuan](IDNgay),
);



CREATE TABLE [ChiTietLichHoc] (
	IDLophoc INT,
	IDLichhoc INT,
	 PRIMARY KEY (IDLophoc, IDLichhoc),
	FOREIGN KEY (IDLichhoc) REFERENCES [LichHoc](IDLichhoc),
    FOREIGN KEY (IDLophoc) REFERENCES LopHoc(IDLophoc),
);

----chưa làm  -----
CREATE TABLE [PhuongThucThanhToan](
	[IdThanhToan] INT PRIMARY KEY,
	[TenPhuongThucThanhToan] NVARCHAR(50),
);
CREATE TABLE[HoaDonTT](
	[IdHoaDon] INT PRIMARY KEY IDENTITY (1,1),
	[NgayNopTien] DATETIME ,
	[SoTien] INT,
	[IdThanhToan] INT,
	[IDHocVien] INT,
	[IdLopHoc] INT,
	[DaThanhToan] BIT,
	FOREIGN KEY ([IdThanhToan]) REFERENCES [PhuongThucThanhToan]([IdThanhToan]),
	 FOREIGN KEY (IDHocVien) REFERENCES HocVien(IDHocVien),
    FOREIGN KEY (IDLophoc) REFERENCES LopHoc(IDLophoc)
);


----------THÊM DỮ LIỆU------------------
----------------------------------THÊM DỮ LIỆU
------------THÊM ACCOUNT ADMIN----------------
INSERT INTO [Admin]
VALUES (N'admin',N'admin');
------------THÊM TRẠNG THÁI HỌC VIÊN----------------
INSERT INTO [TrangThaiHV]
VALUES (1, N'Đang học');
INSERT INTO [TrangThaiHV]
VALUES (2, N'Bỏ học');
INSERT INTO [TrangThaiHV]
VALUES (3, N'Nợ học phí');
INSERT INTO [TrangThaiHV]
VALUES (4, N'Bảo lưu');
--UPDATE [TrangThaiHV] SET TenTrangThai = N'Đang học' WHERE IDTrangThai=1 ;
----------------------------------------------------
------------THÊM THỨ---------------
INSERT INTO [CacNgayTrongTuan]
VALUES (1, N'Thứ 2');
INSERT INTO [CacNgayTrongTuan]
VALUES (2, N'Thứ 3');
INSERT INTO [CacNgayTrongTuan]
VALUES (3, N'Thứ 4');
INSERT INTO [CacNgayTrongTuan]
VALUES (4, N'Thứ 5');
INSERT INTO [CacNgayTrongTuan]
VALUES (5, N'Thứ 6');
INSERT INTO [CacNgayTrongTuan]
VALUES (6, N'Thứ 7');
INSERT INTO [CacNgayTrongTuan]
VALUES (7, N'Chủ Nhật');
--------------------------------------------------------
------------THÊM GIẢNG VIÊN----------------
INSERT INTO [GiangVien]
VALUES (112, N'Kiên',N'Châu thành','0976391970',N'kt78139@gmail.com',null,null,100000,null);
---------------------------------------------
------------THÊM HỌC VIÊN----------------
INSERT INTO [HocVien]
VALUES (1112, N'Kiên Trần',2006/10/16,N'Châu thành Tây Ninh','0976391970',N'kt78139@gmail.com',null,1,'2.5 toeic',null);
INSERT INTO [HocVien]
VALUES (11112, N'Kiên Trần1',2002/10/16,N'Châu thành Tây Ninh1','0976391970',N'kt781390@gmail.com',null,1,'2.5 toeic',null);
------------THÊM CHƯƠNG TRÌNH HỌC----------------
INSERT INTO [ChuongTrinhHoc]
VALUES (1, N'Toeic 2.5',N'2 Tuần',N'3 Tháng',10000,N'Dành cho các bạn TOEIC 2.5');
-------------------------------------------------
------------THÊM LỊCH HỌC----------------
INSERT INTO [LichHoc]
VALUES ( N'07:00',N'09:00',1,16/10/2002);
--------------------------------------
------------THÊM LỚP HỌC----------------
INSERT INTO [LopHoc]
VALUES (111, N'TOEIC 2.5',112,1,30);
------------CHI TIẾT LỚP HỌC----------------
INSERT INTO [ChiTietLopHoc]
VALUES (111,1112,6.5,6.5,5.0,7.5);

----------------------------------
------------THÊM CHI TIẾT LỊCH HỌC----------------
INSERT INTO [ChiTietLichHoc]
VALUES (111,11);
INSERT INTO [ChiTietLichHoc]
VALUES (111,13);
----------------------------------
-----------------------CHỌN BẢNG-------------------------
select * from [TrangThaiHV]
select * from [CacNgayTrongTuan]

	select * from [TaiKhoan]
	select * from [HocVien]
	select * from [GiangVien]
	select * from [ChuongTrinhHoc]
	select * from [LichHoc]
	select * from [LopHoc]
	select * from [ChiTietLopHoc]
	select * from [ChiTietLichHoc]
