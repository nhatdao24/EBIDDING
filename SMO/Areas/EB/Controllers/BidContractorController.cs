using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
 
namespace SMO.Areas.EB.Controllers
{
    public class BidContractorController : Controller
    {
        private BidContractorService _service;

        public BidContractorController()
        {
            _service = new BidContractorService();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Index(string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            return PartialView(_service);
        }

        [ValidateAntiForgeryToken]
        public ActionResult List(BidContractorService service)
        {
            service.Search();
            return PartialView(service);
        }

        [MyValidateAntiForgeryToken]
        public ActionResult SelectContractor(string bidId)
        {
            _service.ObjDetail.BID_ID = bidId;
            return PartialView(_service);
        }

        [ValidateAntiForgeryToken]
        public ActionResult ListDebutContractor(BidContractorService service)
        {
            service.Search();
            return PartialView(service);
        }


        public ActionResult ListFileInSelectContractor(string bidId)
        {
            var service = new BidPhaseFileService();
            service.ObjDetail.BID_ID = bidId;
            service.Search();
            return PartialView(service);
        }

        public ActionResult AddFileInSelectContractor(string bidId)
        {
            var service = new BidPhaseFileService();
            service.ObjDetail.BID_ID = bidId;
            return PartialView(service);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitFileInSelectContractor(BidPhaseFileService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.CreateFileInSelectContractor(Request);
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = "SubmitListFile();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [MyValidateAntiForgeryToken]
        public ActionResult DeleteContractorOfBid(string lstContractor)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.Delete(lstContractor);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1003", _service, result);
                result.ExtData = "SubmitListContractorOfBid('0');";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1006", _service, result);
            }
            return result.ToJsonResult();
        }

        [MyValidateAntiForgeryToken]
        public ActionResult Comment(string bidId, string viewId)
        {
            _service.ObjDetail.BID_ID = bidId;
            _service.ViewId = viewId;
            return PartialView(_service);
        }

        [HttpPost, ValidateInput(false)]
        [MyValidateAntiForgeryToken]
        public ActionResult Comment(string bidId, string viewId, string content)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.Comment(bidId, content);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = "$('#btn_close_modal_comment_bid').click();$('#mnuChonNhaThau').click()";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }
    }
}