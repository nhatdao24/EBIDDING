using SMO.Service;
using SMO.Service.MD;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Controllers
{
    public class DieuKhoanSuDungController : Controller
    {
        public DieuKhoanSuDungController()
        {
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}