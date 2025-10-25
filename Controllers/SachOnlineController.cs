using System.Web.Mvc;

namespace PhamVanTung.SachOnline.Controllers
{
    public class SachOnlineController : Controller
    {
        public ActionResult Index()
        {
            return View();
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
