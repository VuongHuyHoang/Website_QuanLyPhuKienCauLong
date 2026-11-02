using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using WebsiteBanPhuKienCauLong_65131148.Models;

namespace WebsiteBanPhuKienCauLong_65131148.Controllers
{
    public class Admin_65131148Controller : Controller
    {
        private QuanLyBanPhuKienCauLong_65131148Entities db = new QuanLyBanPhuKienCauLong_65131148Entities();
        // GET: Admin_65131148
        public ActionResult TrangChu()
        {
            // Thống kê đơn giản
            ViewBag.SoLuongSanPham = db.HangHoas.Count();
            ViewBag.SoLuongDonHang = db.DonHangs.Count();
            ViewBag.DoanhThu = db.DonHangs.Where(x=>x.trangThai =="Hoàn thành")
                                    .Sum(x => x.tongTien) ?? 0;
            ViewBag.KhachHang = db.KhachHangs.Count();

            // Lấy 5 đơn hàng mới nhất
            var donHangMoi = db.DonHangs.OrderByDescending(x => x.ngayDat).Take(5).ToList();
            return View(donHangMoi);
        }
        // ==========================================
        // 2. PHẦN QUẢN LÝ SẢN PHẨM
        // ==========================================
        #region QuanLySanPham
        // Hàm chuyển đổi Tiếng Việt có dấu thành không dấu (Ví dụ: "Vợt Cầu Lông" -> "vot-cau-long")
        public string ToUrlSlug(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return "other";

            string[] vietnameseSigns = new string[]
            {
        "aAeEoOuUiIdDyY",
        "áàạảãâấầậẩẫăắằặẳẵ",
        "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
        "éèẹẻẽêếềệểễ",
        "ÉÈẸẺẼÊẾỀỆỂỄ",
        "óòọỏõôốồộổỗơớờợởỡ",
        "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
        "úùụủũưứừựửữ",
        "ÚÙỤỦŨƯỨỪỰỬỮ",
        "íìịỉĩ",
        "ÍÌỊỈĨ",
        "đ",
        "Đ",
        "ýỳỵỷỹ",
        "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                    str = str.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
            }

            // Loại bỏ ký tự đặc biệt và khoảng trắng thừa
            str = str.ToLower();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", ""); // Xóa khoảng trắng để giống tên folder trong hình (ao, balo...)

            return str;
        }

        // --- 1. GET: Hiển thị form sửa ---
        public ActionResult Edit(int id)
        {
            // Tìm sản phẩm theo ID
            var hangHoa = db.HangHoas.Find(id);
            if (hangHoa == null)
            {
                return HttpNotFound();
            }

            // Load danh sách Danh mục và Hãng để đổ vào Dropdown
            // Chọn sẵn giá trị hiện tại của sản phẩm (hangHoa.maDanhMuc, hangHoa.maHang)
            ViewBag.maDanhMuc = new SelectList(db.DanhMucs, "maDanhMuc", "tenDanhMuc", hangHoa.maDanhMuc);
            ViewBag.maHang = new SelectList(db.Hangs, "maHang", "tenHang", hangHoa.maHang);

            return View(hangHoa);
        }

        // --- 2. POST: Xử lý cập nhật ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HangHoa hangHoa, HttpPostedFileBase anhFile)
        {
            if (ModelState.IsValid)
            {
                // Lấy sản phẩm cũ từ DB để biết ảnh cũ là gì
                var sanPhamCu = db.HangHoas.AsNoTracking().FirstOrDefault(x => x.maHangHoa == hangHoa.maHangHoa);

                // Cần gán lại trạng thái Modified thủ công vì ta đã dùng AsNoTracking ở trên
                db.Entry(hangHoa).State = EntityState.Modified;

                // --- XỬ LÝ ẢNH ---
                if (anhFile != null && anhFile.ContentLength > 0)
                {
                    // A. XÓA ẢNH CŨ (Nếu có)
                    if (sanPhamCu != null && !string.IsNullOrEmpty(sanPhamCu.anh))
                    {
                        string oldFilePath = Server.MapPath("~/images/" + sanPhamCu.anh);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath); // Xóa file cũ đi
                        }
                    }

                    // B. LƯU ẢNH MỚI
                    // 1. Xác định folder dựa trên Danh mục hiện tại
                    var danhMuc = db.DanhMucs.Find(hangHoa.maDanhMuc);
                    string folderName = (danhMuc != null) ? ToUrlSlug(danhMuc.tenDanhMuc) : "other";

                    string folderPath = Server.MapPath("~/images/" + folderName);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string fileName = Path.GetFileName(anhFile.FileName);
                    string path = Path.Combine(folderPath, fileName);
                    anhFile.SaveAs(path);

                    // Cập nhật đường dẫn mới vào DB
                    hangHoa.anh = folderName + "/" + fileName;
                }
                else
                {
                    // Nếu không chọn ảnh mới -> Giữ nguyên ảnh cũ
                    hangHoa.anh = sanPhamCu.anh;
                }

                db.SaveChanges();
                return RedirectToAction("DanhSachSanPham");
            }

            // Load lại dropdown
            ViewBag.maDanhMuc = new SelectList(db.DanhMucs, "maDanhMuc", "tenDanhMuc", hangHoa.maDanhMuc);
            ViewBag.maHang = new SelectList(db.Hangs, "maHang", "tenHang", hangHoa.maHang);
            return View(hangHoa);
        }

        // ==========================================
        // CHỨC NĂNG XÓA SẢN PHẨM
        // ==========================================

        // 1. GET: Hiển thị trang xác nhận xóa
        public ActionResult Delete(int id)
        {
            var hangHoa = db.HangHoas.Find(id);
            if (hangHoa == null)
            {
                return HttpNotFound();
            }
            return View(hangHoa);
        }

        // 2. POST: Thực hiện xóa
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var hangHoa = db.HangHoas.Find(id);
            if (hangHoa == null)
            {
                return HttpNotFound();
            }

            // --- KIỂM TRA RÀNG BUỘC ---
            // Nếu sản phẩm đã có trong đơn hàng (đã từng bán), không xóa!!!
            bool daCoDonHang = db.ChiTietDonHangs.Any(x => x.maHangHoa == id);
            if (daCoDonHang)
            {
                // Thêm lỗi vào ModelState để hiển thị bên View
                ModelState.AddModelError("", "Sản phẩm này đã có người mua, không thể xóa! Hãy chuyển trạng thái sang 'Ngừng kinh doanh' hoặc xóa số lượng tồn kho.");
                return View(hangHoa);
            }

            // --- XỬ LÝ XÓA ---
            try
            {
                // 1. Xóa các Size liên quan trong bảng HangHoa_Size
                var sizes = db.HangHoa_Sizes.Where(x => x.maHangHoa == id).ToList();
                db.HangHoa_Sizes.RemoveRange(sizes);

                // 2. Xóa ảnh vật lý trên Server
                if (!string.IsNullOrEmpty(hangHoa.anh))
                {
                    string filePath = Server.MapPath("~/images/" + hangHoa.anh);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // 3. Xóa sản phẩm khỏi Database
                db.HangHoas.Remove(hangHoa);
                db.SaveChanges();

                return RedirectToAction("DanhSachSanPham");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi xóa: " + ex.Message);
                return View(hangHoa);
            }
        }

        // --- QUẢN LÝ SIZE SẢN PHẨM ---

        // 1. GET: Hiển thị giao diện nhập size
        public ActionResult QuanLySize(int id)
        {
            var hangHoa = db.HangHoas.Find(id);
            if (hangHoa == null) return HttpNotFound();

            // 1. Lấy tất cả size về bộ nhớ trước 
            var allSizes = db.Sizes.ToList();

            // Tạo list chứa kết quả lọc
            List<Size> filteredSizes = new List<Size>();

            // 2. Logic lọc dựa trên Mã Danh Mục 
            if (hangHoa.maDanhMuc == 2) // Danh mục 2 là GIÀY
            {
                // Chỉ lấy các size là SỐ (36, 37, 38...)
                filteredSizes = allSizes.Where(s => int.TryParse(s.tenSize, out _)).ToList();
            }
            else if (hangHoa.maDanhMuc == 3 || hangHoa.maDanhMuc == 4) // Danh mục 3, 4 là ÁO, QUẦN
            {
                // Chỉ lấy các size là CHỮ (S, M, L...) 
                filteredSizes = allSizes.Where(s => !int.TryParse(s.tenSize, out _)).ToList();
            }
            else
            {
                // Các danh mục khác (Vợt, Balo, Phụ kiện...) -> Không có size
            }

            // 3. Lấy dữ liệu tồn kho cũ của sản phẩm này
            var existingSizes = db.HangHoa_Sizes.Where(x => x.maHangHoa == id).ToList();

            // Truyền dữ liệu sang View
            ViewBag.HangHoa = hangHoa;
            ViewBag.ExistingSizes = existingSizes;

            // 🔥 Gửi danh sách ĐÃ LỌC sang View
            return View(filteredSizes);
        }

        // 2. POST: Lưu cấu hình size
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuanLySize(int id, bool coSize, int? soLuongChung, List<int> maSize, List<int> soLuongRieng)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var hangHoa = db.HangHoas.Find(id);
                    if (hangHoa == null) return HttpNotFound();

                    // BƯỚC 1: Luôn luôn xóa cấu hình size cũ để tránh dư thừa dữ liệu
                    // (Ví dụ: Đang từ có size chuyển sang không size, phải xóa hết data trong bảng HangHoa_Size)
                    var oldSizes = db.HangHoa_Sizes.Where(x => x.maHangHoa == id);
                    db.HangHoa_Sizes.RemoveRange(oldSizes);
                    db.SaveChanges(); // Lưu xóa trước

                    // BƯỚC 2: Xử lý theo chế độ user chọn
                    int tongTonKho = 0;

                    if (coSize == false)
                    {
                        // === CASE A: SẢN PHẨM KHÔNG CÓ SIZE ===
                        // Chỉ lấy số lượng từ ô nhập chung
                        tongTonKho = soLuongChung ?? 0;
                    }
                    else
                    {
                        // === CASE B: SẢN PHẨM CÓ SIZE ===
                        // Duyệt danh sách size và lưu vào bảng HangHoa_Size
                        if (maSize != null && soLuongRieng != null)
                        {
                            for (int i = 0; i < maSize.Count; i++)
                            {
                                int sl = soLuongRieng[i];
                                if (sl > 0)
                                {
                                    HangHoa_Size newItem = new HangHoa_Size()
                                    {
                                        maHangHoa = id,
                                        maSize = maSize[i],
                                        soLuongTon = sl
                                    };
                                    db.HangHoa_Sizes.Add(newItem);
                                    tongTonKho += sl; // Cộng dồn tổng
                                }
                            }
                        }
                    }

                    // BƯỚC 3: Cập nhật tổng tồn kho vào bảng chính HangHoa
                    hangHoa.soLuongTon = tongTonKho;

                    db.SaveChanges();
                    transaction.Commit();

                    return RedirectToAction("DanhSachSanPham");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    // Load lại view nếu lỗi (cần lấy lại data như hàm GET)
                    return RedirectToAction("QuanLySize", new { id = id });
                }
            }
        }
        // Danh sách sản phẩm
        // GET: Admin/DanhSachSanPham
        public ActionResult DanhSachSanPham(int? maDanhMuc, int? maHang, string sortOrder, int page = 1)
        {
            int pageSize = 8; // Số sản phẩm trên 1 trang 

            // 1. Khởi tạo Query (chưa thực thi)
            var products = db.HangHoas.Include(h => h.DanhMuc).Include(h => h.Hang).AsQueryable();

            // 2. Xử lý Lọc (Filter)
            if (maDanhMuc.HasValue)
            {
                products = products.Where(x => x.maDanhMuc == maDanhMuc.Value);
            }
            if (maHang.HasValue)
            {
                products = products.Where(x => x.maHang == maHang.Value);
            }

            // 3. Xử lý Sắp xếp (Sorting)
            // Lưu trạng thái sắp xếp vào ViewBag để giữ giá trị trên View
            ViewBag.SortOrder = sortOrder;

            switch (sortOrder)
            {
                case "gia_tang":
                    products = products.OrderBy(p => p.gia);
                    break;
                case "gia_giam":
                    products = products.OrderByDescending(p => p.gia);
                    break;
                case "ten_az":
                    products = products.OrderBy(p => p.tenHangHoa);
                    break;
                default:
                    // Mặc định sắp xếp theo mới nhất (Mã hàng hóa giảm dần)
                    products = products.OrderByDescending(p => p.maHangHoa);
                    break;
            }

            // 4. Xử lý Phân trang (Pagination)
            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Lưu các biến phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // Lưu các biến lọc vào ViewBag để giữ trạng thái Dropdown
            ViewBag.CurrentDanhMuc = maDanhMuc;
            ViewBag.CurrentHang = maHang;

            // Lấy dữ liệu cho Dropdown
            ViewBag.maDanhMuc = new SelectList(db.DanhMucs, "maDanhMuc", "tenDanhMuc");
            ViewBag.maHang = new SelectList(db.Hangs, "maHang", "tenHang");

            // Thực thi truy vấn lấy dữ liệu trang hiện tại
            var pagedData = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return View(pagedData);
        }

        // Tạo mới (GET)
        public ActionResult TaoSanPham()
        {
            ViewBag.maDanhMuc = new SelectList(db.DanhMucs, "maDanhMuc", "tenDanhMuc");
            ViewBag.maHang = new SelectList(db.Hangs, "maHang", "tenHang");
            return View();
        }

        // Tạo mới (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaoSanPham(HangHoa hangHoa, HttpPostedFileBase anhFile)
        {
            if (ModelState.IsValid)
            {
                // 1. Lấy thông tin danh mục để biết lưu vào folder nào
                var danhMuc = db.DanhMucs.Find(hangHoa.maDanhMuc);
                string folderName = (danhMuc != null) ? ToUrlSlug(danhMuc.tenDanhMuc) : "other";

                // 2. Xử lý lưu ảnh
                if (anhFile != null && anhFile.ContentLength > 0)
                {
                    // Tạo đường dẫn thư mục: ~/images/ten-danh-muc/
                    string folderPath = Server.MapPath("~/images/" + folderName);

                    // Kiểm tra nếu thư mục chưa tồn tại thì tạo mới
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // Tạo tên file an toàn (thêm timestamp để tránh trùng tên)
                    string fileName = Path.GetFileName(anhFile.FileName);

                    // Lưu file vào thư mục con
                    string path = Path.Combine(folderPath, fileName);
                    anhFile.SaveAs(path);

                    // 🔥 QUAN TRỌNG: Lưu đường dẫn vào DB bao gồm cả tên thư mục
                    // Ví dụ: "giay/giay-yonex.jpg" thay vì chỉ "giay-yonex.jpg"
                    hangHoa.anh = folderName + "/" + fileName;
                }

                db.HangHoas.Add(hangHoa);
                db.SaveChanges();
                return RedirectToAction("DanhSachSanPham");
            }

            // Load lại dropdown nếu lỗi
            ViewBag.maDanhMuc = new SelectList(db.DanhMucs, "maDanhMuc", "tenDanhMuc", hangHoa.maDanhMuc);
            ViewBag.maHang = new SelectList(db.Hangs, "maHang", "tenHang", hangHoa.maHang);
            return View(hangHoa);
        }

        #endregion

        // ==========================================
        // 3. PHẦN QUẢN LÝ ĐƠN HÀNG
        // ==========================================
        #region QuanLyDonHang

        // GET: Admin/DanhSachDonHang
        public ActionResult DanhSachDonHang(string search, string trangThai, string sortOrder, DateTime? tuNgay, DateTime? denNgay, int page = 1)
        {
            int pageSize = 8; // Số đơn hàng trên 1 trang

            // 1. Query gốc
            var query = db.DonHangs.AsQueryable();

            // 2. TÌM KIẾM ĐA NĂNG (Mã ĐH, Tên KH, SĐT)
            if (!string.IsNullOrEmpty(search))
            {
                // Lưu ý: maDonHang là số nên cần ToString() để tìm kiếm dạng chuỗi
                query = query.Where(x => x.maDonHang.ToString().Contains(search) ||
                                         x.tenNguoiNhan.Contains(search) ||
                                         x.soDienThoai.Contains(search));
            }

            // 3. LỌC THEO TRẠNG THÁI
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(x => x.trangThai == trangThai);
            }

            // 4. LỌC THEO NGÀY (Từ ngày - Đến ngày)
            if (tuNgay.HasValue)
            {
                query = query.Where(x => x.ngayDat >= tuNgay.Value);
            }
            if (denNgay.HasValue)
            {
                // Cộng thêm 1 ngày rồi trừ 1 giây để lấy hết cuối ngày hôm đó (23:59:59)
                var denNgayCuoi = denNgay.Value.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.ngayDat <= denNgayCuoi);
            }

            // 5. SẮP XẾP
            ViewBag.SortOrder = sortOrder;
            switch (sortOrder)
            {
                case "gia_tang":
                    query = query.OrderBy(x => x.tongTien);
                    break;
                case "gia_giam":
                    query = query.OrderByDescending(x => x.tongTien);
                    break;
                case "ngay_tang":
                    query = query.OrderBy(x => x.ngayDat);
                    break;
                default:
                    // Mặc định: Đơn mới nhất lên đầu
                    query = query.OrderByDescending(x => x.ngayDat);
                    break;
            }

            // 6. PHÂN TRANG
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Lưu dữ liệu vào ViewBag để giữ trạng thái trên View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;
            ViewBag.TrangThai = trangThai;
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");

            // Lấy dữ liệu
            var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return View(model);
        }

        public ActionResult ChiTietDonHang(int id)
        {
            var donHang = db.DonHangs.Find(id);
            if (donHang == null) return HttpNotFound();
            ViewBag.ChiTiet = db.ChiTietDonHangs.Where(x => x.maDonHang == id).ToList();
            return View(donHang);
        }

        [HttpPost]
        public ActionResult CapNhatTrangThai(int id, string trangThai)
        {
            var donHang = db.DonHangs.Find(id);
            if (donHang != null)
            {
                donHang.trangThai = trangThai;
                if (trangThai == "Hoàn thành")
                {
                    donHang.ngayThanhToan = DateTime.Now;
                    donHang.ngayNhan = DateTime.Now;
                    donHang.trangThaiGiaoHang = "Đã giao";
                }
                else if (trangThai == "Đang giao")
                {
                    donHang.ngayGiao = DateTime.Now;
                    donHang.trangThaiGiaoHang = "Đang giao";
                }
                db.SaveChanges();
            }
            return RedirectToAction("ChiTietDonHang", new { id = id });
        }
        #endregion

        // =============================================
        // 4. QUẢN LÝ THƯƠNG HIỆU (HÃNG)
        // =============================================
        // --- QUẢN LÝ HÃNG (Thương hiệu) ---

        // 1. Hiển thị danh sách + Form thêm nhanh
        public ActionResult DanhSachHang(int page = 1)
        {
            int pageSize = 20;
            var query = db.Hangs.AsQueryable();

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            var model = query.OrderBy(x => x.maHang)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();

            return View(model);
        }

        // 2. Xử lý thêm mới (POST từ Form bên phải)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaoHangNhanh(string tenHang)
        {
            if (!string.IsNullOrWhiteSpace(tenHang))
            {
                try
                {
                    var hang = new Hang { tenHang = tenHang };
                    db.Hangs.Add(hang);
                    db.SaveChanges();
                    TempData["Success"] = "Thêm hãng thành công!";
                }
                catch (Exception)
                {
                    TempData["Error"] = "Lỗi! Có thể tên hãng bị trùng hoặc lỗi hệ thống.";
                }
            }
            else
            {
                TempData["Error"] = "Vui lòng nhập tên hãng!";
            }
            return RedirectToAction("DanhSachHang");
        }

        // 3. Xóa hãng
        public ActionResult XoaHang(int id) 
        {
            var hang = db.Hangs.Find(id); 
            if (hang != null)
            {
                // Kiểm tra ràng buộc
                if (db.HangHoas.Any(x => x.maHang == id)) 
                {
                    TempData["Error"] = "Không thể xóa! Hãng này đang có sản phẩm.";
                }
                else
                {
                    db.Hangs.Remove(hang);
                    db.SaveChanges();
                    TempData["Success"] = "Xóa hãng thành công!";
                }
            }
            return RedirectToAction("DanhSachHang");
        }

        // =============================================
        // 5. QUẢN LÝ SIZE (KÍCH CỠ)
        // =============================================
        #region QuanLySize

        public ActionResult DanhSachSize(int page = 1)
        {
            int pageSize = 20;
            var query = db.Sizes.AsQueryable();

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            var model = query.OrderBy(x => x.maSize)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();
            return View(model);
        }

        public ActionResult TaoSize() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaoSize(Size size)
        {
            if (ModelState.IsValid)
            {
                db.Sizes.Add(size);
                db.SaveChanges();
                return RedirectToAction("DanhSachSize");
            }
            return View(size);
        }

        public ActionResult XoaSize(int id)
        {
            var size = db.Sizes.Find(id);
            if (size != null)
            {
                // Kiểm tra xem size này đã dùng cho sản phẩm nào chưa (Bảng HangHoa_Size)
                if (db.HangHoa_Sizes.Any(x => x.maSize == id))
                {
                    TempData["Error"] = "Size này đang được sử dụng, không thể xóa!";
                }
                else
                {
                    db.Sizes.Remove(size);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("DanhSachSize");
        }
        #endregion

        // =============================================
        // 6. QUẢN LÝ TÀI KHOẢN (KHÁCH HÀNG)
        // =============================================
        #region QuanLyTaiKhoan

        public ActionResult DanhSachKhachHang(string search, int page = 1)
        {
            int pageSize = 20;
            var query = db.KhachHangs.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.hoTen.Contains(search) || x.email.Contains(search) || x.sdt.Contains(search));
            }

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            var model = query.OrderByDescending(x => x.maKhachHang)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();

            return View(model);
        }

        public ActionResult ChiTietKhachHang(int id)
        {
            var kh = db.KhachHangs.Find(id);
            if (kh == null) return HttpNotFound();

            // Lấy lịch sử mua hàng
            ViewBag.LichSuDonHang = db.DonHangs.Where(x => x.maKhachHang == id).OrderByDescending(x => x.ngayDat).ToList();

            return View(kh);
        }
        #endregion

        //////////BÁO CÁO////////////////////////////////////////////////////////
        // GET: Admin/BaoCao
        public ActionResult BaoCao(DateTime? fromDate, DateTime? toDate)
        {
            // 1. Thiết lập ngày mặc định (Tháng hiện tại) nếu chưa chọn
            var tuNgay = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var denNgay = toDate ?? DateTime.Now.Date; // Lấy hết ngày hôm nay

            // Mở rộng denNgay đến cuối ngày (23:59:59) để lấy đủ đơn trong ngày
            var denNgayCuoi = denNgay.AddDays(1).AddTicks(-1);

            // 2. Lấy danh sách đơn hàng trong khoảng thời gian (Chỉ lấy đơn Hoàn thành để tính tiền)
            var donHangs = db.DonHangs
                             .Where(x => x.ngayDat >= tuNgay && x.ngayDat <= denNgayCuoi)
                             .ToList();

            // 3. Tính toán số liệu tổng quan
            var model = new BaoCao_65131148
            {
                TuNgay = tuNgay,
                DenNgay = denNgay,
                TongDoanhThu = donHangs.Where(x => x.trangThai == "Hoàn thành").Sum(x => x.tongTien) ?? 0,
                TongDonHang = donHangs.Count,
                DonHangHuy = donHangs.Count(x => x.trangThai == "Đã hủy")
            };

            // Tính tổng sản phẩm bán ra (phải join sang bảng chi tiết)
            // Lấy ID các đơn hàng hoàn thành
            var maDonHangHoanThanh = donHangs.Where(x => x.trangThai == "Hoàn thành").Select(x => x.maDonHang).ToList();

            if (maDonHangHoanThanh.Any())
            {
                model.SanPhamDaBan = db.ChiTietDonHangs
                                       .Where(x => maDonHangHoanThanh.Contains(x.maDonHang))
                                       .Sum(x =>(int?) x.soLuong) ?? 0;
            }

            // 4. Xử lý dữ liệu Biểu đồ Doanh thu (Line Chart - Gom nhóm theo ngày)
            var doanhThuTheoNgay = donHangs
                .Where(x => x.trangThai == "Hoàn thành")
                .GroupBy(x => x.ngayDat.Value.Date)
                .Select(g => new { Ngay = g.Key, DoanhThu = g.Sum(x => x.tongTien) })
                .OrderBy(x => x.Ngay)
                .ToList();

            model.LabelsNgay = doanhThuTheoNgay.Select(x => x.Ngay.ToString("dd/MM")).ToList();
            model.DataDoanhThu = doanhThuTheoNgay.Select(x => x.DoanhThu ?? 0).ToList();

            // 5. Xử lý dữ liệu Top 5 Sản phẩm bán chạy (Bar Chart)
            // Logic: Lấy chi tiết đơn của các đơn hoàn thành trong khoảng thời gian
            var topSanPhams = db.ChiTietDonHangs
                                .Where(x => x.DonHang.ngayDat >= tuNgay && x.DonHang.ngayDat <= denNgayCuoi && x.DonHang.trangThai == "Hoàn thành")
                                .GroupBy(x => x.HangHoa.tenHangHoa)
                                .Select(g => new { TenSP = g.Key, SoLuong = g.Sum(x => x.soLuong) })
                                .OrderByDescending(x => x.SoLuong)
                                .Take(5)
                                .ToList();

            model.LabelsSanPham = topSanPhams.Select(x => x.TenSP).ToList();
            model.DataSoLuongBan = topSanPhams.Select(x => (int?)x.SoLuong ?? 0).ToList();

            return View(model);
        }
        //Trang quản lsi đánh giá
        public ActionResult DanhSachDanhGia(string search, int? filterStar, int page = 1)
        {
            int pageSize = 10;
            var query = db.DanhGiaSanPhams.AsQueryable();

            // 1. TÌM KIẾM (Theo tên khách hoặc tên sản phẩm)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.KhachHang.hoTen.Contains(search) ||
                                         x.HangHoa.tenHangHoa.Contains(search));
            }

            // 2. LỌC THEO SỐ SAO (Để tìm đánh giá xấu/tốt)
            if (filterStar.HasValue)
            {
                query = query.Where(x => x.soSao == filterStar.Value);
            }

            // Sắp xếp: Mới nhất lên đầu
            query = query.OrderByDescending(x => x.ngayDanhGia);

            // --- THỐNG KÊ NHANH ---
            ViewBag.TongDanhGia = db.DanhGiaSanPhams.Count();
            ViewBag.DiemTrungBinh = db.DanhGiaSanPhams.Any()
                                    ? Math.Round(db.DanhGiaSanPhams.Average(x => x.soSao ?? 0), 1)
                                    : 0;
            ViewBag.DanhGia5Sao = db.DanhGiaSanPhams.Count(x => x.soSao == 5);

            // Giữ lại giá trị filter trên View
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStar = filterStar;

            // Phân trang thủ công đơn giản (hoặc dùng PagedList)
            var model = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.TotalPages = (int)Math.Ceiling((double)query.Count() / pageSize);
            ViewBag.CurrentPage = page;

            return View(model);
        }

        // XÓA ĐÁNH GIÁ 
        [HttpPost]
        public ActionResult XoaDanhGia(int id)
        {
            var dg = db.DanhGiaSanPhams.Find(id);
            if (dg != null)
            {
                db.DanhGiaSanPhams.Remove(dg);
                db.SaveChanges();
                TempData["Success"] = "Đã xóa đánh giá thành công!";
            }
            return RedirectToAction("DanhSachDanhGia");
        }
    }
}
