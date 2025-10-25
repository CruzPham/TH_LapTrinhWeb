using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using PhamVanTung.SachOnline.Models;
using PhamVanTung.SachOnline.Services; // EmailService

namespace PhamVanTung.SachOnline.Controllers
{
    public class PhamVanTung_GioHangController : Controller
    {
        private readonly SachOnlineEntities db = new SachOnlineEntities();

        // ====== GIỎ HÀNG CƠ BẢN ======
        private List<GIOHANG> LayGioHang()
        {
            var lst = Session["GioHang"] as List<GIOHANG>;
            if (lst == null)
            {
                lst = new List<GIOHANG>();
                Session["GioHang"] = lst;
            }
            return lst;
        }

        // Tổng số lượng & tổng tiền
        private int TongSoLuong()
        {
            var lst = Session["GioHang"] as List<GIOHANG>;
            return lst?.Sum(n => n.iSoLuong) ?? 0;
        }

        private decimal TongTien()
        {
            var lst = Session["GioHang"] as List<GIOHANG>;
            return lst?.Sum(n => n.dThanhTien) ?? 0m;
        }

        // Thêm sản phẩm vào giỏ, sau đó quay lại URL cũ (chỉ cho phép quay về URL nội bộ)
        public ActionResult ThemGioHang(int iMaSach, string strURL)
        {
            var lst = LayGioHang();
            var sp = lst.SingleOrDefault(n => n.iMaSach == iMaSach);
            if (sp == null)
            {
                sp = new GIOHANG(iMaSach);
                lst.Add(sp);
            }
            else
            {
                sp.iSoLuong++;
            }
            Session["GioHang"] = lst;

            if (!string.IsNullOrEmpty(strURL) && Url.IsLocalUrl(strURL))
                return Redirect(strURL);

            return RedirectToAction("Index", "PhamVanTungSachOnline");
        }

        // Trang giỏ hàng
        public ActionResult GioHang()
        {
            var lst = LayGioHang();
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();

            if (User.Identity.IsAuthenticated)
            {
                var kh = db.KHACHHANGs.SingleOrDefault(x => x.TaiKhoan == User.Identity.Name);
                ViewBag.KhachHang = kh;
            }
            return View(lst);
        }

        // Xóa 1 sản phẩm khỏi giỏ
        public ActionResult XoaGioHang(int iMaSach)
        {
            var lst = LayGioHang();
            var sp = lst.SingleOrDefault(n => n.iMaSach == iMaSach);
            if (sp != null) lst.Remove(sp);
            Session["GioHang"] = lst;
            return RedirectToAction("GioHang");
        }

        // Xóa toàn bộ giỏ
        public ActionResult XoaTatCaGioHang()
        {
            Session["GioHang"] = null;
            return RedirectToAction("Index", "PhamVanTungSachOnline");
        }

        // Cập nhật số lượng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatGioHang(int iMaSach, FormCollection f)
        {
            var lst = LayGioHang();
            var sp = lst.SingleOrDefault(n => n.iMaSach == iMaSach);
            if (sp != null)
            {
                int slMoi = sp.iSoLuong;
                int.TryParse(f["txtSoLuong"], out slMoi);
                sp.iSoLuong = Math.Max(1, slMoi);
            }
            Session["GioHang"] = lst;
            return RedirectToAction("GioHang");
        }

        public ActionResult Index() => RedirectToAction("GioHang");

        // ====== MINI CART (HEADER) ======
        [ChildActionOnly]
        public ActionResult GioHangPartial()
        {
            var lst = Session["GioHang"] as List<GIOHANG> ?? new List<GIOHANG>();
            ViewBag.TongSoLuong = lst.Sum(x => x.iSoLuong);
            ViewBag.TongTien = lst.Sum(x => x.dThanhTien);
            return PartialView("~/Views/PhamVanTung_GioHang/GioHangPartial.cshtml", lst);
        }

        // ====== ĐẶT HÀNG ======
        [HttpGet]
        [Authorize] // đảm bảo phải đăng nhập
        public ActionResult DatHang()
        {
            // Chỉ redirect khi cả Session lẫn Identity đều chưa có
            if (Session["TaiKhoan"] == null && !User.Identity.IsAuthenticated)
            {
                var returnUrl = Url.Action("DatHang", "PhamVanTung_GioHang");
                return RedirectToAction("DangNhap", "User", new { returnUrl });
            }

            var lst = LayGioHang();
            if (lst == null || lst.Count == 0)
                return RedirectToAction("GioHang");

            var kh = db.KHACHHANGs.SingleOrDefault(x => x.TaiKhoan == User.Identity.Name);
            if (kh == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("DangNhap", "User");
            }

            ViewBag.KhachHang = kh;
            ViewBag.NgayDat = DateTime.Now;
            ViewBag.TongTien = TongTien();
            return View(lst);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DatHang(DateTime? NgayGiao)
        {
            var kh = db.KHACHHANGs.SingleOrDefault(x => x.TaiKhoan == User.Identity.Name);
            if (kh == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("DangNhap", "User");
            }

            var lst = LayGioHang();
            if (lst == null || lst.Count == 0)
                return RedirectToAction("GioHang");

            // Validate ngày giao và trả lỗi cạnh ô input
            string loiNgayGiao = null;
            if (!NgayGiao.HasValue)
                loiNgayGiao = "Vui lòng chọn ngày giao hàng!";
            else if (NgayGiao.Value.Date < DateTime.Today)
                loiNgayGiao = "Ngày giao phải từ hôm nay trở đi!";
            // (tuỳ chọn) giới hạn không quá 60 ngày:
            // else if (NgayGiao.Value.Date > DateTime.Today.AddDays(60))
            //     loiNgayGiao = "Ngày giao tối đa trong 60 ngày tới.";

            if (loiNgayGiao != null)
            {
                ViewBag.LoiNgayGiao = loiNgayGiao;
                ViewBag.KhachHang = kh;
                ViewBag.NgayDat = DateTime.Now;
                ViewBag.TongTien = TongTien();
                return View("DatHang", lst);
            }

            // ===== Tạo đơn hàng =====
            var dh = new DONDATHANG
            {
                MaKH = kh.MaKH,
                NgayDat = DateTime.Now,
                NgayGiao = NgayGiao.Value.Date
            };
            db.DONDATHANGs.Add(dh);
            db.SaveChanges(); // để có MaDonHang

            // Lưu chi tiết
            foreach (var item in lst)
            {
                db.CHITIETDATHANGs.Add(new CHITIETDATHANG
                {
                    MaDonHang = dh.MaDonHang,
                    MaSach = item.iMaSach,
                    SoLuong = item.iSoLuong,
                    DonGia = item.dDonGia
                });
            }
            db.SaveChanges();

            // Gửi email xác nhận (không chặn luồng nếu lỗi)
            try
            {
                EmailService.SendOrderConfirmation(kh, dh, lst);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Send mail error: " + ex.Message);
            }

            // Clear giỏ và chuyển trang xác nhận
            Session["GioHang"] = null;
            TempData["DonHang"] = dh;
            TempData["KhachHang"] = kh;
            return RedirectToAction("XacNhanDonHang");
        }

        public ActionResult XacNhanDonHang()
        {
            ViewBag.DonHang = TempData["DonHang"];
            ViewBag.KhachHang = TempData["KhachHang"];
            return View();
        }
    }
}
