using System.Web.Mvc;

namespace PhamVanTung.SachOnline.Areas.PhamVanTung_Admin.Controllers
{
    public class BaseAdminController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["Admin"] == null &&
                !(filterContext.ActionDescriptor.ControllerDescriptor.ControllerName == "Home"
                  && filterContext.ActionDescriptor.ActionName == "Login"))
            {
                filterContext.Result = RedirectToAction("Login", "Home", new { area = "PhamVanTung_Admin" });
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
