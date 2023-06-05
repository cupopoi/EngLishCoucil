
use master
drop database TrungTamTA
create database TrungTamTA
use TrungTamTA

CREATE TABLE [Admin] (
  [IDTaiKhoan] INT PRIMARY KEY IDENTITY ,
  [TaiKhoan] VARCHAR(50) ,
  [MatKhau] VARCHAR(50) NOT NULL,
);

CREATE TABLE [TaiKhoan] (
  [IDTaiKhoan] INT PRIMARY KEY IDENTITY ,
  [TaiKhoan] VARCHAR(50) ,
  [MatKhau] VARCHAR(50) NOT NULL,
);

CREATE TABLE [ChuongTrinhHoc] (
  IDChuongTrinh INT PRIMARY KEY IDENTITY ,
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
  IDGiangvien INT PRIMARY KEY IDENTITY ,
  TenGiangVien NVARCHAR(50),
  DiaChi  NVARCHAR(MAX),
  SoDienThoai VARCHAR(20),
  Email VARCHAR(50) UNIQUE,
  Hinh VARCHAR(MAX),
  BangCap  NVARCHAR(MAX),
  Luong  FLOAT,
  IDTaiKhoan INT UNIQUE,
   FOREIGN KEY ([IDTaiKhoan]) REFERENCES [TaiKhoan]([IDTaiKhoan]),
);

CREATE TABLE [LopHoc] (
  IDLophoc INT PRIMARY KEY IDENTITY ,
  TenLopHoc NVARCHAR(50),
  IDGiangVien INT,
  IDChuongTrinh INT,
  SoLuong INT,
  FOREIGN KEY (IDGiangVien) REFERENCES GiangVien(IDGiangvien),
  FOREIGN KEY (IDChuongTrinh) REFERENCES ChuongTrinhHoc(IDChuongTrinh),
);

CREATE TABLE [HocVien] (
  IDHocvien INT PRIMARY KEY IDENTITY ,
  TenHocVien NVARCHAR(50),
  NgaySinh DATETIME,
  DiaChi NVARCHAR(50),
  SoDienThoai NVARCHAR(50),
  Email NVARCHAR(50) UNIQUE,
  Hinh NVARCHAR(MAX),
  IDTrangThai INT,
  IDTaiKhoan INT,
  NgayDangKy DATETIME,
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
	[DaThanhToan] BIT,
	[NgayNopTien] DATETIME,
	PRIMARY KEY (IDLophoc, IDHocVien),
    FOREIGN KEY (IDHocVien) REFERENCES HocVien(IDHocVien),
    FOREIGN KEY (IDLophoc) REFERENCES LopHoc(IDLophoc)
);
CREATE TABLE [CacNgayTrongTuan] (
	IDNgay INT PRIMARY KEY,
	TenNgay NVARCHAR(50),
);
 
CREATE TABLE [LichHoc] (
	IDLichhoc INT PRIMARY KEY IDENTITY ,
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


----------------------------------THÊM DỮ LIỆU--------------------------
------------THÊM ACCOUNT ADMIN----------------
INSERT INTO [Admin]
VALUES (N'admin',N'admin');
INSERT INTO [TaiKhoan]
VALUES (N'kientran',N'e10adc3949ba59abbe56e057f20f883e');
INSERT INTO [TaiKhoan]
VALUES (N'kientran1',N'e10adc3949ba59abbe56e057f20f883e');

------------THÊM TRẠNG THÁI HỌC VIÊN----------------
INSERT INTO [TrangThaiHV]
VALUES (1, N'Đăng ký học');
INSERT INTO [TrangThaiHV]
VALUES (2, N'Đang học');
INSERT INTO [TrangThaiHV]
VALUES (3, N'Bỏ học');
INSERT INTO [TrangThaiHV]
VALUES (4, N'Nợ học phí');
INSERT INTO [TrangThaiHV]
VALUES (5, N'Bảo lưu');
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
VALUES ( N'Kiên',N'Châu thành','0976391970',N'kt78139@gmail.com',null,null,100000,null);
---------------------------------------------
------------THÊM HỌC VIÊN----------------
--UPDATE [HocVien] SET IDTaiKhoan = null WHERE IDTaiKhoan= 1  ;
--drop table [HocVien];
INSERT INTO [HocVien]
VALUES ( N'Kiên Trần',28/05/2004,N'Châu thành Tây Ninh','0976391970',N'kt78139@gmail.com',null,1,'Ielts 2.5',1,28/05/2023);
INSERT INTO [HocVien]
VALUES ( N'Kiên Trần1',16/10/2002,N'Châu thành Tây Ninh1','0976391970',N'kt781390@gmail.com',null,1,'Ielts 2.5',null,null);
------------THÊM CHƯƠNG TRÌNH HỌC----------------
INSERT INTO [ChuongTrinhHoc]
VALUES ( N'Ielts 2.5',N'2',N'3',10000,N'Dành cho các bạn IELTS 2.5');
-------------------------------------------------
------------THÊM LỊCH HỌC----------------
INSERT INTO [LichHoc]
VALUES ( N'07:00',N'09:00',1,30/05/2023);
--------------------------------------
------------THÊM LỚP HỌC----------------
INSERT INTO [LopHoc]
VALUES ( N'Ielts 2.5',1,1,30);
------------CHI TIẾT LỚP HỌC----------------
INSERT INTO [ChiTietLopHoc]
VALUES (1,1,6.5,6.5,5.0,7.5,0,null);
--UPDATE [ChiTietLopHoc] SET DaThanhToan = 0 WHERE IDHocVien= 1  ;

----------------------------------
------------THÊM CHI TIẾT LỊCH HỌC----------------
INSERT INTO [ChiTietLichHoc]
VALUES (1,1);
INSERT INTO [ChiTietLichHoc]
VALUES (1,2);
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
