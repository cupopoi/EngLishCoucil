using Antlr.Runtime.Misc;
using EnglishCoucil.Areas.Admin.Data;
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
using EnglishCoucil.Models;
using System.Data.Linq;
using PayPal.Api;

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
                ViewData["Loi1"] = "Username not blank!";
            }
            else if (String.IsNullOrEmpty(Password))
            {
                ViewData["Loi2"] = "Password not blank!";
            }
            else
            {
                TaiKhoan user = data.TaiKhoans.SingleOrDefault(n => n.TaiKhoan1 == Username && n.MatKhau == mahoamd5(Password));
                if (user != null)
                {
                    Session["IDtk"] = user.IDTaiKhoan;
                    Session["User"] = user;
                    Session["User_name"] = user.TaiKhoan1;

                    bool hasClass = false;

                    // Lấy thông tin HocVien dựa trên IDTaiKhoan từ bảng TaiKhoan
                    HocVien hocVien = data.HocViens.FirstOrDefault(hv => hv.IDTaiKhoan == user.IDTaiKhoan);
                    if (hocVien != null)
                    {
                        Session["IDhv"] = hocVien.IDHocvien;

                        // Kiểm tra xem có thông tin lớp học hay không
                        ChiTietLopHoc chiTietLopHoc = data.ChiTietLopHocs.FirstOrDefault(chitiet => chitiet.IDHocVien == hocVien.IDHocvien);
                        if (chiTietLopHoc != null)
                        {
                            hasClass = true;
                            Session["IDlh"] = chiTietLopHoc.IDLophoc;
                        }
                    }

                    // Kiểm tra nếu có lớp học hoặc không yêu cầu lớp học
                    if (hasClass)
                    {
                        // Tiếp tục xử lý và chuyển hướng đến trang Index của HomeController
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // Trường hợp không có lớp học
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ViewBag.Thongbao = "Username or Password incorrect!";
                }
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
        public ActionResult EnrollStudy()
        {
            if (Session["User"] != null)
            {
            return View();
            }
            return RedirectToAction("Register");
        }
        [HttpPost]
        public ActionResult EnrollStudy(FormCollection collection, HocVien hocvien,ChiTietLopHoc chiTietLopHoc, int IDtk,int IDlh)
        {
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
               
                hocvien.TenHocVien = tenhocvien;
                // Chuyển đổi chuỗi thành kiểu DateTime với định dạng "dd/MM/yyyy"
                //Tạo học viên và lưu vào data 
                hocvien.NgaySinh = DateTime.ParseExact(Request.Form["NgaySinh"], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                hocvien.DiaChi = diachi;
                hocvien.SoDienThoai = sodt;
                hocvien.Email = email;
                hocvien.Hinh = collection["Hinh"];
                hocvien.IDTrangThai = 2;
                hocvien.IDTaiKhoan = IDtk;
                DateTime currentDateTime = DateTime.Now;
                hocvien.NgayDangKy = currentDateTime;
                data.HocViens.InsertOnSubmit(hocvien);
                data.SubmitChanges();
                //từ học viên mới tạo lấy id hocvien và id lop hoc
                var id = data.HocViens.Single(s => s.IDTaiKhoan == IDtk).IDHocvien;
                if(id != 0)
                {
                 chiTietLopHoc.IDHocVien = id;
                chiTietLopHoc.IDLophoc = IDlh;      
                chiTietLopHoc.DaThanhToan = false;
                data.ChiTietLopHocs.InsertOnSubmit(chiTietLopHoc);
                data.SubmitChanges();
                }
                else
                {
                    ViewBag.Success = "Join UnSuccess";
                    return RedirectToAction("EnrollStudy");
                }
                
                ViewBag.Success = "Join Success";
                return RedirectToAction("EnrollStudy");
            }
            return this.EnrollStudy();
        }
        #endregion

        #region Show time table 
        public ActionResult StudentTimetable(int? IDhv)
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
            if (listChiTietLichHoc.Count == 0)
            {
                ViewBag.Message = "There is no Schedule for you, you are free.";
            }
            return View(listChiTietLichHoc);
        }
        #endregion

        #region Study score
        public ActionResult viewScore(int IDhv)
        {
            ViewBag.IDhv = IDhv;
            string namehv = data.HocViens.FirstOrDefault(x => x.IDHocvien == IDhv)?.TenHocVien;
            ViewBag.namehv = namehv;
            var scores = data.ChiTietLopHocs.Where(x => x.IDHocVien == IDhv).ToList();
           return View(scores);
        }
        #endregion

        #region All Cource
        public ActionResult AllCource()
        {
            var chuongtrinh = new List<ViewAllCouceClass>();
            foreach (var item in data.LopHocs) {
                ViewAllCouceClass classData = new ViewAllCouceClass();
                classData.IDLopHoc = item.IDLophoc;
                classData.TenLopHoc = item?.TenLopHoc;
                classData.TenChuongTrinh = item.ChuongTrinhHoc.TenChuongTrinh;
                classData.ThoiLuong = item.ChuongTrinhHoc.ThoiLuong;
                classData.SoBuoiHoc = item.ChuongTrinhHoc.SoBuoiHoc;
                classData.SoLuong = item.SoLuong;
                classData.MoTa = item.ChuongTrinhHoc.MoTa;
                classData.GiaTien = item.ChuongTrinhHoc.GiaTien;
                classData.Count = data.ChiTietLopHocs.Count(x => x.IDLophoc == item.IDLophoc);
                chuongtrinh.Add(classData);
            }
            return View(chuongtrinh);
        }
        #endregion

        #region Cource in Student
        public ActionResult CourceShow()
        {
            List<ChiTietLopHoc> listChiTietLopHoc = data.ChiTietLopHocs.Where(item => item.IDHocVien == (int)Session["IDhv"]).ToList();
            double? totalPrice = 0;

            foreach (ChiTietLopHoc item in listChiTietLopHoc)
            {
                LopHoc lopHoc = data.LopHocs.SingleOrDefault(x => x.IDLophoc == item.IDLophoc);
                if (lopHoc != null)
                {
                    item.LopHoc = lopHoc; // Gán đối tượng LopHoc cho ChiTietLopHoc để sử dụng trong view
                    totalPrice += lopHoc.ChuongTrinhHoc.GiaTien;
                }
            }

            ViewBag.totalPrice = totalPrice;
            ViewBag.totalCource = listChiTietLopHoc.Count;

            if (listChiTietLopHoc.Count == 0)
            {
                ViewBag.Message = "There is no timetable for you, you are free.";
            }

            return View(listChiTietLopHoc);
        }

        #endregion

        #region Pay Cource
        public ActionResult PayCource(int? idlh, int? IDtk, int? IDhv)
        {
            List<ChiTietLopHoc> listChiTietLopHoc = new List<ChiTietLopHoc>();
            List<ChiTietLopHoc> listchuongtrinh = data.ChiTietLopHocs.Where(item => item.IDHocVien == IDhv).ToList();
            foreach (ChiTietLopHoc item in listchuongtrinh)
            {
                LopHoc lopHoc = data.LopHocs.SingleOrDefault(x => x.IDLophoc == item.IDLophoc);
                if (lopHoc != null)
                {
                    List<ChiTietLopHoc> ChuongtrinhForClass = data.ChiTietLopHocs.Where(x => x.IDLophoc == lopHoc.IDLophoc).ToList();
                    foreach (ChiTietLopHoc payment in ChuongtrinhForClass)
                    {
                        if (payment.DaThanhToan == false)
                        {
                            payment.DaThanhToan = true;
                            payment.NgayNopTien = DateTime.Now;
                            data.SubmitChanges();
                            Session["CheckPay"] = payment.DaThanhToan;
                            ViewBag.Message = "Payment Success";
                            return RedirectToAction("CourceShow", new { idlh, IDtk, IDhv });
                        }
                    }
                   
                }
            }
            Session["CheckPay"] = null;
            return RedirectToAction("CourceShow", new { idlh, IDtk, IDhv });
        }
        public ActionResult paySuccess()
        {
            return View();
        }

        #endregion

        #region Pay online
        //private double TongTien()
        //{
        //    double? sum = 0;
        //    List<ChiTietLopHoc> listChiTietLopHoc = new List<ChiTietLopHoc>();
        //    List<ChiTietLopHoc> listchuongtrinh = data.ChiTietLopHocs.Where(item => item.IDHocVien == (int)Session["IDhv"]).ToList();
        //    foreach (ChiTietLopHoc item in listchuongtrinh)
        //    {
        //        LopHoc lopHoc = data.LopHocs.SingleOrDefault(x => x.IDLophoc == item.IDLophoc);
        //        if (lopHoc != null)
        //        {
        //            List<ChiTietLopHoc> ChuongtrinhForClass = data.ChiTietLopHocs.Where(x => x.IDLophoc == lopHoc.IDLophoc).ToList();
        //            foreach (ChiTietLopHoc payment in ChuongtrinhForClass)
        //            {
        //                if (payment.DaThanhToan != false)
        //                {
        //                    sum += payment.LopHoc.ChuongTrinhHoc.GiaTien;
        //                }
        //            }
        //        }

        //    }
        //    return double.Parse(sum.ToString());
        //}
        //public ActionResult PaymentWithPaypal()
        //{
        //    APIContext apiContext = PaypalConfiguration.GetAPIContext();
        //    try
        //    {
        //        string payerId = Request.Params["PayerID"];
        //        if (string.IsNullOrEmpty(payerId))
        //        {
        //            string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/User/PaymentWithPaypal?";

        //            var guid = Convert.ToString((new Random()).Next(100000));

        //            var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);

        //            var links = createdPayment.links.GetEnumerator();
        //            string paypalRedirectUrl = null;
        //            while (links.MoveNext())
        //            {
        //                Links lnk = links.Current;
        //                if (lnk.rel.ToLower().Trim().Equals("approval_url"))
        //                {
        //                    paypalRedirectUrl = lnk.href;
        //                }
        //            }

        //            Session.Add(guid, createdPayment.id);
        //            return Redirect(paypalRedirectUrl);
        //        }
        //        else
        //        {
        //            var guid = Request.Params["guid"];
        //            var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);

        //            if (executedPayment.state.ToLower() != "approved")
        //            {
        //                throw new Exception();
        //            }
        //            else
        //            {
        //                List<ChiTietLopHoc> listChiTietLopHoc = new List<ChiTietLopHoc>();
        //                List<ChiTietLopHoc> listchuongtrinh = data.ChiTietLopHocs.Where(item => item.IDHocVien == (int)Session["IDhv"]).ToList();
        //                foreach (ChiTietLopHoc item in listchuongtrinh)
        //                {
        //                    LopHoc lopHoc = data.LopHocs.SingleOrDefault(x => x.IDLophoc == item.IDLophoc);
        //                    if (lopHoc != null)
        //                    {
        //                        List<ChiTietLopHoc> ChuongtrinhForClass = data.ChiTietLopHocs.Where(x => x.IDLophoc == lopHoc.IDLophoc).ToList();
        //                        foreach (ChiTietLopHoc payment in ChuongtrinhForClass)
        //                        {
        //                            if (payment.DaThanhToan == false)
        //                            {
        //                                payment.DaThanhToan = true;
        //                                payment.NgayNopTien = DateTime.Now;
        //                                data.SubmitChanges();
        //                                Session["CheckPay"] = payment.DaThanhToan;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Loi = ex.Message;
        //        ViewBag.Error = ex.StackTrace;
        //        return View("payError");
        //    }

        //    return RedirectToAction("paySuccess");
        //}

        //private PayPal.Api.Payment payment;

        //private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        //{
        //    var paymentExecution = new PaymentExecution()
        //    {
        //        payer_id = payerId
        //    };
        //    this.payment = new Payment()
        //    {
        //        id = paymentId
        //    };
        //    return this.payment.Execute(apiContext, paymentExecution);
        //}

        //private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        //{
        //    // Tạo itemList và thêm các item vào itemList
        //    var itemList = new ItemList()
        //    {
        //        items = new List<Item>()
        //    };

        //    LopHoc lopHoc = data.LopHocs.SingleOrDefault(x => x.IDLophoc == (int)Session["IDlh"]);
        //    if (lopHoc != null)
        //    {
        //        List<ChiTietLopHoc> ChuongtrinhForClass = data.ChiTietLopHocs.Where(x => x.IDLophoc == lopHoc.IDLophoc).ToList();
        //        foreach (ChiTietLopHoc payment in ChuongtrinhForClass)
        //        {
        //            decimal itemPrice = decimal.Parse(payment.LopHoc.ChuongTrinhHoc.GiaTien.ToString());

        //            itemList.items.Add(new Item()
        //            {
        //                name = payment.LopHoc.ChuongTrinhHoc.TenChuongTrinh,
        //                currency = "USD",
        //                quantity = "1",
        //                price = itemPrice.ToString("0.00"),
        //                sku = "sku"
        //            });
        //        }
        //    }

        //    // Tạo thông tin thanh toán (amount)
        //    var amount = new Amount()
        //    {
        //        currency = "USD",
        //        total = TongTien().ToString("0.00"), // Thay bằng tổng tiền của itemList
        //        details = new Details()
        //        {
        //            subtotal = TongTien().ToString("0.00"), // Thay bằng tổng tiền của itemList
        //        }
        //    };

        //    // Tạo transaction và sử dụng thông tin itemList và amount đã tạo
        //    var transactionList = new List<Transaction>();
        //    transactionList.Add(new Transaction()
        //    {
        //        description = "Transaction description",
        //        invoice_number = Convert.ToString((new Random()).Next(100000)),
        //        amount = amount,
        //        item_list = itemList
        //    });

        //    // Tạo payer và redirect_urls
        //    var payer = new Payer()
        //    {
        //        payment_method = "paypal"
        //    };

        //    var redirUrls = new RedirectUrls()
        //    {
        //        cancel_url = redirectUrl + "&Cancel=true",
        //        return_url = redirectUrl
        //    };

        //    // Tạo payment và sử dụng các thông tin đã tạo
        //    this.payment = new Payment()
        //    {
        //        intent = "sale",
        //        payer = payer,
        //        transactions = transactionList,
        //        redirect_urls = redirUrls
        //    };

        //    return this.payment.Create(apiContext);
        //}


        public ActionResult payError()
        {
            return View();
        }

        #endregion

        #region Show Info
        public ActionResult showInfo( int IDtk)
        {
            var info = new AccountInfo();
            var user = data.HocViens.FirstOrDefault(x => x.IDTaiKhoan == IDtk);
            if (user != null)
            {
                info.IDHocVien = user.IDHocvien;
                info.TenHocVien = user?.TenHocVien;
                info.NgaySinh = user.NgaySinh;
                info.DiaChi = user.DiaChi;
                info.Email = user.Email;
                info.Hinh = user.Hinh;
                info.SoDienThoai = user.SoDienThoai;
                info.NgayDangKy = user.NgayDangKy;
                info.IDTaiKhoan = IDtk;
                info.Username = user.TaiKhoan.TaiKhoan1;
            }
            else
            {
                var userName = data.TaiKhoans.FirstOrDefault(x => x.IDTaiKhoan == IDtk);
                info.IDTaiKhoan = IDtk;
                info.Username = userName.TaiKhoan1;
            }
            return View(info);
        }
        #endregion

        #region Change Info
        [HttpGet]
        public ActionResult EditInfo(int? IDhv, int IDtk)
        {
            var info = new AccountInfo();
            var user= data.HocViens.FirstOrDefault(x => x.IDTaiKhoan == IDtk);
            if (user != null)
            {
                info.IDHocVien = user.IDHocvien;
                info.TenHocVien = user?.TenHocVien;
                info.NgaySinh = user.NgaySinh;
                info.DiaChi = user.DiaChi;
                info.Email = user.Email;
                info.Hinh = user.Hinh;
                info.SoDienThoai = user.SoDienThoai;
                info.NgayDangKy = user.NgayDangKy;
                info.IDTaiKhoan = IDtk;
                info.Username = user.TaiKhoan.TaiKhoan1;
            }
            else
            {
                var userName = data.TaiKhoans.FirstOrDefault(x => x.IDTaiKhoan == IDtk);
                info.IDTaiKhoan = IDtk;
                info.Username = userName.TaiKhoan1;
            }
            return View(info);
        }

        [HttpPost]
        public ActionResult EditInfo(HocVien editHocvien,int? IDhv,  int IDtk)
        {
            if (ModelState.IsValid)
            {
                HocVien hocvien = data.HocViens.SingleOrDefault(c => c.IDHocvien == editHocvien.IDHocvien);
                if (hocvien != null)
                {
                    hocvien.TenHocVien = editHocvien.TenHocVien;
                    hocvien.NgaySinh = editHocvien.NgaySinh;
                    hocvien.DiaChi = editHocvien.DiaChi;
                    hocvien.SoDienThoai = editHocvien.SoDienThoai;
                    hocvien.Email = editHocvien.Email;
                    hocvien.Hinh = editHocvien.Hinh;
                    hocvien.IDTrangThai = 2;
                    data.SubmitChanges();
                    ViewBag.Success = "Change Success";
                    return RedirectToAction("showInfo", new { IDtk });
                }
                else
                {
                    return HttpNotFound();
                }
            }
            else
            {
                ViewBag.Success= "No Infomation";
                return View(editHocvien);
            }
        }
        #endregion
    }
}