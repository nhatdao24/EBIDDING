using SMO.Service.AD;
using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SMO.Areas.EB.Controllers
{
    public class BidCouncilController : Controller
    {
        private BidCouncilService _service;

        public BidCouncilController()
        {
            _service = new BidCouncilService();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Index(string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            return PartialView(_service);
        }

        [ValidateAntiForgeryToken]
        public ActionResult List(BidCouncilService service, string isUpdate)
        {
            if (service.ObjList.Count > 0 && isUpdate == "1")
            {
                service.Update();
                var result = new TransferObject();
                result.Type = TransferType.AlertSuccessAndJsCommand;
                if (_service.State)
                {
                    SMOUtilities.GetMessage("1002", _service, result);
                }
                else
                {
                    result.Type = TransferType.AlertDanger;
                    SMOUtilities.GetMessage("1005", _service, result);
                }
                ViewBag.Message = result;
            }
            var newService = new BidCouncilService();
            newService.ObjDetail.BID_ID = service.ObjDetail.BID_ID;
            newService.ViewId = service.ViewId;
            newService.Search();
            return PartialView(newService);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult AddCouncil(string bidId)
        {
            var service = new UserService();
            ViewBag.BidId = bidId;
            return PartialView(service);
        }

        [ValidateAntiForgeryToken]
        public ActionResult ListCouncilForAdd(UserService service, string bidId)
        {
            service.SearchForAddCouncilBid(bidId);
            return PartialView(service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult AddCouncilToBid(string bidId, string lstCouncil)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.AddCouncilToBid(bidId, lstCouncil);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = "SubmitListCouncilForAdd(); SubmitListCouncilOfBid();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult DeleteCouncilOfBid(string lstCouncil)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.Delete(lstCouncil);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1003", _service, result);
                result.ExtData = "SubmitListCouncilOfBid('0');";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1006", _service, result);
            }
            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Update(BidCouncilService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Update();
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }
    }
}