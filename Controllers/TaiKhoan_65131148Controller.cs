using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBanPhuKienCauLong_65131148.Models;

namespace WebsiteBanPhuKienCauLong_65131148.Controllers
{
    public class TaiKhoan_65131148Controller : Controller
    {
        // Khởi tạo đối tượng kết nối Database
        private QuanLyBanPhuKienCauLong_65131148Entities db = new QuanLyBanPhuKienCauLong_65131148Entities();

        // ==========================================
        // 1. XỬ LÝ ĐĂNG KÝ (Chỉ dành cho Khách hàng)
        // ==========================================

        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangKy(string hoTen, string sdt, string diaChi, string email, string username, string matKhau)
        {
            // Kiểm tra xem tên đăng nhập đã tồn tại trong bảng Khách hàng hay chưa
            var checkExist = db.KhachHangs.FirstOrDefault(x => x.username == username);
            if (checkExist != null)
            {
                ViewBag.Error = "Tên đăng nhập này đã có người sử dụng!";
                return View();
            }

            // Tạo đối tượng Khách hàng mới từ các biến nhận được
            KhachHang kh = new KhachHang();
            kh.hoTen = hoTen;
            kh.sdt = sdt;
            kh.diaChi = diaChi;
            kh.email = email;
            kh.username = username;
            kh.matKhau = matKhau;

            db.KhachHangs.Add(kh);
            db.SaveChanges();

            // Đăng ký xong thì chuyển sang trang Đăng nhập
            return RedirectToAction("DangNhap");
        }

        // ==========================================
        // 2. XỬ LÝ ĐĂNG NHẬP & PHÂN QUYỀN
        // ==========================================

        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(string username, string matKhau)
        {
            // Bước 1: Kiểm tra trong bảng NHÂN VIÊN trước (Quyền quản trị)
            var nv = db.NhanViens.FirstOrDefault(x => x.username == username && x.matKhau == matKhau);
            if (nv != null)
            {
                // Lưu thông tin vào Session
                Session["User"] = nv;
                Session["MaID"] = nv.maNhanVien;
                Session["HoTen"] = nv.hoTen;
                Session["Role"] = "Admin"; // Gán nhãn quyền Admin

                // Chuyển hướng đến trang quản lý của Admin
                return RedirectToAction("TrangChu", "Admin_65131148");
            }

            // Bước 2: Nếu không phải nhân viên, kiểm tra bảng KHÁCH HÀNG
            var kh = db.KhachHangs.FirstOrDefault(x => x.username == username && x.matKhau == matKhau);
            if (kh != null)
            {
                // Lưu thông tin vào Session
                Session["User"] = kh;
                Session["MaID"] = kh.maKhachHang;
                Session["HoTen"] = kh.hoTen;
                Session["Role"] = "Customer"; // Gán nhãn quyền Khách hàng

                // Chuyển hướng về trang chủ của khách hàng
                return RedirectToAction("TrangChu", "HangHoas_65131148");
            }

            // Nếu không khớp bảng nào thì báo lỗi
            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng!";
            return View();
        }

        // ==========================================
        // 3. XỬ LÝ ĐĂNG XUẤT
        // ==========================================
        public ActionResult DangXuat()
        {
            Session.Clear(); // Xóa sạch Session
            return RedirectToAction("TrangChu", "HangHoas_65131148");
        }

        // 1. HIỂN THỊ TRANG THÔNG TIN
        public ActionResult ThongTinTaiKhoan()
        {
            // Kiểm tra đăng nhập
            if (Session["User"] == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan_65131148");
            }

            // Lấy thông tin mới nhất từ DB (tránh dùng Session cũ)
            var khSession = (KhachHang)Session["User"];
            var kh = db.KhachHangs.Find(khSession.maKhachHang);

            // 🔥 LẤY DANH SÁCH SẢN PHẨM ĐÃ MUA THÀNH CÔNG
            // Logic: Vào bảng Chi tiết đơn -> Lọc theo Mã khách & Trạng thái Hoàn thành -> Lấy thông tin Hàng hóa -> Distinct (Lọc trùng)
            var sanPhamDaMua = db.ChiTietDonHangs
                                 .Where(ct => ct.DonHang.maKhachHang == kh.maKhachHang
                                           && ct.DonHang.trangThai == "Hoàn thành")
                                 .Select(ct => ct.HangHoa)
                                 .Distinct() // Một món mua nhiều lần thì chỉ hiện 1 lần
                                 .ToList();

            ViewBag.SanPhamDaMua = sanPhamDaMua; // Gửi sang View

            return View(kh);
        }

        // 2. XỬ LÝ CẬP NHẬT THÔNG TIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatThongTin(KhachHang model)
        {
            var kh = db.KhachHangs.Find(model.maKhachHang);
            if (kh != null)
            {
                // Cập nhật các trường cho phép sửa
                kh.hoTen = model.hoTen;
                kh.sdt = model.sdt;
                kh.diaChi = model.diaChi;

                // Email thường là định danh, hạn chế cho sửa hoặc phải xác thực lại
                // kh.email = model.email; 

                db.SaveChanges();

                // Cập nhật lại Session
                Session["User"] = kh;
                TempData["Success"] = "Cập nhật thông tin thành công!";
            }
            return RedirectToAction("ThongTinTaiKhoan");
        }

        // 3. XỬ LÝ ĐỔI MẬT KHẨU
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiMatKhau(int maKhachHang, string matKhauCu, string matKhauMoi, string xacNhanMatKhau)
        {
            var kh = db.KhachHangs.Find(maKhachHang);

            // 1. Kiểm tra mật khẩu cũ
            if (kh.matKhau != matKhauCu) // Lưu ý: Nếu có mã hóa MD5/SHA thì phải mã hóa matKhauCu trước khi so sánh
            {
                TempData["ErrorPass"] = "Mật khẩu hiện tại không đúng!";
                return RedirectToAction("ThongTinTaiKhoan");
            }

            // 2. Kiểm tra xác nhận mật khẩu
            if (matKhauMoi != xacNhanMatKhau)
            {
                TempData["ErrorPass"] = "Mật khẩu xác nhận không khớp!";
                return RedirectToAction("ThongTinTaiKhoan");
            }

            // 3. Kiểm tra độ dài (Tùy chọn)
            if (matKhauMoi.Length < 6)
            {
                TempData["ErrorPass"] = "Mật khẩu mới phải từ 6 ký tự trở lên!";
                return RedirectToAction("ThongTinTaiKhoan");
            }

            // 4. Lưu mật khẩu mới
            kh.matKhau = matKhauMoi; // Nhớ mã hóa nếu hệ thống có dùng mã hóa
            db.SaveChanges();

            TempData["SuccessPass"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ThongTinTaiKhoan");
        }
    }
}