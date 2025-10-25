using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using PhamVanTung.SachOnline.Models;
using PhamVanTung.SachOnline.Services;

namespace PhamVanTung.SachOnline.Controllers
{
    public class PhamVanTung_GioHangController : Controller
    {
        private readonly SachOnlineEntities db = new SachOnlineEntities();

        // ===== CART HELPERS =====
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

        // ===== CART ACTIONS =====
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
            {
                return Redirect(strURL);
            }

            return RedirectToAction("Index", "PhamVanTungSachOnline");
        }

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

        public ActionResult XoaSPKhoiGioHang(int iMaSach)
        {
            var lst = LayGioHang();
            var sp = lst.SingleOrDefault(n => n.iMaSach == iMaSach);
            if (sp != null)
            {
                lst.Remove(sp);
                Session["GioHang"] = lst;
            }
            return RedirectToAction("GioHang");
        }

        public ActionResult XoaGioHang()
        {
            Session["GioHang"] = null;
            return RedirectToAction("Index", "PhamVanTungSachOnline");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatGioHang(int iMaSach, FormCollection f)
        {
            var lst = LayGioHang();
            var sp = lst.SingleOrDefault(n => n.iMaSach == iMaSach);
            if (sp != null)
            {
                var soLuongText = f["txtSoLuong"];
                if (int.TryParse(soLuongText, out var slMoi) && slMoi > 0)
                {
                    sp.iSoLuong = slMoi;
                }
            }
            Session["GioHang"] = lst;
            return RedirectToAction("GioHang");
        }

        public ActionResult Index() => RedirectToAction("GioHang");

        [ChildActionOnly]
        public ActionResult GioHangPartial()
        {
            var lst = Session["GioHang"] as List<GIOHANG> ?? new List<GIOHANG>();
            ViewBag.TongSoLuong = lst.Sum(x => x.iSoLuong);
            ViewBag.TongTien = lst.Sum(x => x.dThanhTien);
            return PartialView("~/Views/PhamVanTung_GioHang/GioHangPartial.cshtml", lst);
        }

        // ===== CHECKOUT =====
        [HttpGet]
        public ActionResult DatHang()
        {
            if (Session["TaiKhoan"] == null && !User.Identity.IsAuthenticated)
            {
                var returnUrl = Url.Action("DatHang", "PhamVanTung_GioHang");
                return RedirectToAction("DangNhap", "User", new { returnUrl });
            }

            var lst = LayGioHang();
            if (lst == null || lst.Count == 0)
            {
                return RedirectToAction("GioHang");
            }

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
            {
                return RedirectToAction("GioHang");
            }

            string loiNgayGiao = null;
            if (!NgayGiao.HasValue)
            {
                loiNgayGiao = "Vui long chon ngay giao hang!";
            }
            else if (NgayGiao.Value.Date < DateTime.Today)
            {
                loiNgayGiao = "Ngay giao phai tu hom nay tro di!";
            }

            if (loiNgayGiao != null)
            {
                ViewBag.LoiNgayGiao = loiNgayGiao;
                ViewBag.KhachHang = kh;
                ViewBag.NgayDat = DateTime.Now;
                ViewBag.TongTien = TongTien();
                return View("DatHang", lst);
            }

            var dh = new DONDATHANG
            {
                MaKH = kh.MaKH,
                NgayDat = DateTime.Now,
                NgayGiao = NgayGiao.Value.Date
            };
            db.DONDATHANGs.Add(dh);
            db.SaveChanges();

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

            try
            {
                EmailService.SendOrderConfirmation(kh, dh, lst);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Send mail error: " + ex.Message);
            }

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
