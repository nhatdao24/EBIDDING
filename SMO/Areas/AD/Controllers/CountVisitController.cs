using SMO.Service.AD;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SMO.Areas.AD.Controllers
{
    [AuthorizeCustom(Right = "R180")]
    public class CountVisitController : Controller
    {
        private CountVisitService _service;

        public CountVisitController()
        {
            _service = new CountVisitService();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Index()
        {
            return PartialView(_service);
        }

        public ActionResult List(CountVisitService service)
        {
            service.Search();
            return PartialView(service);
        }

        [MyValidateAntiForgeryToken]

        public ActionResult DetailCountVisit()
        {
            return PartialView(_service);
        }

        [ValidateAntiForgeryToken]
        public ActionResult Statistical(CountVisitService service)
        {
            service.Search();
            return PartialView(service);
        }

    }
}