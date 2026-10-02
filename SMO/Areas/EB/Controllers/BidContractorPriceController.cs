using SMO.Core.Entities;
using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMO.Areas.EB.Controllers
{
    public class BidContractorPriceController : Controller
    {
        private BidContractorPriceService _service;

        public BidContractorPriceController()
        {
            _service = new BidContractorPriceService();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Index(string bidId, string isUpdate)
        {
            ViewBag.IsUpdate = isUpdate;
            _service.ObjDetail.BID_ID = bidId;
            return PartialView(_service);
        }

        [ValidateAntiForgeryToken]
        public ActionResult List(BidContractorPriceService service, string isUpdate, string isTT)
        {
            ViewBag.IsUpdate = isUpdate;
            if (service.ObjList.Count > 0 && isUpdate == "1")
            {
                service.Update(Request, isTT);
                var result = new TransferObject();
                result.Type = TransferType.AlertSuccessAndJsCommand;
                if (service.State)
                {
                    SMOUtilities.GetMessage("1002", service, result);
                    result.Data = isTT;
                }
                else
                {
                    result.Type = TransferType.AlertDanger;
                    SMOUtilities.GetMessage("1005", service, result);
                }
                return result.ToJsonResult();
            }
            var newService = new BidContractorPriceService();
            newService.ObjDetail.BID_ID = service.ObjDetail.BID_ID;
            newService.ViewId = service.ViewId;
            newService.Search();
            return PartialView(newService);

        }
    }
}