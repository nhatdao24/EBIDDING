using System.Web.Mvc;

namespace SMO.Areas.EB
{
    public class EBAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "EB";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "EB_default",
                "EB/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}