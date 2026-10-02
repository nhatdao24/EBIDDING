using SMO.Service.AD;
using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Controllers
{
    public class IndexController : Controller
    {
        private BidService _service;

        public IndexController()
        {
            _service = new BidService();
        }

        public ActionResult Index()
        {
            var countVisit = new CountVisitService();
            countVisit.ObjDetail.ACCESS_TYPE = Request.Browser.Browser;
            countVisit.ObjDetail.VIEW_TYPE = "PK";
            countVisit.ObjDetail.IS_MOBILE = Request.Browser.IsMobileDevice;
            countVisit.ObjDetail.CREATE_DATE = DateTime.Now;
            countVisit.ObjDetail.CREATE_BY = ProfileUtilities.User?.USER_NAME;
            countVisit.Create();
            return View(_service);
        }

        public ActionResult Test()
        {
            return View();
        }

        public ActionResult SearchPortal(BidService service)
        {
            service.SearchPortal();
            return PartialView(service);
        }

        public ActionResult ChiTiet(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                dynamic param = new ExpandoObject();
                param.IsFetch_ListNew = true;
                _service.Get(id, param);
            }

            return View(_service);
        }
    }
}