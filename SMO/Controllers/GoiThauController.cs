using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Areas.Controllers
{
    public class GoiThauController : Controller
    {
        private BidService _service;

        public GoiThauController()
        {
            _service = new BidService();
        }

        public ActionResult Index()
        {
            _service.Search();
            return View(_service);
        }

        public ActionResult ChiTiet(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                dynamic param = new ExpandoObject();
                param.IsFetch_ListNew = true;
                _service.Get(id, param);
            }
            return View (_service);
        }
    }
}