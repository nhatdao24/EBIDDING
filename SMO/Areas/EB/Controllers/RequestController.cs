using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Areas.EB.Controllers
{
    public class RequestController : Controller
    {
        private RequestService _service;

        public RequestController()
        {
            _service = new RequestService();
        }

        [AuthorizeCustom(Right = "R501")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index()
        {
            _service.ObjDetail.STATUS = EB_Status.KhoiTao;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R501")]
        [ValidateAntiForgeryToken]
        public ActionResult List(RequestService service)
        {
            service.Search();
            return PartialView(service);
        }

        [Authorize]
        public ActionResult Detail(string id)
        {
            _service.Get(id);
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R503")]
        public ActionResult Reject(string id, string comment)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            
            _service.Reject(id, comment);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }
    }
}