using SMO.Service.EB;
using SMO.Service.MD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SMO.Areas.EB.Controllers
{
    public class ReviewSpecController : Controller
    {
        private ReviewSpecService _service;

        public ReviewSpecController()
        {
            _service = new ReviewSpecService();
        }

        [AuthorizeCustom(Right = "R902")]
        [MyValidateAntiForgeryToken]
        public ActionResult Index(string reviewId)
        {
            _service.ObjDetail.REVIEW_ID = reviewId;
            return PartialView(_service);
        }

        [AuthorizeCustom(Right = "R902")]
        [MyValidateAntiForgeryToken]
        public ActionResult BuildTree(string specSelected, string reviewId)
        {
            _service.ObjDetail.REVIEW_ID = reviewId;
            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            var lstNodeSpec = _service.BuildTreeSpec();

            ViewBag.TongTrongSo = _service.ObjList.Sum(x => x.RATIO);
            ViewBag.zNodeSpec = oSerializer.Serialize(lstNodeSpec);
            ViewBag.SpecSelected = specSelected;
            return PartialView();
        }

        [AuthorizeCustom(Right = "R902")]
        public ActionResult AddSpecMD(string reviewId)
        {
            var service = new MdReviewSpecService();
            var lstNodeSpec = service.BuildTreeSpec(reviewId);
            JavaScriptSerializer oSerializer = new JavaScriptSerializer();
            ViewBag.zNodeSpec = oSerializer.Serialize(lstNodeSpec);
            ViewBag.ReviewId = reviewId;
            return PartialView(service);
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R902")]
        [MyValidateAntiForgeryToken]
        public ActionResult AddSpecMDToReview(string lstSpec, string reviewId)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.AddSpecMDToBid(lstSpec, reviewId);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "BuildTreeSpec('', true);";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R902")]
        [MyValidateAntiForgeryToken]
        public ActionResult UpdateTree(List<NodeReviewSpec> lstNode)
        {
            var result = new TransferObject();
            result.State = true;
            result.Type = TransferType.AlertSuccessAndJsCommand;
            _service.UpdateTree(lstNode);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1002", _service, result);
                result.ExtData = "";
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", _service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R902")]
        [MyValidateAntiForgeryToken]
        public ActionResult Create(string parent, string reviewId)
        {
            _service.ObjHeader.PARENT_ID = parent;
            _service.ObjHeader.REVIEW_ID = reviewId;
            return PartialView(_service);
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R902")]
        [MyValidateAntiForgeryToken]
        public ActionResult Create(ReviewSpecService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Create();
            if (service.State)
            {
                SMOUtilities.GetMessage("1001", service, result);
                result.ExtData = string.Format("BuildTreeSpec('{0}', true);Forms.CloseRightContent();", service.ObjDetail.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1004", service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R902")]
        [MyValidateAntiForgeryToken]
        public ActionResult Edit(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                _service.Get(id);
            }
            return PartialView(_service);
        }


        [AuthorizeCustom(Right = "R902")]
        [MyValidateAntiForgeryToken]
        public ActionResult Update(ReviewSpecService service)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            service.Update();
            if (service.State)
            {
                SMOUtilities.GetMessage("1002", service, result);
                result.ExtData = string.Format("BuildTreeSpec('{0}', true);Forms.CloseRightContent();", service.ObjHeader.PKID);
            }
            else
            {
                result.Type = TransferType.AlertDanger;
                SMOUtilities.GetMessage("1005", service, result);
            }
            return result.ToJsonResult();
        }

        [AuthorizeCustom(Right = "R902")]
        [MyValidateAntiForgeryToken]
        public ActionResult Preview(string reviewId)
        {
            _service.ObjDetail.REVIEW_ID = reviewId;
            _service.Search();
            return PartialView(_service);
        }

        [HttpPost]
        [AuthorizeCustom(Right = "R902")]
        [MyValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            var result = new TransferObject();
            result.Type = TransferType.AlertSuccessAndJsCommand;
            result.State = true;
            _service.Delete(id);
            if (_service.State)
            {
                SMOUtilities.GetMessage("1003", _service, result);
                result.ExtData = string.Format("BuildTreeSpec('', true);");
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