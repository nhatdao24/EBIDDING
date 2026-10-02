using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Areas.EB.Controllers
{
    public class BidNewController : Controller
    {
        private BidNewService _service;

        public BidNewController()
        {
            _service = new BidNewService();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Index(string id)
        {
            _service.Get(id);
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Update(BidNewService service, HttpPostedFileBase fileInput)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Update(fileInput);
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", service, result);
            }
            return result.ToJsonResult();
        }
    }
}