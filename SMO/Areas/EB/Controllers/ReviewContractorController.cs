using SMO.Service.EB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SMO.Areas.EB.Controllers
{
    public class ReviewContractorController : Controller
    {
        private ReviewContractorService _service;

        public ReviewContractorController()
        {
            _service = new ReviewContractorService();
        }

        [AuthorizeCustom(Right = "R403")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index(string reviewId)
        {
            _service.ObjDetail.REVIEW_ID = reviewId;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R403")]
        [ValidateAntiForgeryToken]
        public ActionResult List(ReviewContractorService service)
        {
            service.Search();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R403")]
        [MyValidateAntiForgeryToken]
        public ActionResult AddDetailReviewContractor(string headerId, string contractorId, string reviewId)
        {
            _service.ObjReviewContractorDetail.HEADER_ID = headerId;
            _service.ObjDetail.CONTRACTOR_ID = contractorId;
            _service.ObjDetail.REVIEW_ID = reviewId;
            return PartialView(_service);
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R403")]
        [ValidateAntiForgeryToken]
        public ActionResult AddDetailReviewContractor(ReviewContractorService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.AddDetailReviewContractor();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = "SubmitListContractorOfReview();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R403")]
        [MyValidateAntiForgeryToken]
        public ActionResult EditDetailReviewContractor(string detailId, string contractorId, string reviewId)
        {
            _service.GetDetailReviewContractor(detailId);
            _service.ObjDetail.CONTRACTOR_ID = contractorId;
            _service.ObjDetail.REVIEW_ID = reviewId;
            return PartialView(_service);
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R403")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateDetailReviewContractor(ReviewContractorService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.UpdateDetailReviewContractor();
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = "SubmitListContractorOfReview();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R403")]
        [MyValidateAntiForgeryToken]
        public ActionResult DeleteDetailReviewContractor(string id)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.DeleteDetailReviewContractor(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1003", _service, result);
                result.ExtData = "SubmitListContractorOfReview();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1006", _service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R403")]
        [MyValidateAntiForgeryToken]
        public ActionResult AddBid(string contractorId, string reviewId)
        {
            _service.ObjDetail.CONTRACTOR_ID = contractorId;
            _service.ObjDetail.REVIEW_ID = reviewId;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R403")]
        [ValidateAntiForgeryToken]
        public ActionResult ListBidForAdd(ReviewContractorService service)
        {
            service.SearchBid();
            return PartialView(service);
        }

        [AuthorizeCustom(Right = "R403")]
        [MyValidateAntiForgeryToken]
        public ActionResult AddMaterialType()
        {
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R403")]
        [ValidateAntiForgeryToken]
        public ActionResult ListMaterialTypeForAdd(ReviewContractorService service)
        {
            service.SearchMaterialType();
            return PartialView(service);
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R403")]
        [MyValidateAntiForgeryToken]
        public ActionResult DeleteContractorOfReview(string id)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.Delete(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1003", _service, result);
                result.ExtData = "SubmitListContractorOfReview();";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1006", _service, result);
            }
            return result.ToJsonResult();
        }
    }
}