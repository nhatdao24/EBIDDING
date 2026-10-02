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
    public class ReviewCouncilController : Controller
    {
        private ReviewCouncilService _service;

        public ReviewCouncilController()
        {
            _service = new ReviewCouncilService();
        }

        [AuthorizeCustom(Right = "R903")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index(string reviewId)
        {
            _service.ObjDetail.REVIEW_ID = reviewId;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R903")]
        [ValidateAntiForgeryToken]
        public ActionResult List(ReviewCouncilService service, string isUpdate)
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
            var newService = new ReviewCouncilService();
            newService.ObjDetail.REVIEW_ID = service.ObjDetail.REVIEW_ID;
            newService.ViewId = service.ViewId;
            newService.Search();
            return PartialView(newService);
        }

        [AuthorizeCustom(Right = "R903")]
        [MyValidateAntiForgeryToken]
        public ActionResult AddCouncil(string reviewId)
        {
            var service = new UserService();
            ViewBag.ReviewId = reviewId;
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R903")]
        [ValidateAntiForgeryToken]
        public ActionResult ListCouncilForAdd(UserService service, string reviewId)
        {
            service.SearchForAddCouncilReview(reviewId);
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R903")]
        [MyValidateAntiForgeryToken]
        public ActionResult AddCouncilToReview(string reviewId, string lstCouncil)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.AddCouncilToReview(reviewId, lstCouncil);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = "SubmitListCouncilForAdd(); SubmitListCouncilOfReview();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R903")]
        [MyValidateAntiForgeryToken]
        public ActionResult DeleteCouncilOfReview(string lstCouncil, string reviewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.Delete(lstCouncil, reviewId);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1003", _service, result);
                result.ExtData = "SubmitListCouncilOfReview('0');";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1006", _service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R903")]
        [MyValidateAntiForgeryToken]
        public ActionResult Update(ReviewCouncilService service)
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

        [HttpPost]
        [AuthorizeCustom(Right = "R903")]
        [MyValidateAntiForgeryToken]
        public ActionResult SendMailCouncilOfReview2(string id, string lstSelected)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.CreateNotifyCouncil(id, lstSelected);
            _service.CreateNotiEmail(id, lstSelected);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", _service, result);
                result.ExtData = "SubmitListCouncilOfReview('0');";
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