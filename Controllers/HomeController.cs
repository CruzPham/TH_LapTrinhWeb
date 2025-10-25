using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhamVanTung.SachOnline.Models;

namespace PhamVanTung.SachOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly SachOnlineEntities db = new SachOnlineEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["Admin"] != null)
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string UserName, string Password, bool remember = false)
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
            {
                ViewBag.ThongBao = "Vui long dien vao truong nay.";
                return View();
            }

            var admin = db.ADMINs.SingleOrDefault(a => a.TenDN == UserName && a.MatKhau == Password);
            if (admin != null)
            {
                Session["Admin"] = admin;
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            ViewBag.ThongBao = "Ten dang nhap hoac mat khau khong dung";
            return View();
        }

        public ActionResult Logout()
        {
            Session["Admin"] = null;
            return RedirectToAction("Login");
        }
    }
}
