using System.Linq;

namespace PhamVanTung.SachOnline.Models
{
    public class GIOHANG
    {
        private readonly SachOnlineEntities db = new SachOnlineEntities();

        public int iMaSach { get; set; }
        public string sTenSach { get; set; }
        public string sAnhBia { get; set; }
        public decimal dDonGia { get; set; }
        public int iSoLuong { get; set; }
        public decimal dThanhTien => iSoLuong * dDonGia;

        public GIOHANG(int MaSach)
        {
            iMaSach = MaSach;
            var sach = db.SACHes.Single(n => n.MaSach == iMaSach);
            sTenSach = sach.TenSach;
            sAnhBia = sach.AnhBia;      // ví dụ: "PhamVanTung.jpg"
            dDonGia = (decimal)sach.GiaBan;
            iSoLuong = 1;
        }
    }
}
