using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhamVanTung.SachOnline.Models;

namespace PhamVanTung.SachOnline.Controllers
{
    public class PhamVanTungSachOnlineController : Controller
    {
        // Kết nối đến database
        SachOnlineEntities data = new SachOnlineEntities();

        /// <summary>
        /// Lấy sách mới theo số lượng chỉ định
        /// </summary>
        /// <param name="count">số lượng sách</param>
        /// <returns>List<SACH></returns>
        private List<SACH> LaySachMoi(int count)
        {
            return data.SACHes
                       .OrderByDescending(s => s.NgayCapNhat)
                       .Take(count)
                       .ToList();
        }

        // GET: Index
        public ActionResult Index()
        {
            var sachMoi = LaySachMoi(6);

            if (sachMoi.Count == 0)
            {
                ViewBag.ThongBao = "Chua co sach moi trong co so du lieu.";
            }

            return View(sachMoi);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Cart()
        {
            return RedirectToAction("GioHang", "PhamVanTung_GioHang");
        }

        public ActionResult NavPartial()
        {
            return PartialView();
        }

        public ActionResult SliderPartial()
        {
            return PartialView();
        }

        public ActionResult ChuDePartial()
        {
            return PartialView();
        }

        public ActionResult NhaXuatBanPartial()
        {
            return PartialView();
        }

        public ActionResult SachBanNhieuPartial()
        {
            return PartialView();
        }

        public ActionResult FooterPartial()
        {
            return PartialView();
        }
    }
}
