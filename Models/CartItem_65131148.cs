using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBanPhuKienCauLong_65131148.Models
{
	public class CartItem_65131148
	{
        public int MaHangHoa { get; set; }
        public string TenHangHoa { get; set; }
        public string Anh { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public int? MaSize { get; set; } 
        public string TenSize { get; set; } 
        public List<HangHoa_Size> DanhSachSize { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}