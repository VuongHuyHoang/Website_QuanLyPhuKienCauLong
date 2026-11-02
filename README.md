# 🏸 Website Quản Lý Bán Phụ Kiện Cầu Lông - Thiên Hương Badminton

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple.svg)](https://dotnet.microsoft.com/)
[![ASP.NET MVC](https://img.shields.io/badge/ASP.NET%20MVC-5.2.9-blue.svg)](https://dotnet.microsoft.com/apps/aspnet/mvc)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-6.5.1-green.svg)](https://learn.microsoft.com/en-us/ef/ef6/)
[![SQL Server](https://img.shields.io/badge/Microsoft%20SQL%20Server-2019%2B-red.svg)](https://www.microsoft.com/sql-server)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.2.3-blueviolet.svg)](https://getbootstrap.com/)

Hệ thống Website Thương Mại Điện Tử chuyên kinh doanh dụng cụ, trang phục và phụ kiện thể thao cầu lông (vợt cầu lông, giày, áo/quần, balo, túi xách, dây cước, quấn cán,...). Dự án được xây dựng theo mô hình kiến trúc **ASP.NET MVC 5** kết hợp cơ sở dữ liệu **Microsoft SQL Server** thông qua **Entity Framework 6 Database First**.

---

## 👨‍💻 Thông Tin Tác Giả & Đề Tài

- **Sinh viên thực hiện:** Vương Huy Hoàng
- **Mã số sinh viên:** 65131148
- **Lớp:** 65.CNTT-2
- **Khoa:** Công Nghệ Thông Tin - Trường Đại học Nha Trang
- **Học phần:** Đồ Án Phát Triển Ứng Dụng Web
- **Giảng viên hướng dẫn:** ThS. Bùi Chí Thành

---

## 🚀 Các Tính Năng Nổi Bật

### 1. Phân Hệ Khách Hàng (Customer)
- **Trang chủ & Banner quảng cáo:** Hiển thị danh sách sản phẩm mới, thương hiệu đối tác, thông tin ưu đãi.
- **Duyệt & Tìm kiếm sản phẩm thông minh:**
  - Tìm kiếm theo từ khóa tên sản phẩm.
  - Lọc đa tiêu chí kết hợp: theo nhiều Hãng (Yonex, Lining, Victor...), nhiều Danh mục (Vợt, Giày, Quần áo...), khoảng giá (`Min - Max`).
  - Sắp xếp linh hoạt: Theo giá tăng/giảm dần, theo tên A-Z / Z-A.
  - Phân trang Server-side mượt mà, giữ trạng thái bộ lọc khi chuyển trang.
- **Chi tiết sản phẩm & Biến thể Size:**
  - Xem thông số kỹ thuật, hình ảnh thực tế, tình trạng tồn kho theo từng Size.
  - Tính điểm đánh giá sao trung bình.
- **Đánh giá & Nhận xét sản phẩm (Review):**
  - Khách hàng được chấm sao (1 - 5⭐) và gửi nhận xét.
  - **Ràng buộc nghiệp vụ chặt chẽ:** Chỉ những khách hàng đã đặt mua sản phẩm và đơn hàng ở trạng thái **"Hoàn thành"** mới được viết đánh giá (chống review ảo/spam).
  - Tải thêm đánh giá không cần load lại trang (AJAX + JSON).
- **Giỏ hàng (Shopping Cart):**
  - Thêm sản phẩm, cập nhật số lượng, xóa sản phẩm.
  - **Đổi size ngay trong giỏ hàng:** Nếu đổi sang size đã có sẵn trong giỏ, hệ thống tự động gộp số lượng.
  - **Chính sách phí vận chuyển tự động:** Đơn hàng từ 2.000.000 VNĐ trở lên được **Miễn phí vận chuyển (0đ)**, đơn dưới 2 triệu phí ship 30.000 VNĐ.
- **Đặt hàng an toàn (Checkout):**
  - Kiểm tra tồn kho theo từng kích cỡ trước khi trừ hàng.
  - Áp dụng **Database Transaction (`DbContextTransaction`)**, tự động Rollback nếu xảy ra lỗi hoặc thiếu hàng tồn kho.
- **Quản lý tài khoản cá nhân:**
  - Đăng ký, đăng nhập hệ thống.
  - Cập nhật thông tin họ tên, số điện thoại, địa chỉ nhận hàng mặc định.
  - Đổi mật khẩu bảo mật (kiểm tra mật khẩu hiện tại, khớp mật khẩu mới).
  - Theo dõi danh sách và trạng thái chi tiết của từng đơn hàng đã đặt (*Chờ xử lý $\rightarrow$ Đang giao $\rightarrow$ Hoàn thành / Đã hủy*).
- **Liên hệ & Góp ý:** Gửi thư liên hệ trực tiếp qua hệ thống Email SMTP.

---

### 2. Phân Hệ Quản Trị (Admin Dashboard)
- **Dashboard Tổng quan:** Thống kê doanh thu thực tế, số lượng đơn hàng, số lượng sản phẩm, tổng khách hàng và 5 đơn hàng mới nhất.
- **Quản lý Sản phẩm (CRUD):**
  - Thêm, sửa, xóa sản phẩm.
  - Upload ảnh tự động: Chuyển đổi tên danh mục thành slug thư mục (`aocaulong`, `giaycaulong`, `votcaulong`...) và xóa file ảnh cũ khi cập nhật.
- **Quản lý Tồn kho & Kích cỡ (Size):**
  - Phân loại kích cỡ theo danh mục sản phẩm (Giày dùng size số 36 - 44, Áo/Quần dùng size chữ S - XXL, Vợt/Phụ kiện không có size).
  - Quản lý số lượng tồn chi tiết cho từng size, tự động đồng bộ tổng tồn kho về sản phẩm chính.
- **Quản lý Đơn hàng:**
  - Tra cứu đơn hàng nhanh theo Mã ĐH, Tên khách hàng hoặc Số điện thoại.
  - Bộ lọc trạng thái đơn (*Chờ xử lý, Đang giao, Hoàn thành, Đã hủy*) và lọc theo khoảng thời gian (*Từ ngày - Đến ngày*).
  - Cập nhật trạng thái đơn hàng (tự động cập nhật ngày giao, ngày nhận, ngày thanh toán).
- **Quản lý Thương hiệu & Danh mục kích cỡ:** Thêm nhanh, xóa hãng sản xuất và kích cỡ.
- **Quản lý Khách hàng:** Xem danh sách, tra cứu và xem lịch sử các đơn hàng của từng khách hàng.
- **Kiểm duyệt Đánh giá:** Tra cứu phản hồi của khách, lọc theo số sao, xóa đánh giá vi phạm/tiêu cực.
- **Báo cáo Thống kê Doanh thu (Chart.js):**
  - Biểu đồ đường (Line Chart): Diễn biến doanh thu thực tế theo từng ngày trong khoảng thời gian tùy chọn.
  - Biểu đồ cột (Bar Chart): Top 5 sản phẩm bán chạy nhất cửa hàng.

---

## 🛠️ Công Nghệ Sử Dụng

- **Ngôn ngữ:** C# (.NET Framework 4.7.2)
- **Framework:** ASP.NET MVC 5.2.9
- **ORM:** Entity Framework 6.5.1 (Database First)
- **Cơ sở dữ liệu:** Microsoft SQL Server 2014 / 2019 / 2022
- **Giao diện & UI:**
  - Razor View Engine (`.cshtml`)
  - Bootstrap 5.2.3 & Bootstrap Icons
  - Custom Responsive CSS (`Content/Main.css`, `Content/AdminStyle.css`)
- **Scripting & Thư viện:**
  - jQuery 3.7.0 & jQuery Validation Unobtrusive
  - Chart.js (Vẽ biểu đồ thống kê)
  - Newtonsoft.Json (Xử lý dữ liệu JSON)
  - System.Net.Mail (Dịch vụ gửi mail SMTP)

---

## 🗄️ Cấu Trúc Cơ Sở Dữ Liệu

Cơ sở dữ liệu bao gồm **10 bảng** quan hệ:
1. `DanhMuc`: Danh mục ngành hàng (Vợt, Giày, Quần, Áo, Balo, Phụ kiện...).
2. `Hang`: Hãng sản xuất (Yonex, Victor, Lining, Mizuno, VNB...).
3. `HangHoa`: Thông tin sản phẩm, đơn giá, hình ảnh, mô tả, tồn kho.
4. `Size`: Định nghĩa kích cỡ (Số hoặc Chữ).
5. `HangHoa_Size`: Quan hệ N - N giữa Hàng hóa và Kích cỡ, lưu số lượng tồn kho theo từng size.
6. `KhachHang`: Thông tin tài khoản và liên hệ của khách hàng.
7. `NhanVien`: Tài khoản nhân viên / quản trị viên.
8. `DonHang`: Thông tin đơn đặt hàng, trạng thái, phí ship, địa chỉ giao nhận.
9. `ChiTietDonHang`: Các mặt hàng, số lượng, đơn giá và size được mua trong đơn hàng.
10. `DanhGiaSanPham`: Đánh giá 1-5 sao và bình luận của khách hàng về sản phẩm.

---

## 📂 Cấu Trúc Thư Mục Dự Án

```
WebsiteBanPhuKienCauLong_65131148/
├── App_Data/                           # Thư mục dữ liệu ứng dụng
├── App_Start/                          # Cấu hình Bundle, Filter, RouteConfig
├── Content/                            # File CSS, Bootstrap, Style giao diện
├── Controllers/                        # Bộ điều khiển Controller
│   ├── Admin_65131148Controller.cs     # Nghiệp vụ quản trị Admin
│   ├── HangHoas_65131148Controller.cs  # Nghiệp vụ khách hàng, mua sắm, giỏ hàng
│   └── TaiKhoan_65131148Controller.cs  # Nghiệp vụ đăng ký, đăng nhập, hồ sơ
├── Database/                           # Chứa script CSDL SQL Server
│   └── QuanLyBanPhuKienCauLong_65131148.sql
├── Images/                             # Hình ảnh sản phẩm phân theo danh mục
├── Models/                             # Entity Framework EDMX và ViewModels
│   ├── Model1_65131148.edmx            # Sơ đồ CSDL Entity Framework
│   ├── CartItem_65131148.cs            # ViewModel Giỏ hàng
│   └── BaoCao_65131148.cs              # ViewModel Báo cáo thống kê
├── Scripts/                            # Thư viện JavaScript, jQuery, Bootstrap
├── Views/                              # Giao diện Razor Pages (.cshtml)
│   ├── Admin_65131148/                 # Các view của quản trị viên
│   ├── HangHoas_65131148/              # Các view của khách hàng
│   ├── TaiKhoan_65131148/              # Các view tài khoản
│   └── Shared/                         # Layout chung (_LayoutMain, _LayoutAdmin)
├── Web.config                          # Cấu hình hệ thống & Chuỗi kết nối DB
└── WebsiteBanPhuKienCauLong_65131148.sln # File Solution Visual Studio
```

---

## ⚙️ Hướng Dẫn Cài Đặt & Chạy Dự Án

### 1. Yêu Cầu Môi Trường
- **Hệ điều hành:** Windows 10/11
- **Phần mềm lập trình:** Visual Studio 2019 hoặc Visual Studio 2022 (cài đặt workload *ASP.NET and web development*)
- **Hệ quản trị CSDL:** SQL Server 2014 trở lên & SQL Server Management Studio (SSMS)
- **.NET Framework:** Phiên bản 4.7.2 trở lên

---

### 2. Cài Đặt Cơ Sở Dữ Liệu
1. Mở **SQL Server Management Studio (SSMS)** và kết nối tới máy chủ SQL Server của bạn.
2. Mở file script SQL tại đường dẫn:
   ```
   WebsiteBanPhuKienCauLong_65131148/Database/QuanLyBanPhuKienCauLong_65131148.sql
   ```
3. Nhấn **Execute (F5)** để tạo CSDL `QuanLyBanPhuKienCauLong_65131148` cùng toàn bộ bảng và dữ liệu mẫu có sẵn.

---

### 3. Cấu Hình Chuỗi Kết Nối (Connection String)
Mở file `Web.config` tại thư mục gốc của project, tìm thẻ `<connectionStrings>` và chỉnh sửa `data source` phù hợp với tên SQL Server trên máy tính của bạn:

```xml
<connectionStrings>
  <add name="QuanLyBanPhuKienCauLong_65131148Entities" 
       connectionString="metadata=res://*/Models.Model1_65131148.csdl|res://*/Models.Model1_65131148.ssdl|res://*/Models.Model1_65131148.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=TÊN_SERVER_CỦA_BẠN;initial catalog=QuanLyBanPhuKienCauLong_65131148;integrated security=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework&quot;" 
       providerName="System.Data.EntityClient" />
</connectionStrings>
```
*(Thay thế `TÊN_SERVER_CỦA_BẠN` ví dụ: `.\SQLEXPRESS` hoặc `localhost`)*

---

### 4. Khởi Chạy Ứng Dụng
1. Nhấp đúp mở file `WebsiteBanPhuKienCauLong_65131148.sln` bằng **Visual Studio**.
2. Chuột phải vào Solution $\rightarrow$ Chọn **Restore NuGet Packages** (nếu Visual Studio yêu cầu tải lại các gói thư viện).
3. Nhấn phím **F5** hoặc nút **Google Chrome / IIS Express** trên thanh công cụ để biên dịch và khởi chạy website trên trình duyệt.

---

## 🔑 Tài Khoản Thử Nghiệm

| Vai trò | Tên đăng nhập (Username) | Mật khẩu | Quyền hạn |
| :--- | :--- | :--- | :--- |
| **Quản trị viên (Admin)** | `nv1` | `123456` | Toàn quyền quản trị hệ thống, duyệt đơn hàng, thống kê doanh thu |
| **Khách hàng mẫu 1** | `khach1` | `123456` | Mua hàng, đánh giá các sản phẩm đã mua |
| **Khách hàng mẫu 2** | `khach2` | `123456` | Mua hàng, theo dõi đơn hàng |
| **Khách hàng mẫu 3** | `tantien123` | `123` | Mua hàng |

*(Bạn cũng có thể tự tạo tài khoản mới thông qua trang **Đăng ký** trên website).*

---

## 📄 Bản Quyền & Giấy Phép

Dự án được xây dựng phục vụ mục đích học tập và nghiên cứu trong khuôn khổ môn học Đồ án Phát triển Ứng dụng Web tại Trường Đại học Nha Trang. Mọi quyền tác giả thuộc về **Vương Huy Hoàng - 65131148**.
