using System.Web.Mvc;

namespace SMO.Areas.PR
{
    public class PRAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PR";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "PR_default",
                "PR/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}