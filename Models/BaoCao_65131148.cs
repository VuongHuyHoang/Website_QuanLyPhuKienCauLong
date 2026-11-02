using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanPhuKienCauLong_65131148.Models
{
	public class BaoCao_65131148
	{
        // Thống kê tổng quan trong khoảng thời gian chọn
        public decimal TongDoanhThu { get; set; }
        public int TongDonHang { get; set; }
        public int SanPhamDaBan { get; set; }
        public int DonHangHuy { get; set; }

        // Dữ liệu cho Biểu đồ Doanh thu (Line Chart)
        public List<string> LabelsNgay { get; set; } // Ngày 1, Ngày 2...
        public List<decimal> DataDoanhThu { get; set; } // 1tr, 2tr...

        // Dữ liệu cho Biểu đồ Top Sản phẩm (Bar Chart)
        public List<string> LabelsSanPham { get; set; }
        public List<int> DataSoLuongBan { get; set; }

        // Bộ lọc hiện tại
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
    }
}