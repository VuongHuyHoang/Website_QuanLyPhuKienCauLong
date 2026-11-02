using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using WebsiteBanPhuKienCauLong_65131148.Models;

namespace WebsiteBanPhuKienCauLong_65131148.Controllers
{
    public class HangHoas_65131148Controller : Controller
    {
        private QuanLyBanPhuKienCauLong_65131148Entities db = new QuanLyBanPhuKienCauLong_65131148Entities();

        // GET: HangHoas_65131148
        public ActionResult TrangChu()
        {
            var sanpham = db.HangHoas.Take(10).ToList();
            return View(sanpham);
        }
        public ActionResult TrangSanPham(string keyword, List<int> maHang, List<int> maDanhMuc, decimal? giaMin, decimal? giaMax, string sortOrder, int page = 1)
        {
            int pageSize = 20;

            var query = db.HangHoas
                          .Include("HangHoa_Size.Size") // Nạp dữ liệu size để dùng cho Popup Modal
                          .Where(x => x.soLuongTon >= 0)
                          .AsQueryable();

            //1  Tìm kiếm
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.tenHangHoa.Contains(keyword));
            }

            // 2️ Lọc theo hãng
            if (maHang != null && maHang.Any())
                query = query.Where(x => maHang.Contains(x.maHang.Value));

            // 3️ Lọc theo danh mục
            if (maDanhMuc != null && maDanhMuc.Any())
                query = query.Where(x => maDanhMuc.Contains(x.maDanhMuc));

            // 4️ Lọc theo giá
            if (giaMin.HasValue)
                query = query.Where(x => x.gia >= giaMin);

            if (giaMax.HasValue)
                query = query.Where(x => x.gia <= giaMax);

            // 5️ SẮP XẾP (Phải làm TRƯỚC khi phân trang)
            switch (sortOrder)
            {
                case "gia_tang":
                    query = query.OrderBy(x => x.gia);
                    break;
                case "gia_giam":
                    query = query.OrderByDescending(x => x.gia);
                    break;
                case "ten_az":
                    query = query.OrderBy(x => x.tenHangHoa);
                    break;
                case "ten_za":
                    query = query.OrderByDescending(x => x.tenHangHoa);
                    break;
                default:
                    query = query.OrderBy(x => x.maHangHoa); // Mặc định mới nhất
                    break;
            }

            // 6️ Phân trang (Làm SAU khi đã lọc và sắp xếp)
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Thực thi truy vấn lấy dữ liệu
            var result = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 7 ViewBag dữ liệu phụ
            ViewBag.Hangs = db.Hangs.ToList();
            ViewBag.DanhMucs = db.DanhMucs.ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // 🔥 GIỮ BỘ LỌC & TRẠNG THÁI SẮP XẾP
            ViewBag.maHang = maHang;
            ViewBag.maDanhMuc = maDanhMuc;
            ViewBag.giaMin = giaMin;
            ViewBag.giaMax = giaMax;
            ViewBag.keyword = keyword;
            ViewBag.SortOrder = sortOrder; // Quan trọng để Dropdown ở View biết đang chọn cái nào

            return View(result);
        }

        public ActionResult TrangGioiThieu()
        {
            return View();
        }

        public ActionResult TrangLienHe()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TrangLienHe(string hoTen, string sdt, string email, string tieuDe, string noiDung)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // --- 1. TẠO NỘI DUNG EMAIL (Object MailMessage) ---
                    MailMessage mail = new MailMessage();

                    mail.From = new MailAddress("hethong@phukiencaulong.vn", "Hệ thống Website");
                    mail.To.Add("huyhoangkaitokid40@gmail.com");

                    mail.Subject = $"[Liên hệ mới] {tieuDe}";
                    mail.IsBodyHtml = true;

                    // Nội dung HTML 
                    mail.Body = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd;'>
                <h2 style='color: #f26522;'>📩 Có liên hệ mới từ Website</h2>
                <p><strong>Họ tên khách:</strong> {hoTen}</p>
                <p><strong>Số điện thoại:</strong> {sdt}</p>
                <p><strong>Email khách:</strong> {email}</p>
                <hr />
                <p><strong>Tiêu đề:</strong> {tieuDe}</p>
                <p><strong>Nội dung:</strong></p>
                <div style='background: #f9f9f9; padding: 15px; border-radius: 5px;'>
                    {noiDung.Replace("\n", "<br>")}
                </div>
            </div>";

                    using (var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525))
                    {

                        client.Credentials = new NetworkCredential("b68b07c15c5950", "da22a3abb50edc");
                        client.EnableSsl = true;

                        client.Send(mail);
                    }

                    TempData["Success"] = "Gửi liên hệ thành công! Chúng tôi sẽ phản hồi sớm nhất.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi gửi mail: " + ex.Message;
                }
            }

            return RedirectToAction("TrangLienHe");
        }
        // GET: HangHoas_65131148/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HangHoa hangHoa = db.HangHoas.Find(id);
            if (hangHoa == null)
            {
                return HttpNotFound();
            }
            // 🔥 Lấy danh sách size (nếu có)
            var sizes = db.HangHoa_Sizes
                          .Where(x => x.maHangHoa == id)
                          .ToList();

            ViewBag.Sizes = sizes;
            // --- 🔥 PHẦN MỚI: TÍNH ĐIỂM TRUNG BÌNH ---
            var danhGias = db.DanhGiaSanPhams.Where(x => x.maHangHoa == id).ToList();

            double diemTB = 0;
            if (danhGias.Any()) // Nếu có đánh giá thì mới tính
            {
                // Tính trung bình cộng cột soSao
                diemTB = danhGias.Average(x => x.soSao ?? 0);
            }

            // Lưu vào ViewBag để mang sang View
            ViewBag.DiemTrungBinh = diemTB;
            ViewBag.SoLuotDanhGia = danhGias.Count;

            // --- LOGIC KIỂM TRA ĐÃ MUA HÀNG CHƯA ---
            bool daMua = false;
            if (Session["User"] != null)
            {
                var kh = (KhachHang)Session["User"];
                // Kiểm tra trong bảng ChiTietDonHang xem có sản phẩm này và đơn đã Hoàn thành chưa
                daMua = db.ChiTietDonHangs.Any(x => x.maHangHoa == id
                                              && x.DonHang.maKhachHang == kh.maKhachHang
                                              && x.DonHang.trangThai == "Hoàn thành");
            }
            ViewBag.DaMua = daMua; // Gửi kết quả sang View
                                   // ----------------------------------------

            return View(hangHoa);
        }

        // --- CÁC HÀM XỬ LÝ ĐÁNH GIÁ SẢN PHẨM ---

        // 1. Gửi đánh giá (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiDanhGia(int maHangHoa, int soSao, string NoiDung)
        {
            // Kiểm tra đăng nhập
            if (Session["User"] == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan_65131148");
            }

            var kh = (KhachHang)Session["User"];

            // 🔥 1. KIỂM TRA ĐIỀU KIỆN: Đã mua & Đơn hàng đã "Hoàn thành"
            bool daMua = db.ChiTietDonHangs.Any(x => x.maHangHoa == maHangHoa
                                                  && x.DonHang.maKhachHang == kh.maKhachHang
                                                  && x.DonHang.trangThai == "Hoàn thành");

            if (!daMua)
            {
                // Nếu chưa mua, đá về trang chi tiết và báo lỗi
                TempData["Error"] = "Bạn cần mua sản phẩm này thành công mới được viết đánh giá!";
                return RedirectToAction("Details", new { id = maHangHoa });
            }

            if (!string.IsNullOrEmpty(NoiDung))
            {
                var danhGia = new DanhGiaSanPham
                {
                    maHangHoa = maHangHoa,
                    maKhachHang = kh.maKhachHang,
                    soSao = soSao,
                    binhLuan = NoiDung,
                    ngayDanhGia = DateTime.Now
                };

                db.DanhGiaSanPhams.Add(danhGia);
                db.SaveChanges();
            }

            // Quay lại trang chi tiết sản phẩm
            return RedirectToAction("Details", new { id = maHangHoa });
        }

        // 2. Tải thêm đánh giá (Dùng cho nút "Xem thêm") - Trả về JSON
        public ActionResult TaiThemDanhGia(int maHangHoa, int page = 1)
        {
            int pageSize = 5; // Số lượng lấy mỗi lần
            var reviews = db.DanhGiaSanPhams.Where(x => x.maHangHoa == maHangHoa)
                                     .OrderByDescending(x => x.ngayDanhGia)
                                     .Skip((page - 1) * pageSize)
                                     .Take(pageSize)
                                     .Select(x => new
                                     {
                                         TenKH = x.KhachHang.hoTen,
                                         SoSao = x.soSao,
                                         NoiDung = x.binhLuan,
                                         Ngay = x.ngayDanhGia
                                     }).ToList();

            // Chuyển định dạng ngày tháng sang chuỗi để JSON hiểu
            var result = reviews.Select(x => new {
                x.TenKH,
                x.SoSao,
                x.NoiDung,
                Ngay = x.Ngay.Value.ToString("dd/MM/yyyy HH:mm")
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: HangHoas_65131148/Create
        public ActionResult Create()
        {
            ViewBag.maDanhMuc = new SelectList(db.DanhMucs, "maDanhMuc", "tenDanhMuc");
            ViewBag.maHang = new SelectList(db.Hangs, "maHang", "tenHang");
            return View();
        }

        // POST: HangHoas_65131148/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "maHangHoa,tenHangHoa,anh,moTa,gia,soLuongTon,maDanhMuc,maHang")] HangHoa hangHoa)
        {
            if (ModelState.IsValid)
            {
                db.HangHoas.Add(hangHoa);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.maDanhMuc = new SelectList(db.DanhMucs, "maDanhMuc", "tenDanhMuc", hangHoa.maDanhMuc);
            ViewBag.maHang = new SelectList(db.Hangs, "maHang", "tenHang", hangHoa.maHang);
            return View(hangHoa);
        }

        // GET: HangHoas_65131148/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HangHoa hangHoa = db.HangHoas.Find(id);
            if (hangHoa == null)
            {
                return HttpNotFound();
            }
            ViewBag.maDanhMuc = new SelectList(db.DanhMucs, "maDanhMuc", "tenDanhMuc", hangHoa.maDanhMuc);
            ViewBag.maHang = new SelectList(db.Hangs, "maHang", "tenHang", hangHoa.maHang);
            return View(hangHoa);
        }

        // POST: HangHoas_65131148/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "maHangHoa,tenHangHoa,anh,moTa,gia,soLuongTon,maDanhMuc,maHang")] HangHoa hangHoa)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hangHoa).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.maDanhMuc = new SelectList(db.DanhMucs, "maDanhMuc", "tenDanhMuc", hangHoa.maDanhMuc);
            ViewBag.maHang = new SelectList(db.Hangs, "maHang", "tenHang", hangHoa.maHang);
            return View(hangHoa);
        }

        // GET: HangHoas_65131148/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HangHoa hangHoa = db.HangHoas.Find(id);
            if (hangHoa == null)
            {
                return HttpNotFound();
            }
            return View(hangHoa);
        }

        // POST: HangHoas_65131148/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            HangHoa hangHoa = db.HangHoas.Find(id);
            db.HangHoas.Remove(hangHoa);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private List<CartItem_65131148> LayGioHang()
        {
            var gioHang = Session["GioHang"] as List<CartItem_65131148>;
            if (gioHang == null)
            {
                gioHang = new List<CartItem_65131148>();
                Session["GioHang"] = gioHang;
            }
            return gioHang;
        }

        public ActionResult ThemGioHang(int id, int? maSize)
        {
            var gioHang = LayGioHang();

            // Tìm xem sản phẩm + size này đã có trong giỏ chưa
            var item = gioHang.FirstOrDefault(x => x.MaHangHoa == id && x.MaSize == maSize);

            if (item != null)
            {
                item.SoLuong++; // Đã có thì tăng số lượng
            }
            else
            {
                // Chưa có thì truy vấn DB để lấy thông tin
                var sp = db.HangHoas.Find(id);

                string tenSize = null;
                if (maSize.HasValue)
                {
                    var sizeObj = db.Sizes.Find(maSize.Value);
                    tenSize = sizeObj?.tenSize;
                }

                gioHang.Add(new CartItem_65131148
                {
                    MaHangHoa = id,
                    TenHangHoa = sp.tenHangHoa,
                    Anh = sp.anh,
                    DonGia = sp.gia ?? 0,
                    SoLuong = 1,
                    MaSize = maSize,
                    TenSize = tenSize
                });
            }

            TempData["Message"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("TrangSanPham"); // Hoặc trả về trang hiện tại
        }

        // Thêm hàm phụ để tính phí ship (tránh viết lại code nhiều lần)
        private decimal TinhPhiShip(decimal tongTienHang)
        {
            // Logic: Trên 2 triệu thì 0đ, ngược lại là 30.000đ
            return (tongTienHang >= 2000000) ? 0 : 30000;
        }
        // 1. Cập nhật Action GioHang
        public ActionResult GioHang()
        {
            var gioHang = LayGioHang();

            // 🔥 Logic Mới: Điền danh sách size khả dụng cho từng item trong giỏ
            foreach (var item in gioHang)
            {
                // Nếu sản phẩm có size (MaSize != null), thì lấy list size từ DB
                if (item.MaSize != null)
                {
                    item.DanhSachSize = db.HangHoa_Sizes
                                          .Where(s => s.maHangHoa == item.MaHangHoa && s.soLuongTon > 0)
                                          .ToList();
                }
            }

            decimal tongTienHang = gioHang.Sum(x => x.ThanhTien);
            decimal phiShip = TinhPhiShip(tongTienHang);

            ViewBag.TongTienHang = tongTienHang; // Tiền hàng chưa ship
            ViewBag.PhiShip = phiShip;           // Phí ship
            ViewBag.TongThanhToan = tongTienHang + phiShip; // Tổng phải trả

            return View(gioHang);
        }
        // 2. THÊM MỚI Action: Đổi Size trong giỏ hàng
        [HttpPost]
        public ActionResult DoiSizeGioHang(int id, int sizeCu, int sizeMoi)
        {
            var gioHang = LayGioHang();

            // Tìm sản phẩm đang muốn đổi
            var itemCanDoi = gioHang.FirstOrDefault(x => x.MaHangHoa == id && x.MaSize == sizeCu);

            if (itemCanDoi != null)
            {
                // Kiểm tra xem trong giỏ đã có sản phẩm đó với "Size Mới" chưa?
                var itemDaCo = gioHang.FirstOrDefault(x => x.MaHangHoa == id && x.MaSize == sizeMoi);

                if (itemDaCo != null)
                {
                    // Trường hợp 1: Đã có -> Gộp số lượng vào cái đã có và xóa cái cũ
                    itemDaCo.SoLuong += itemCanDoi.SoLuong;
                    gioHang.Remove(itemCanDoi);
                }
                else
                {
                    // Trường hợp 2: Chưa có -> Cập nhật Size mới và Tên Size mới cho item hiện tại
                    var sizeInfo = db.Sizes.Find(sizeMoi); // Lấy tên size từ DB
                    if (sizeInfo != null)
                    {
                        itemCanDoi.MaSize = sizeMoi;
                        itemCanDoi.TenSize = sizeInfo.tenSize;
                    }
                }

                TempData["Message"] = "Cập nhật size thành công!";
            }

            return RedirectToAction("GioHang");
        }

        public ActionResult XoaGioHang(int id, int? maSize)
        {
            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(x => x.MaHangHoa == id && x.MaSize == maSize);
            if (item != null)
            {
                gioHang.Remove(item);
            }
            return RedirectToAction("GioHang");
        }


        [HttpPost]
        public ActionResult CapNhatGioHang(int id, int? maSize, int soLuong)
        {
            var gioHang = LayGioHang();
            var item = gioHang.FirstOrDefault(x => x.MaHangHoa == id && x.MaSize == maSize);
            if (item != null && soLuong > 0)
            {
                item.SoLuong = soLuong;
            }
            return RedirectToAction("GioHang");
        }

        // 2. Cập nhật Action DatHang (Trang thanh toán)
        public ActionResult DatHang()
        {
            if (Session["User"] == null) return RedirectToAction("DangNhap", "TaiKhoan_65131148");
            var gioHang = LayGioHang();
            if (gioHang == null || gioHang.Count == 0) return RedirectToAction("TrangSanPham", "HangHoas_65131148");

            decimal tongTienHang = gioHang.Sum(x => x.ThanhTien);
            decimal phiShip = TinhPhiShip(tongTienHang);

            ViewBag.TongTienHang = tongTienHang;
            ViewBag.PhiShip = phiShip;
            ViewBag.TongThanhToan = tongTienHang + phiShip;

            return View(gioHang);
        }

        [HttpPost]
        public ActionResult XacNhanDatHang(string diaChiGiao, string soDienThoai, string tenNguoiNhan)
        {
            var gioHang = LayGioHang();
            if (gioHang == null || gioHang.Count == 0) return RedirectToAction("TrangSanPham");

            int maKH = int.Parse(Session["MaID"].ToString());

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    decimal tongTienHang = gioHang.Sum(x => x.ThanhTien);
                    decimal phiShip = TinhPhiShip(tongTienHang); // Tính lại phí ship tại server
                    // 1. Tạo Đơn Hàng
                    DonHang dh = new DonHang();
                    dh.maKhachHang = maKH;
                    dh.ngayDat = DateTime.Now;
                    dh.trangThai = "Chờ xử lý";
                    dh.trangThaiGiaoHang = "Chưa giao";
                    dh.tenNguoiNhan = tenNguoiNhan;
                    dh.diaChiGiao = diaChiGiao;
                    dh.soDienThoai = soDienThoai;
                    dh.tongTien = gioHang.Sum(x => x.ThanhTien);
                    dh.phiShip = phiShip; // 🔥 LƯU PHÍ SHIP VÀO DB
                    dh.tongTien = tongTienHang + phiShip; // 🔥 TỔNG TIỀN ĐƠN HÀNG = HÀNG + SHIP

                    db.DonHangs.Add(dh);
                    db.SaveChanges(); // Lưu để lấy ID đơn hàng

                    // 2. Lưu Chi Tiết & Trừ Kho
                    foreach (var item in gioHang)
                    {
                        // Tạo chi tiết đơn hàng
                        ChiTietDonHang ct = new ChiTietDonHang();
                        ct.maDonHang = dh.maDonHang;
                        ct.maHangHoa = item.MaHangHoa;
                        ct.soLuong = item.SoLuong;
                        ct.donGia = item.DonGia;
                        ct.maSize = item.MaSize; // Lưu size vào chi tiết

                        db.ChiTietDonHangs.Add(ct);

                        // --- XỬ LÝ TRỪ TỒN KHO ---
                        if (item.MaSize.HasValue)
                        {
                            // Case 1: Có Size -> Trừ ở bảng HangHoa_Size
                            var khoSize = db.HangHoa_Sizes.FirstOrDefault(x => x.maHangHoa == item.MaHangHoa && x.maSize == item.MaSize);
                            if (khoSize != null)
                            {
                                if (khoSize.soLuongTon < item.SoLuong) throw new Exception($"Sản phẩm {item.TenHangHoa} size {item.TenSize} không đủ hàng!");
                                khoSize.soLuongTon -= item.SoLuong;
                            }
                        }
                        else
                        {
                            // Case 2: Không Size -> Trừ ở bảng HangHoa
                            var sp = db.HangHoas.Find(item.MaHangHoa);
                            if (sp != null)
                            {
                                if (sp.soLuongTon < item.SoLuong) throw new Exception($"Sản phẩm {item.TenHangHoa} không đủ hàng!");
                                sp.soLuongTon -= item.SoLuong;
                            }
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    // Xóa giỏ hàng
                    Session["GioHang"] = null;
                    return RedirectToAction("DatHangThanhCong");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Message"] = "Lỗi đặt hàng: " + ex.Message;
                    return RedirectToAction("GioHang");
                }
            }
        }

        public ActionResult DatHangThanhCong() => View();

        // 1. Trang danh sách đơn hàng của khách hàng
        // GET: Xem danh sách đơn hàng của khách
        public ActionResult DonHang(int page = 1)
        {
            // 1. Kiểm tra đăng nhập
            if (Session["User"] == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan_65131148");
            }

            var kh = (KhachHang)Session["User"];
            int pageSize = 6; 

            // 2. Truy vấn dữ liệu (Chưa lấy về ngay)
            var query = db.DonHangs
                          .Where(x => x.maKhachHang == kh.maKhachHang)
                          .OrderByDescending(x => x.ngayDat); // Đơn mới nhất lên đầu

            // 3. Tính toán phân trang
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // 4. Cắt dữ liệu (Skip & Take)
            var listDonHang = query
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToList();

            // 5. Gửi dữ liệu phân trang sang View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(listDonHang);
        }

        // 2. Trang chi tiết của một đơn hàng cụ thể
        public ActionResult ChiTietDonHang(int id)
        {
            if (Session["User"] == null) return RedirectToAction("DangNhap", "TaiKhoan_65131148");

            // Lấy thông tin đơn hàng
            var donHang = db.DonHangs.Find(id);
            if (donHang == null) return HttpNotFound();

            // Lấy danh sách sản phẩm trong đơn hàng đó
            var chiTiet = db.ChiTietDonHangs.Include("Size")
                            .Where(ct => ct.maDonHang == id)
                            .ToList();

            ViewBag.DonHang = donHang;
            return View(chiTiet);
        }

        

    }
}
