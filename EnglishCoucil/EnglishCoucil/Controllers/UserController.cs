﻿using EnglishCoucil.Areas.Admin.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace EnglishCoucil.Controllers
{
    public class UserController : Controller
    {
        dbQLtrungtamDataContext data = new dbQLtrungtamDataContext();
        #region MD5 encrypt
        // GET: User
        private string mahoamd5(string input)
        {
            using (var md5 = MD5.Create())
            {
                var dulieu = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var builder = new StringBuilder();

                for (int i = 0; i < dulieu.Length; i++)
                {
                    builder.Append(dulieu[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        #endregion

        #region check info
        public static bool checkkitu(string input)
        {
            char[] specialChar = { ' ', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '+', '=', '{', '}', '[', ']', '|', '\\', ':', ';', '\"', '\'', '<', '>', ',', '.', '?', '/' };
            foreach (char item in input)
            {
                if (specialChar.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool checkkhoangtrang(string input)
        {
            return input.Contains(" ");
        }
        #endregion

        #region Register
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(FormCollection collection, TaiKhoan user)
        {
            var Username = collection["Username"];
            var Password = collection["Password"];
            var PasswordConfirm = collection["PasswordConfirm"];
            if (String.IsNullOrEmpty(Username))
            {
                ViewData["Loi1"] = "UserName not Blank!";
            }
            else if (checktk(Username))
            {
                ViewData["Loi1"] = "UserName already exist!";
            }
            else if (checkkhoangtrang(Username))
            {
                ViewData["Loi1"] = "UserName shouldn't have space!";
            }
            else if (checkkitu(Username))
            {
                ViewData["Loi1"] = "UserName shouldn't have special character!";
            }
            else if (String.IsNullOrEmpty(Password))
            {
                ViewData["Loi2"] = "Password not Blank!";
            }
       
            else if (checkkhoangtrang(Password))
            {
                ViewData["Loi2"] = "Password shouldn't have space!";
            }
            else if (String.IsNullOrEmpty(PasswordConfirm))
            {
                ViewData["Loi3"] = "Must confirm Password!";
            }
            else if (PasswordConfirm != Password)
            {
                ViewData["Loi3"] = "Confirm Password doesn't match!";
            }
            else
            {
                user.TaiKhoan1 = Username;
                //lưu mật khẩu dưới dạng md5
                user.MatKhau = mahoamd5(Password);
                data.TaiKhoans.InsertOnSubmit(user);
                data.SubmitChanges();
                return RedirectToAction("Login");
            }
            return this.Register();
        }
        #endregion

        #region Check info
        private bool checktk(string Username)
        {
            return data.TaiKhoans.Count(x => x.TaiKhoan1 == Username) > 0;
        }
        #endregion

        #region Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var Username = collection["Username"];
            var Password = collection["Password"];

            if (String.IsNullOrEmpty(Username))
            {
                ViewData["Loi1"] = "UserName not Blank!";
            }
            else if (String.IsNullOrEmpty(Password))
            {
                ViewData["Loi2"] = "Password not Blank!";
            }
            else
            {
                TaiKhoan user = data.TaiKhoans.SingleOrDefault(n => n.TaiKhoan1 == Username && n.MatKhau == mahoamd5(Password));
                if (user != null)
                {
                    Session["IDtk"] = user.IDTaiKhoan;
                    Session["User"] = user;
                    Session["User_name"] = user.TaiKhoan1;
                    return RedirectToAction("Index", "Home");
                }
                else if (!checktk(Username))
                {
                    ViewBag.Thongbao = "UserName already exist!";
                }
                else
                    ViewBag.Thongbao = "UserName or Password incorrect!";
            }
            return this.Login();
        }
        #endregion

        #region Log Off And Change Info when Login
        public ActionResult Logoff()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult LoginPartial()
        {
            return PartialView("LoginPartial");
        }
        #endregion

        #region funtion check and ProcessUpload img
        private bool IsValidEmail(string email)
        {
            // Biểu thức chính quy kiểm tra định dạng email
            string pattern = @"^[a-zA-Z0-9.-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            // Kiểm tra xem email có trùng khớp với biểu thức chính quy hay không
            return Regex.IsMatch(email, pattern);
        }

        public bool CheckInput(string input)
        {
            // Kiểm tra độ dài của dãy
            if (input.Length > 8)
            {
                return false;
            }

            // Kiểm tra từng ký tự trong dãy
            foreach (char c in input)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckId(int id)
        {
            return data.HocViens.Count(x => x.IDHocvien == id) > 0;

        }


        private bool checkemail(string email)
        {
            return data.GiangViens.Any(x => x.Email == email);
        }


        //được sử dụng để upload hình ảnh
        public string ProcessUpload(HttpPostedFileBase file)
        {  //kiểm tra file nếu không có tệp tin nào được tải lên, và phương thức trả về một chuỗi rỗng ""
            if (file == null)
            {
                return "";
            }
            //nếu không thì sẽ lưu vào đường dẫn và thực hiện phương thức SaveAs ( với đường dẫn là "~/Areas/Admin/Content/images/" + file.FileName)
            file.SaveAs(Server.MapPath("~/Areas/Admin/Content/images/" + file.FileName));
            //Sau khi tệp tin được lưu thành công
            // trả về đường dẫn của tệp tin hình ảnh đã lưu, bắt đầu bằng "/Content/images/" + tên tệp tin file.FileName.
            return "/Areas/Admin/Content/images/" + file.FileName;
        }


        #endregion

        #region Enroll study
        [HttpGet]
        public ActionResult EnrollStudy(int? IDtk)
        {
     
            return  View(); 
        }
        [HttpPost]
        public ActionResult EnrollStudy(FormCollection collection, HocVien hocvien, int IDtk)
        {
            var idhocvien = collection["IDHocvien"];
            var tenhocvien = collection["Tenhocvien"];
            var diachi = collection["DiaChi"];
            var sodt = collection["Phone"];
            var email = collection["Email"];

           if (string.IsNullOrEmpty(tenhocvien))
            {
                ViewData["Loi2"] = "Vui lòng nhập tên học viên";
            }

            else if (string.IsNullOrEmpty(Request.Form["NgaySinh"]))
            {
                ViewData["Loi3"] = "Vui lòng nhập ngày sinh";
            }
            else if (!DateTime.TryParseExact(Request.Form["NgaySinh"], "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                ViewData["Loi3"] = "Ngày sinh không hợp lệ";
            }
            else if (string.IsNullOrEmpty(diachi))
            {
                ViewData["Loi4"] = "Vui lòng nhập địa chỉ";
            }
            else if (string.IsNullOrEmpty(sodt))
            {
                ViewData["Loi5"] = "Vui lòng nhập số điện thoại";
            }
            else if (string.IsNullOrEmpty(email))
            {
                ViewData["Loi6"] = "Vui lòng nhập email";
            }
            else if (checkemail(email))
            {
                ViewData["Loi6"] = "Email đã có";
            }
            else if (!IsValidEmail(email))
            {
                ViewData["Loi6"] = "Email không hợp lệ";
            }
            else
            {
                hocvien.IDHocvien = int.Parse(idhocvien);
                hocvien.TenHocVien = tenhocvien;
                // Chuyển đổi chuỗi thành kiểu DateTime với định dạng "dd/MM/yyyy"
                hocvien.NgaySinh = DateTime.ParseExact(Request.Form["NgaySinh"], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                hocvien.DiaChi = diachi;
                hocvien.SoDienThoai = sodt;
                hocvien.Email = email;
                hocvien.Hinh = collection["Hinh"];

                data.HocViens.InsertOnSubmit(hocvien);
                data.SubmitChanges();
                return RedirectToAction("Hocvien");
            }
            return this.EnrollStudy(IDtk);
        }
        #endregion

        #region Show time table 
        public ActionResult StudentTimetable(int IDhv)
        {
            ViewBag.IDhv = IDhv;
            var nameHv = data.HocViens.FirstOrDefault(x => x.IDHocvien == IDhv).TenHocVien.ToString();
            ViewBag.nameHv = nameHv;
            //danh sách rỗng của ChiTietLichHoc được khởi tạo để lưu trữ thông tin về lịch học của sinh viên.
            List<ChiTietLichHoc> listChiTietLichHoc = new List<ChiTietLichHoc>();
            // Danh sách ChiTietLopHoc của sinh viên được lấy từ cơ sở dữ liệu có IDHocVien bằng với IDhocvien được truyền vào.
            List<ChiTietLopHoc> listChiTietLopHoc = data.ChiTietLopHocs.Where(item => item.IDHocVien == IDhv).ToList();
            //lặp qua từng đối tượng ChiTietLopHoc trong danh sách.
            foreach (ChiTietLopHoc item in listChiTietLopHoc)
            {
                //LopHoc tương ứng với IDLophoc của đối tượng ChiTietLopHoc được lấy từ cơ sở dữ liệu với điều kiện là IDLophoc 
                LopHoc lopHoc = data.LopHocs.SingleOrDefault(x => x.IDLophoc == item.IDLophoc);
                if (lopHoc != null)
                {
                    //ChitietLichHoc tương ứng với LopHoc đó được lấy từ cơ sở dữ liệu IDLophoc
                    List<ChiTietLichHoc> listChiTietLichHocForLop = data.ChiTietLichHocs.Where(x => x.IDLophoc == lopHoc.IDLophoc).ToList();
                    foreach (ChiTietLichHoc chiTietLichHoc in listChiTietLichHocForLop)
                    {
                        listChiTietLichHoc.Add(chiTietLichHoc);
                    }
                }
            }

            return View(listChiTietLichHoc);
        }
        #endregion
    }
}