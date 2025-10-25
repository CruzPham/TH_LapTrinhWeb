using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
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
        public ActionResult Index(int? page)
        {
            const int pageSize = 6;
            int pageNumber = page ?? 1;

            var sachMoi = data.SACHes
                              .OrderByDescending(s => s.NgayCapNhat)
                              .ToPagedList(pageNumber, pageSize);

            if (!sachMoi.Any())
            {
                ViewBag.ThongBao = "Chua co sach moi trong co so du lieu.";
            }

            return View(sachMoi);
        }

        public ActionResult ChiTietSach(int id)
        {
            var sach = data.SACHes.SingleOrDefault(s => s.MaSach == id);
            if (sach == null)
            {
                return HttpNotFound();
            }

            return View(sach);
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
            var chuDe = data.CHUDEs.OrderBy(cd => cd.TenChuDe).ToList();
            return PartialView(chuDe);
        }

        public ActionResult NhaXuatBanPartial()
        {
            var nxb = data.NHAXUATBANs.OrderBy(x => x.TenNXB).ToList();
            return PartialView(nxb);
        }

        public ActionResult SachBanNhieuPartial()
        {
            return PartialView();
        }

        public ActionResult FooterPartial()
        {
            return PartialView();
        }

        public ActionResult SachTheoChuDe(int id, int? page)
        {
            const int pageSize = 6;
            int pageNumber = page ?? 1;

            var chuDe = data.CHUDEs.SingleOrDefault(cd => cd.MaCD == id);
            if (chuDe == null)
            {
                return HttpNotFound();
            }

            ViewBag.TenChuDe = chuDe.TenChuDe;
            ViewBag.ChuDeId = id;

            var sachTheoChuDe = data.SACHes
                                   .Where(s => s.MaCD == id)
                                   .OrderBy(s => s.MaSach);

            return View(sachTheoChuDe.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult SachTheoNXB(int id, int? page)
        {
            const int pageSize = 6;
            int pageNumber = page ?? 1;

            var nxb = data.NHAXUATBANs.SingleOrDefault(x => x.MaNXB == id);
            if (nxb == null)
            {
                return HttpNotFound();
            }

            ViewBag.TenNXB = nxb.TenNXB;
            ViewBag.NxbId = id;

            var sachTheoNxb = data.SACHes
                                  .Where(s => s.MaNXB == id)
                                  .OrderBy(s => s.MaSach);

            return View(sachTheoNxb.ToPagedList(pageNumber, pageSize));
        }
    }
}
