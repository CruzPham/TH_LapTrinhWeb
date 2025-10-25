using System.Web.Mvc;

namespace PhamVanTung.SachOnline.Areas.PhamVanTung_Admin
{
    public class PhamVanTung_AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PhamVanTung_Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "PhamVanTung_Admin_default",
                "PhamVanTung_Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}