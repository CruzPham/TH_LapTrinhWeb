using System.Linq;
using System.Web.Mvc;
using PhamVanTung.SachOnline.Models;

namespace PhamVanTung.SachOnline.Areas.PhamVanTung_Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly SachOnlineEntities db = new SachOnlineEntities();

        // GET: Admin/Home/Login
        [HttpGet]
        public ActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl; // để quay về trang trước nếu cần
            return View();
        }

        // POST: Admin/Home/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string UserName, string Password, string returnUrl = null)
        {
            var ad = db.ADMINs.SingleOrDefault(a => a.TenDN == UserName && a.MatKhau == Password);
            if (ad != null)
            {
                Session["Admin"] = ad;

                // quay lại trang cũ nếu được truyền vào
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index");
            }

            ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
            return View();
        }

        // Đăng xuất
        public ActionResult Logout()
        {
            Session["Admin"] = null;
            return RedirectToAction("Login");
        }

        // Trang chủ Admin
        public ActionResult Index()
        {
            if (Session["Admin"] == null)
                return RedirectToAction("Login");

            ViewBag.CountSach = db.SACHes.Count();
            ViewBag.CountChuDe = db.CHUDEs.Count();
            ViewBag.CountKhachHang = db.KHACHHANGs.Count();
            ViewBag.CountDonHang = db.DONDATHANGs.Count();

            return View();
        }

    }
}
