using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhamVanTung.SachOnline.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["Admin"] == null)
            {
                filterContext.Result = RedirectToAction("Login", "Home", new { area = "" });
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}
